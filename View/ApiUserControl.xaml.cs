using dogpixels_viewer.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace dogpixels_viewer.View
{
    /// <summary>
    /// Interaction logic for ApiUserControl.xaml
    /// </summary>
    public partial class ApiUserControl : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Configuration configuration;
        private readonly SQLiteController database;

        private readonly HttpClient client;

        private readonly DispatcherTimer timer;
        private readonly int timerPauseInterval = 1; // seconds; not configurable by user
        private List<ImageData> FileList = new();

        private JsonSerializerOptions jsonSerializerOptions;
        private int FileIndex;

        public event ApiQueryHitDelegate? ApiQueryHit;
        public delegate void ApiQueryHitDelegate(string subjectMd5, ApiResponse response);

        public event ApiQueryMissDelegate? ApiQueryMiss;
        public delegate void ApiQueryMissDelegate();

        public ApiUserControl(Configuration configuration, SQLiteController databaseContext)
        {
            InitializeComponent();
            this.configuration = configuration;
            this.database = databaseContext;

            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", $"dogpixels-viewer/{Assembly.GetExecutingAssembly().GetName().Version} (username: {configuration.ImageBoardAccount})");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            log.Info($"[API] Client initialized, User-Agent: {client.DefaultRequestHeaders.UserAgent}");

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(timerPauseInterval)
            };
            timer.Tick += Timer_Tick;

            jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();

            if (FileIndex >= FileList.Count)
            {
                log.Info($"[api] Done. {FileIndex} files were queried.'");
                return;
            }

            if (UiDisplayShowThumbnail.IsChecked == true)
            {
                BitmapImage bmp = ThumbnailController.GetThumbnail(FileList[FileIndex].path);
                UiDisplayImage.Source = bmp;
            }

            string uri = configuration.ImageBoardApi.Replace("{md5}", FileList[FileIndex].md5);

            log.Debug($"[api] #{FileIndex + 1:000000} fetching '{uri}'");
            HttpResponseMessage response = await client.GetAsync(uri);
            string json = await response.Content.ReadAsStringAsync();
            log.Debug($"[api] #{FileIndex + 1:000000} response status: {(decimal) response.StatusCode} {response.StatusCode}" + (response.IsSuccessStatusCode ? $", content: {json}" : ""));

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    ApiResponse content = JsonSerializer.Deserialize<ApiResponse>(json, jsonSerializerOptions);
                    ApiQueryHit?.Invoke(FileList[FileIndex].md5, content);
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to parse API response. Has the API changed? Details:\n{ex}");
                }
            }
            else
            {
                ApiQueryMiss?.Invoke();
            }

            FileIndex++;

            UiProgressBar.Value = FileIndex;

            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (configuration == null)
            {
                log.Error("Called ScanDir function with null Configuration");
                return;
            }

            // if scan has not run yet or was finished, start a new one
            if (FileList.Count == 0 || FileIndex + 1 >= FileList.Count)
            {
                try
                {
                    FileList = database.Get(new ImageData { tags = string.Empty });
                    UiProgressBar.Maximum = FileList.Count;
                    log.Info($"[api] Commencing api tag lookup for {FileList.Count} files with empty tags in database.");
                }

                catch (Exception ex)
                {
                    log.Error(ex);
                    return;
                }

                FileIndex = 0;
            }

            timer.IsEnabled = !timer.IsEnabled;
        }
    }
}

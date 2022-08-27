using dogpixels_viewer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    /// Interaction logic for DirectoryUserControl.xaml
    /// </summary>
    public partial class DirectoryUserControl : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Configuration configuration;

        public event ImageFoundDelegate? ImageFound;
        public delegate void ImageFoundDelegate(ImageData imageData);

        private DispatcherTimer timer;
        private int timerPauseInterval = 0; // milliseconds
        private string[] FileList = Array.Empty<string>();

        private uint FileIndex;

        public DirectoryUserControl(Configuration configuration)
        {
            this.configuration = configuration;
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(timerPauseInterval);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Stop();

            if (FileIndex >= FileList.Length)
            {
                log.Info($"[scan] Done. {FileIndex} files were found and indexed in '{configuration.RootDirectory}'");
                return;
            }

            string path = FileList[FileIndex];
            string md5 = FileMD5(path);
            log.Info($"[scan] Found file #{FileIndex + 1:000000} md5: '{md5}' path: '{path}'");
            ImageFound?.Invoke(new ImageData { path = path, md5 = md5 });

            if (UiDisplayShowThumbnail.IsChecked == true)
            {
                BitmapImage bmp = ThumbnailController.GetThumbnail(path);
                UiDisplayImage.Source = bmp;
            }
            
            FileIndex++;

            UiProgressBar.Value = FileIndex;

            timer.Start();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (configuration == null)
            {
                log.Error("Called ScanDir function with null Configuration");
                return;
            }

            // if scan has not run yet or was finished, start a new one
            if (FileList.Length == 0 || FileIndex + 1 >= FileList.Length)
            {
                try
                {
                    FileList = await Task.Run(() => Directory.GetFiles(configuration.RootDirectory, "*", SearchOption.AllDirectories));
                    UiProgressBar.Maximum = FileList.Length;
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

        static string FileMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}

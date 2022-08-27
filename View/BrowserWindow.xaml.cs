using dogpixels_viewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace dogpixels_viewer.View
{
    /// <summary>
    /// Interaction logic for BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Configuration Configuration;
        public SQLiteController database;

        private DispatcherTimer InputPauseTimer;

        public BrowserWindow(Configuration configuration, SQLiteController databaseContext)
        {
            InitializeComponent();

            Configuration = configuration;
            database = databaseContext;

            database.InitializeFTS5();

            InputPauseTimer = new DispatcherTimer();
            InputPauseTimer.Interval = TimeSpan.FromMilliseconds(1500);
            InputPauseTimer.Tick += InputPauseTimer_Tick;
        }

        private void InputPauseTimer_Tick(object? sender, EventArgs e)
        {
            InputPauseTimer.Stop();

            string query = UiSearchBar.Text;
            List<ImageData> results = database.Get(query);

            log.Debug($"database searched for '{query}', yielded {results.Count} matches.");

            UiResultListView.Items.Clear();

            UiResultsCount.Text = results.Count.ToString();

            foreach (ImageData image in results)
            {
                UiResultListView.Items.Add(new ThumbnailUserControl(image.path));
            }
        }

        private void UiSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            // merely starts measuring a short interval to prevent spamming the database; actual handle in InputPauseTimer_Tick
            InputPauseTimer.Start();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            database.DeinitializeFTS5();
            Close();
        }
    }
}

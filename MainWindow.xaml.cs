using dogpixels_viewer.Model;
using dogpixels_viewer.View;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

namespace dogpixels_viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Configuration? configuration;
        SQLiteController data;

        private int UiFilesInDatabase
        {
            get => int.Parse(_UiFilesInDatabase.Text);
            set => _UiFilesInDatabase.Text = value.ToString();
        }
        private int UiFilesTagged
        {
            get => int.Parse(_UiFilesTagged.Text);
            set => _UiFilesTagged.Text = value.ToString();
        }
        private int UiApiHits
        {
            get => int.Parse(_UiApiHits.Text);
            set => _UiApiHits.Text = value.ToString();
        }
        private int UiApiMisses
        {
            get => int.Parse(_UiApiMisses.Text);
            set => _UiApiMisses.Text = value.ToString();
        }

        public MainWindow()
        {
            log.Info($"App started, Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            InitializeComponent();

            // Config Initialization
            string lastconfig = ConfigurationFactory.GetLastProfileName();
            
            if (lastconfig == null)
            {
                lastconfig = "default";
                log.Info($"No previous configuration determined, opening Configuration Window for profile '{lastconfig}'");
                OpenConfigWindow(new Configuration(lastconfig));
            }

            configuration = ConfigurationFactory.Read(lastconfig);
            if (configuration == null)
            {
                log.Warn("Failed to initialize configuration; stopping.");
                while(true) { }
            }

            // Data Initialization
            data = new SQLiteController(configuration.ProfileName); 
            data.Initialize();

            DirectoryUserControl directoryUserControl = new DirectoryUserControl(configuration);
            directoryUserControlSlot.Child = directoryUserControl;
            directoryUserControl.ImageFound += DirectoryUserControl_ImageFound;

            ApiUserControl apiUserControl = new ApiUserControl(configuration, data);
            apiUserControlSlot.Child = apiUserControl;
            apiUserControl.ApiQueryHit += ApiUserControl_ApiQueryHit;
            apiUserControl.ApiQueryMiss += ApiUserControl_ApiQueryMiss;


            browserUserControlSlot.Child = new BrowserUserControl(configuration, data);

            // initialize bottom stats
            int totalFiles = data.Count(new ImageData());
            int untaggedFiles = data.Count(new ImageData { tags = string.Empty });
            UiFilesInDatabase = totalFiles;
            UiFilesTagged = totalFiles - untaggedFiles;
        }


        private void ApiUserControl_ApiQueryHit(string subjectMd5, ApiResponse response)
        {
            UiApiHits++;
            if (data.UpdateTags(subjectMd5, response.getAllTags()))
            {
                UiFilesTagged++;
            }
        }
        private void ApiUserControl_ApiQueryMiss()
        {
            UiApiMisses++;
        }

        private void DirectoryUserControl_ImageFound(ImageData imageData)
        {
            if (data.Insert(imageData.path, imageData.md5))
            {
                UiFilesInDatabase++;
            }
        }

        private void OpenConfigWindow(Configuration config = null)
        {
            if (config == null)
            {
                config = configuration;
            }

            ConfigurationWindow cw = new ConfigurationWindow(config);
            bool? result = cw.ShowDialog();
            if (result == true)
            {
                configuration = cw.Configuration;
                log.Info($"Applied configuration profile '{configuration.ProfileName}' from Configuration Window");
                ConfigurationFactory.Write(configuration);
                ConfigurationFactory.SetLastProfileName(configuration.ProfileName);
            }
            else
            {
                log.Debug($"Closed Configuration Window with no positive result");
            }
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            new AttributionsWindow().Show();
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigWindow(configuration);
        }
    }
}

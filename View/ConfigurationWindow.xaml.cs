using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dogpixels_viewer
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Configuration Configuration;

        public ConfigurationWindow(Configuration config)
        {
            this.Configuration = config;
            InitializeComponent();

            ProfileName.Text = config.ProfileName;
            RootDirectory.Text = config.RootDirectory;
            ImageBoardApi.Text = config.ImageBoardApi;
            ImageBoardAccount.Text = config.ImageBoardAccount;
            StoreTagsInFiles.IsChecked = config.StoreTagsInFiles;
        }

        /// <summary>
        /// Tries to parse all settings to a Configuration object, closes the dialog and signals positive dialog result.
        /// If parsing fails, an error message will be displayed instead, without closing the dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Configuration.ProfileName = ProfileName.Text;
                Configuration.RootDirectory = RootDirectory.Text;
                Configuration.ImageBoardApi = ImageBoardApi.Text;
                Configuration.ImageBoardAccount = ImageBoardAccount.Text;
                Configuration.StoreTagsInFiles = StoreTagsInFiles.IsChecked == true? true : false;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                log.Error("Failed to save settings:");
                log.Error(ex);
                System.Windows.MessageBox.Show("Failed to save settings.", "Cannot Save Settings", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Closes the dialog and signals a negative dialog result.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dialog.SelectedPath != null)
                {
                    Configuration.RootDirectory = dialog.SelectedPath;
                    RootDirectory.Text = dialog.SelectedPath;
                }
            }
        }
    }
}

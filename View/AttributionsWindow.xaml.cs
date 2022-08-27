using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace dogpixels_viewer
{
    /// <summary>
    /// Interaction logic for AttributionsWindow.xaml
    /// </summary>
    public partial class AttributionsWindow : Window
    {
        public AttributionsWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as Hyperlink).Name) 
            {
                case "log4net_website": open("https://logging.apache.org/log4net/"); break;
                case "log4net_license": open("https://logging.apache.org/log4net/license.html"); break;
                case "ookii_website": open("https://github.com/ookii-dialogs/ookii-dialogs-wpf"); break;
                case "ookii_license": open("https://github.com/ookii-dialogs/ookii-dialogs-wpf/blob/master/LICENSE"); break;
                case "sqlite_website": open("https://www.sqlite.org/"); break;
                case "sqlite_license": open("https://www.sqlite.org/copyright.html"); break;
                case "mssqlite_website": open("https://docs.microsoft.com/dotnet/standard/data/sqlite/"); break;
                case "mssqlite_license": open("https://github.com/dotnet/efcore/blob/main/LICENSE.txt"); break;
            }
        }

        private void open(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

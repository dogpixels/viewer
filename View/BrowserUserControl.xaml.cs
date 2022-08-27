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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dogpixels_viewer.View
{
    /// <summary>
    /// Interaction logic for BrowserUserControler.xaml
    /// </summary>
    public partial class BrowserUserControl : UserControl
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Configuration configuration;
        private readonly SQLiteController database;
        
        public BrowserUserControl(Configuration configuration, SQLiteController databaseContext)
        {
            InitializeComponent();
            this.configuration = configuration;
            this.database = databaseContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new BrowserWindow(configuration, database).Show();
        }
    }
}

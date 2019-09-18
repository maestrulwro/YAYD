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

namespace YAYD
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MaxWorkUnitsInScheduler = int.Parse(MaxWorkUnitsInScheduler.Text);
            Properties.Settings.Default.IncludeThumbnailInAdHocDownloadAndConvert = (bool)IncludeThumbnailInAdHocDownloadAndConvert.IsChecked;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void DefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            this.Close();
        }
        private void LoadSettings()
        {
            MaxWorkUnitsInScheduler.Text = Properties.Settings.Default.MaxWorkUnitsInScheduler.ToString();
            IncludeThumbnailInAdHocDownloadAndConvert.IsChecked = Properties.Settings.Default.IncludeThumbnailInAdHocDownloadAndConvert;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}

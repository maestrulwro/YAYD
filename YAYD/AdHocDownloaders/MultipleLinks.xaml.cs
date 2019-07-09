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

namespace YAYD.AdHocDownloaders
{
    /// <summary>
    /// Interaction logic for MultipleLinks.xaml
    /// </summary>
    public partial class MultipleLinks : Window
    {
        Scheduler sch;
        public MultipleLinks()
        {
            InitializeComponent();
            sch = new Scheduler();
        }

        private void NewURLInsert_Click(object sender, RoutedEventArgs e)
        {
            AddNewURLDownload(NewURL.Text);
            NewURL.Text = "";
        }

        private void AddNewURLDownload(string url)
        {
            //Controls.SingleLinkInMultiWindowOld slimw = new Controls.SingleLinkInMultiWindowOld(url, "default.mp3");
            Controls.SingleLinkInMultiWindow slimw = new Controls.SingleLinkInMultiWindow(url, "default.mp3");
            ListOfDownloads.Children.Add(slimw);
            sch.Tasks.Add(slimw);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "txt",
                DereferenceLinks = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false,
                RestoreDirectory = true,
                ValidateNames = true
            };
            if (ofd.ShowDialog() != true) return;
            // else continue
            using (System.IO.StreamReader sr = new System.IO.StreamReader(ofd.FileName))
                while (!sr.EndOfStream)
                {
                    string readurl = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(readurl)) AddNewURLDownload(readurl);
                }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sch.Timer.IsEnabled = true;
        }
    }
}

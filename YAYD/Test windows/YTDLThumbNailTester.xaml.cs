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

namespace YAYD.Test_windows
{
    /// <summary>
    /// Interaction logic for YTDLThumbNailTester.xaml
    /// </summary>
    public partial class YTDLThumbNailTester : Window
    {
        public YTDLThumbNailTester()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(FinalFileLocation.Text))
            {
                W("clean_output_msg", FinalFileLocation.Text + " exists; cleaning");
                try
                {
                    System.IO.File.Delete(FinalFileLocation.Text);
                    W("clean_output_msg", FinalFileLocation.Text + " successfully deleted");
                }
                catch (Exception exc)
                {
                    W("clean_output_err", exc.ToString());
                }
            }
            else W("clean_output_msg", FinalFileLocation.Text + " does not exist");
            YTDLInteract.DownloadThumbNail dl = new YTDLInteract.DownloadThumbNail(URL.Text, FinalFileLocation.Text);
            dl.TrimmedErrorDataReceived += (obj, dt) => { W("err", ((AdvancedDataReceivedEventArgs)dt).FullData); };
            dl.TrimmedOutputDataReceived += (obj, dt) => { W("out", ((AdvancedDataReceivedEventArgs)dt).FullData); };
            dl.Finished += (obj, dt) => {
                W("Finished!");
                FinalFileLocation.Text = ((YTDLInteract.DownloadThumbNail)obj).LocationOfFinalFile;
            };
        }
        private void W(string s1, string s2)
        {
            W(s1 + ">" + s2);
        }
        private void W(string s)
        {
            Output.Text += s + Environment.NewLine;
            ((ScrollViewer)Output.Parent).ScrollToBottom();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.InitialDirectory = Environment.CurrentDirectory;
            if (sfd.ShowDialog() == true) FinalFileLocation.Text = sfd.FileName;
        }
    }
}

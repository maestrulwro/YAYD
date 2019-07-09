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
using Microsoft.Win32;

namespace YAYD.Test_windows
{
    /// <summary>
    /// Interaction logic for ManualYTDL.xaml
    /// </summary>
    public partial class YTDLDownloadTester : Window
    {
        YTDLInteract.Download dl;
        public YTDLDownloadTester()
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
            dl = new YTDLInteract.Download(URL.Text, "bestaudio", FinalFileLocation.Text);
            dl.TrimmedErrorDataReceived += (obj, dt) => { W("err", dt.FullData); };
            dl.TrimmedOutputDataReceived += (obj, dt)=>{ W("out", dt.FullData); };
            dl.ProgressReported += (obj, dt) =>
            {
                Progress.Value = dt.Percent;
                Percent.Text = dt.Percent.ToString();
                TotalSize.Text = dt.Size;
                Speed.Text = dt.Speed;
                ETA.Text = dt.Time.ToString();
            };
            dl.Finished += (obj, dt) => { W("Finished!"); };
        }
        private void W(string s)
        {
            Output.Text += s+Environment.NewLine;
            ((ScrollViewer)Output.Parent).ScrollToBottom();
        }
        private void W(string s1, string s2)
        {
            W(s1 + ">" + s2);
        }

        private void FinalFileLoc_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Environment.CurrentDirectory;
            if (sfd.ShowDialog() == true) FinalFileLocation.Text = sfd.FileName;
        }
    }
}

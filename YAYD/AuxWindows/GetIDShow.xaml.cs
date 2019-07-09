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
namespace YAYD.AuxWindows
{
    /// <summary>
    /// Interaction logic for GetIDShow.xaml
    /// </summary>
    public partial class GetIDShow : Window
    {
        Scheduler Sch;
        List<URLInList> ListOfURLs;
        public GetIDShow(string file)
        {
            InitializeComponent();
            Sch = new Scheduler(8,new TimeSpan(5000000));
            ListOfURLs = new List<URLInList>();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                while (!sr.EndOfStream)
                {
                    URLInList uil = new URLInList(sr.ReadLine());
                    uil.GID.TrimmedOutputDataReceived += GID_TrimmedOutputDataReceived;
                    uil.GID.TrimmedErrorDataReceived += GID_TrimmedErrorDataReceived;
                    uil.GID.Finished += GID_Finished;
                    ListOfURLs.Add(uil);
                    Sch.Tasks.Add(uil.GID);
                }
            ListOfURLsDG.ItemsSource = ListOfURLs;
        }
        private void GID_Finished(object sender, EventArgs e)
        {
            YTDLInteract.GetID gid = (YTDLInteract.GetID)sender;
            gid.TrimmedOutputDataReceived -= GID_TrimmedOutputDataReceived;
            gid.TrimmedErrorDataReceived -= GID_TrimmedErrorDataReceived;
            gid.Finished -= GID_Finished;
            foreach (string elem in gid.IDList)
            {
                if((!TBData.Text.Contains(elem))||(FilterData.IsChecked == false))
                    TBData.Text += elem + Environment.NewLine;
            }
        }
        private void GID_TrimmedErrorDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            TBError.Text += e.FullData + Environment.NewLine;
            if (AlwaysLastLineError.IsChecked == true)
                TBParentError.ScrollToBottom();
        }
        private void GID_TrimmedOutputDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            TBOutput.Text += e.FullData + Environment.NewLine;
            if (AlwaysLastLineOutput.IsChecked == true)
                TBParentOutput.ScrollToBottom();
        }
        private void SaveLogOutput_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "txt",
                DereferenceLinks = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                ValidateNames = true
            };
            if (sfd.ShowDialog() == true)
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    sw.Write(TBOutput.Text);
        }
        private void SaveLogError_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "txt",
                DereferenceLinks = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                ValidateNames = true
            };
            if (sfd.ShowDialog() == true)
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    sw.Write(TBError.Text);
        }
        private void SaveLogData_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "txt",
                DereferenceLinks = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                ValidateNames = true
            };
            if (sfd.ShowDialog() == true)
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                    sw.Write(TBData.Text);
        }
        public class URLInList
        {
            public enum Status
            {
                Pending,
                Running,
                Finished
            }
            public YTDLInteract.GetID GID { get; }
            public string URL { get; }
            public URLInList(string url)
            {
                URL = url;
                GID = new YTDLInteract.GetID(URL);
            }
        }
    }
}

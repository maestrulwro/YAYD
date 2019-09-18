// This file is completely commented.
// To do: change names for objects in menu.
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
        /// <summary>
        /// To be used when needed, like when showing UpdateYAYD.
        /// </summary>
        AuxWindows.JointTextShow jts = null;
        Scheduler sch;
        /// <summary>
        /// Creates a new window and its <c>Scheduler</c>.
        /// </summary>
        public MultipleLinks()
        {
            InitializeComponent(); //required
            sch = new Scheduler(); //initializes the Scheduler.
        }
        private void NewURLInsert_Click(object sender, RoutedEventArgs e)
        {
            AddNewURLDownload(NewURL.Text);
            NewURL.Text = "";
        }
        /// <summary>
        /// Inserts a new URL in the queue.
        /// </summary>
        /// <param name="url">The URL to be downloaded.</param>
        private void AddNewURLDownload(string url)
        {
            Controls.SingleLinkInMultiWindow slimw = new Controls.SingleLinkInMultiWindow(url, "default.mp3"); // creates the control that downloads the URL
            ListOfDownloads.Children.Add(slimw); // adds the control to the StackPanel that lists all URL downloaders
            sch.Tasks.Add(slimw); //adds the control to the task list of the Scheduler
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) //raised when clicking Links>Download from text file
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
            }; // initializes an OpenFileDialog to select the .txt file
            if (ofd.ShowDialog() != true) return;
            // if the user canceled, don't do anything and return
            // else continue
            using (System.IO.StreamReader sr = new System.IO.StreamReader(ofd.FileName)) //open the file stream, read until the end, and for each non empty line add a link
                while (!sr.EndOfStream)
                {
                    string readurl = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(readurl)) AddNewURLDownload(readurl);
                }
        }
        // enable the timer of the scheduler (effectively start the scheduler)
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sch.Timer.IsEnabled = true;
        }
        // Menu: update youtube-dl
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) //raised when clicked Update youtube-dl in menu
        {
            YTDLInteract.UpdateYAYD upd = new YTDLInteract.UpdateYAYD(); // create new instance
            jts = new AuxWindows.JointTextShow(); // create new console window
            upd.TrimmedErrorDataReceived += Upd_TrimmedErrorDataReceived; // send output to console
            upd.TrimmedOutputDataReceived += Upd_TrimmedOutputDataReceived; // send error to console
            upd.Finished += Upd_Finished; // cleanup after finishing with the command
            jts.Show(); // show the console window
            upd.Ready(); 
            upd.Start(); // start the update
        }

        private void Upd_Finished(object sender, EventArgs e)
        {
            YTDLInteract.UpdateYAYD upd = (YTDLInteract.UpdateYAYD)sender; // get the UpdateYAYD instance as UpdateYAYD, not as object
            jts.AppendOutput("Finished. Status reported: " + upd.Status);
            jts.AppendOutput("Do note that the status is not trustworthy!");
            upd.TrimmedErrorDataReceived -= Upd_TrimmedErrorDataReceived; // disconnect the events to let the UpdateYAYD instance die
            upd.TrimmedOutputDataReceived -= Upd_TrimmedOutputDataReceived;
            upd.Finished -= Upd_Finished;

        }

        private void Upd_TrimmedOutputDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            jts.AppendOutput(e.FullData);
        }

        private void Upd_TrimmedErrorDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            jts.AppendError(e.FullData);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
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
            }; // initializes an OpenFileDialog to select the .txt file
            if (ofd.ShowDialog() != true) return;
            // if the user canceled, don't do anything and return
            // else continue
            new AuxWindows.GetIDShow(ofd.FileName).Show();
        }
        private void MenuItem_AppSettings_Click(object sender, RoutedEventArgs e)
        {
            new Settings().Show();
        }
    }
}

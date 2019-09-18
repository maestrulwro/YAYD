// nothing to be done here
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
using System.Windows.Threading;
namespace YAYD.Controls
{
    /// <summary>
    /// Interaction logic for SingleLinkInMultiWindow.xaml
    /// </summary>
    public partial class SingleLinkInMultiWindow : UserControl, IReturnData
    {
        private AuxWindows.JointTextShow jointLog;
        public YAYD.Orchestrators.AdHocDownLoadAndConvertMP3 AdHocDownLoadAndConvertMP3 { get; private set; }
        public WorkerStatus Status => ((IReturnData)AdHocDownLoadAndConvertMP3).Status;
        public Dispatcher FinalDispatcher => ((IReturnData)AdHocDownLoadAndConvertMP3).FinalDispatcher;
        public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived
        {
            add
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).TrimmedOutputDataReceived += value;
            }

            remove
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).TrimmedOutputDataReceived -= value;
            }
        }
        public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived
        {
            add
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).TrimmedErrorDataReceived += value;
            }

            remove
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).TrimmedErrorDataReceived -= value;
            }
        }
        public event ProgressReportedEventHandler ProgressReported
        {
            add
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).ProgressReported += value;
            }

            remove
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).ProgressReported -= value;
            }
        }
        public event EventHandler Finished
        {
            add
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).Finished += value;
            }

            remove
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).Finished -= value;
            }
        }
        public event EventHandler StatusChanged
        {
            add
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).StatusChanged += value;
            }

            remove
            {
                ((IReturnData)AdHocDownLoadAndConvertMP3).StatusChanged -= value;
            }
        }
        /// <summary>
        /// Initializes a new control, and its corresponding <c>Orchestrators.AdHocDownLoadAndConvertMP3</c>.
        /// </summary>
        /// <param name="URL">The URL to download.</param>
        /// <param name="loc">Location of final file.</param>
        public SingleLinkInMultiWindow(string URL, string loc="")
        {
            jointLog = new AuxWindows.JointTextShow()
            {
                HideInsteadOfClose = true
            };
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(loc))
                loc = "default.mp3";
            AdHocDownLoadAndConvertMP3 = new Orchestrators.AdHocDownLoadAndConvertMP3(URL,loc, this.Dispatcher);
            AdHocDownLoadAndConvertMP3.TrimmedOutputDataReceived += AdHocDownLoadAndConvertMP3_TrimmedOutputDataReceived;
            AdHocDownLoadAndConvertMP3.TrimmedErrorDataReceived += AdHocDownLoadAndConvertMP3_TrimmedErrorDataReceived;
            AdHocDownLoadAndConvertMP3.Finished += AdHocDownLoadAndConvertMP3_Finished;
            AdHocDownLoadAndConvertMP3.GetMetaFinished += AdHocDownLoadAndConvertMP3_GetMetaFinished;
            AdHocDownLoadAndConvertMP3.DownloadFinished += AdHocDownLoadAndConvertMP3_DownloadFinished;
            AdHocDownLoadAndConvertMP3.DownloadThumbNailFinished += AdHocDownLoadAndConvertMP3_DownloadThumbNailFinished;
            AdHocDownLoadAndConvertMP3.ConvertFinished += AdHocDownLoadAndConvertMP3_ConvertFinished;
            AdHocDownLoadAndConvertMP3.StatusChanged += AdHocDownLoadAndConvertMP3_StatusChanged;
            AdHocDownLoadAndConvertMP3.ProgressReported += AdHocDownLoadAndConvertMP3_ProgressReported;
            Ready();
        }
        private void AdHocDownLoadAndConvertMP3_ConvertFinished(object sender, EventArgs e)
        {
            OtherInfo.Text = "Finished conversion!";
        }
        private void AdHocDownLoadAndConvertMP3_DownloadFinished(object sender, EventArgs e)
        {
            OtherInfo.Text = "Finished download!";
        }
        private void AdHocDownLoadAndConvertMP3_StatusChanged(object sender, EventArgs e)
        {
            switch (AdHocDownLoadAndConvertMP3.Status)
            {
                case WorkerStatus.Pending:
                    AccessOutput.Background = Brushes.White;
                    OtherInfo.Text = "Status: Pending";
                    break;
                case WorkerStatus.Ready:
                    AccessOutput.Background = Brushes.Yellow;
                    OtherInfo.Text = "Status: Ready";
                    break;
                case WorkerStatus.Running:
                    AccessOutput.Background = Brushes.Blue;
                    OtherInfo.Text = "Status: Running";
                    break;
                case WorkerStatus.Successful:
                    AccessOutput.Background = Brushes.LimeGreen;
                    OtherInfo.Text = "Status: Successful";
                    break;
                case WorkerStatus.Error:
                    AccessOutput.Background = Brushes.Red;
                    OtherInfo.Text = "Status: Error";
                    break;
                default:
                    break;
            }
        }
        private void AdHocDownLoadAndConvertMP3_DownloadThumbNailFinished(object sender, EventArgs e)
        {
            Feature.Content = new Image() { Source = new BitmapImage(new Uri(AdHocDownLoadAndConvertMP3.LocationOfTempDirectory.FullName + @"\thumbnail")) };
            OtherInfo.Text = "Finished downloading thumbnail!";
        }
        private void AdHocDownLoadAndConvertMP3_ProgressReported(object sender, ProgressReportedEventArgs e)
        {
            Progress.Value = e.Percent;
        }
        private void AdHocDownLoadAndConvertMP3_Finished(object sender, EventArgs e)
        {
            if (AdHocDownLoadAndConvertMP3.Status == WorkerStatus.Successful)
            {
                AccessOutput.Background = Brushes.LimeGreen;
                AccessOutput.IsEnabled = false;
                jointLog.HideInsteadOfClose = false;
                if (jointLog.IsVisible == false) jointLog.Close();
            }
            else if (AdHocDownLoadAndConvertMP3.Status == WorkerStatus.Error)
                AccessOutput.Background = Brushes.Red;
            else
                AccessOutput.Background = Brushes.Orange;
        }
        private void AdHocDownLoadAndConvertMP3_TrimmedErrorDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            jointLog.AppendError(e.FullData);
        }
        private void AdHocDownLoadAndConvertMP3_TrimmedOutputDataReceived(object sender, AdvancedDataReceivedEventArgs e)
        {
            jointLog.AppendOutput(e.FullData);
        }
        private void AdHocDownLoadAndConvertMP3_GetMetaFinished(object sender, EventArgs e)
        {
            FinalFileInfo.Text = AdHocDownLoadAndConvertMP3.GetMeta.Title + Environment.NewLine + AdHocDownLoadAndConvertMP3.LocationOfFinalFile.FullName;
            OtherInfo.Text = "Finished getting meta!";
        }
        private void AccessOutput_Click(object sender, RoutedEventArgs e)
        {
            jointLog.Show();
        }
        public bool Start()
        {
            return ((IReturnData)AdHocDownLoadAndConvertMP3).Start();
        }
        public bool Ready()
        {
            return ((IReturnData)AdHocDownLoadAndConvertMP3).Ready();
        }
    }
}
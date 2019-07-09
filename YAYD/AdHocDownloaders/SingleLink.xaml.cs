using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace YAYD.AdHocDownloaders
{
    /// <summary>
    /// Interaction logic for SingleLink.xaml
    /// </summary>
    public partial class SingleLink : Window, YAYD.IReturnData
    {
        string SafeTitle
        {
            get
            {
                string ret = gm.Title;
                char[] ch = System.IO.Path.GetInvalidFileNameChars().Concat(System.IO.Path.GetInvalidPathChars()).ToArray();
                foreach (char c in ch) ret = ret.Replace(c.ToString(), "");
                return ret;
            }
        }
        string SafeThumbNailName
        {
            get
            {
                return TempDirInfo.FullName + @"\" + dltn.LocationOfFinalFile.Split('\\').Last();
            }
        }
        System.IO.FileInfo FinalFileInfo
        {
            get
            {
                try
                {
                    return new System.IO.FileInfo(FinalFileLocation.Text);
                }
                catch
                {
                    return new System.IO.FileInfo(System.IO.Directory.GetCurrentDirectory() + @"\default.mp3");
                }
            }
            set
            {
                FinalFileLocation.Text = value.FullName;
            }
        }
        System.IO.DirectoryInfo TempDirInfo
        {
            get
            {
                return new System.IO.DirectoryInfo(FinalFileInfo.DirectoryName + @"\temp_" + SafeTitle);
            }
        }
        public Dispatcher FinalDispatcher { get { return this.Dispatcher; } }
        public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
        public YTDLInteract.GetMeta gm;
        YTDLInteract.Download dl;
        WebInteract.DownloadThumbNail dltn;
        FFMPEGInteract.MP3Interact.Convert cnva;

        public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
        public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
        public event ProgressReportedEventHandler ProgressReported;
        public event EventHandler Finished;
        public event EventHandler StatusChanged;

        public SingleLink(string link="", string finalloc="default.mp3")
        {
            InitializeComponent();
            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo
            {
                ProgressState= System.Windows.Shell.TaskbarItemProgressState.None
            };
            URL.Text = link;
            FinalFileLocation.Text = finalloc;
            TrimmedOutputDataReceived += (s, e) => { Output.Text += "O>:" + e.FullData + Environment.NewLine; ((ScrollViewer)Output.Parent).ScrollToBottom(); };
            TrimmedErrorDataReceived += (s, e) => { Output.Text += "E>:" + e.FullData + Environment.NewLine; ((ScrollViewer)Output.Parent).ScrollToBottom(); };
            ProgressReported += (s, e) => { TotalProgress.Value = e.Percent; this.Title = e.Percent.ToString()+"%"; };
            Ready();
        }
        public bool Ready()
        {
            if(Status== WorkerStatus.Pending)
            {
                OnStatusChanged(WorkerStatus.Ready);
            }
            return (Status == WorkerStatus.Ready);
        }
        public bool Start()
        {
            if (Status != WorkerStatus.Ready)
                return false;
            OnStatusChanged(WorkerStatus.Running);
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            OnTrimmedOutputDataReceived("New URL processed:" + URL.Text);
            PartialProgress.Value = 0;
            OnProgressReported(0);
            gm = new YTDLInteract.GetMeta(URL.Text);
            gm.TrimmedOutputDataReceived += (s2, e2) => OnTrimmedOutputDataReceived(e2);
            gm.TrimmedErrorDataReceived += (s2, e2) => OnTrimmedErrorDataReceived(e2);
            gm.ProgressReported += (s2, e2) => { PartialProgress.Value = e2.Percent; OnProgressReported(0.25 * e2.Percent); };
            gm.Finished += Gm_Finished;
            gm.Ready();
            gm.Start();
            return true;
        }
        private void ButtonSelectFinalFileLocation_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt=".mp3",
                OverwritePrompt=true,
                RestoreDirectory=true,
                ValidateNames=true,
                Filter= "Audio mp3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                FilterIndex=0
            };
            if (sfd.ShowDialog() == true)
            {
                FinalFileLocation.Text = sfd.FileName;
            }
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
        private void Gm_Finished(object sender, EventArgs e)
        {
            gm.Finished -= Gm_Finished;
            PartialProgress.Value = 0;
            OnProgressReported(25);
            if (gm.Status != WorkerStatus.Successful)
            {
                OnFinished(false);
                return;
            }
            try
            {
                if (TempDirInfo.Exists)
                {
                    TempDirInfo.Delete(true);
                    OnTrimmedOutputDataReceived("TempDirInfo deleted preexistent directory:" + TempDirInfo.FullName);
                }
                TempDirInfo.Create();
                OnTrimmedOutputDataReceived("TempDirInfo created:" + TempDirInfo.FullName);
            }
            catch(Exception exc)
            {
                OnTrimmedErrorDataReceived("TempDirInfo error operating with directory:" + exc.ToString());
                OnFinished(false);
                return;
            }
            if (FinalFileInfo.Name == "default.mp3") FinalFileInfo = new System.IO.FileInfo(FinalFileInfo.DirectoryName + @"\" + SafeTitle + ".mp3");
            OnTrimmedOutputDataReceived("FinalFileInfo location:" + FinalFileInfo.FullName);
            dl = new YTDLInteract.Download(URL.Text, "bestaudio", TempDirInfo.FullName + @"\audio");
            dl.TrimmedOutputDataReceived += (s2, e2) => OnTrimmedOutputDataReceived(e2);
            dl.TrimmedErrorDataReceived += (s2, e2) => OnTrimmedErrorDataReceived(e2);
            dl.ProgressReported += (s2, e2) => 
            {
                PartialProgress.Value = e2.Percent;
                OnProgressReported(25 + 0.25 * e2.Percent);
                if (e2.Percent > 100)
                    MessageBox.Show("e");
            };
            dl.Finished += Dl_Finished;
            dl.Ready();
            dl.Start();
        }
        private void Dl_Finished(object sender, EventArgs e)
        {
            //dl.Finished -= Dl_Finished;
            PartialProgress.Value = 0;
            OnProgressReported(50);
            if (dl.Status != WorkerStatus.Successful)
            {
                OnFinished(false);
                return;
            }
            if (ID3.UseThumbnail)
            {
                dltn = new WebInteract.DownloadThumbNail(gm.ThumbnailURL, TempDirInfo.FullName + @"\thumb.jpg");
                dltn.TrimmedOutputDataReceived += (s2, e2) => OnTrimmedOutputDataReceived(e2);
                dltn.TrimmedErrorDataReceived += (s2, e2) => OnTrimmedErrorDataReceived(e2);
                dltn.Finished += Dltn_Finished;
                dltn.Ready();
                dltn.Start();
            }
            else
            {
                dltn = null;
                Dltn_Finished(sender, e);
            }
        }
        private void Dltn_Finished(object sender, EventArgs e)
        {
            PartialProgress.Value = 0;
            OnProgressReported(75);
            if (dltn == null)
                cnva = new FFMPEGInteract.MP3Interact.Convert(TempDirInfo.FullName + @"\audio", FinalFileInfo.FullName, OutFormat.Format, ID3.Meta);
            else
            {
                //dltn.Finished -= Dltn_Finished;
                if(dltn.Status!= WorkerStatus.Successful)
                {
                    OnFinished(false);
                    return;
                }
                cnva = new FFMPEGInteract.MP3Interact.Convert(TempDirInfo.FullName + @"\audio", FinalFileInfo.FullName, OutFormat.Format, ID3.Meta, SafeThumbNailName);
            }
            cnva.TrimmedOutputDataReceived += (s2, e2) => OnTrimmedOutputDataReceived(e2);
            cnva.TrimmedErrorDataReceived += (s2, e2) => OnTrimmedErrorDataReceived(e2);
            cnva.ProgressReported += (s2, e2) => { PartialProgress.Value = e2.Percent; OnProgressReported( 75 + 0.25 * e2.Percent); };
            cnva.Finished += Cnva_Finished;
            cnva.Ready();
            cnva.Start();
        }
        private void Cnva_Finished(object sender, EventArgs e)
        {
            //cnva.Finished -= Cnva_Finished;
            PartialProgress.Value = 0;
            OnProgressReported(100);
            if (cnva.Status != WorkerStatus.Successful)
            {
                OnFinished(false);
                return;
            }
            try
            {
                TempDirInfo.Delete(true);
                OnTrimmedOutputDataReceived("TempDirInfo deleted:" + TempDirInfo.FullName);
            }
            catch (Exception exc)
            {
                OnTrimmedErrorDataReceived("Error deleting TempDirInfo:" + exc.ToString());
            }
            OnFinished(true);
        }
        private void OnTrimmedErrorDataReceived(string data)
        {
            FinalDispatcher.Invoke(() => TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "SingleLinkDL")));
        }
        private void OnTrimmedErrorDataReceived(AdvancedDataReceivedEventArgs data)
        {
            FinalDispatcher.Invoke(() => TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "SingleLinkDL")));
        }
        private void OnTrimmedOutputDataReceived(string data)
        {
            FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "SingleLinkDL")));
        }
        private void OnTrimmedOutputDataReceived(AdvancedDataReceivedEventArgs data)
        {
            FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "SingleLinkDL")));
        }
        private void OnFinished(bool isSuccessful)
        {
            if (isSuccessful)
            {
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                OnStatusChanged(WorkerStatus.Successful);
            }
            else
            {
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                OnStatusChanged(WorkerStatus.Error);
            }
            if(Finished!=null)
                FinalDispatcher.Invoke(() => Finished?.Invoke(this, EventArgs.Empty));
        }
        private void OnProgressReported(double val)
        {
            TaskbarItemInfo.ProgressValue = val/100;
            if(ProgressReported!=null)
                FinalDispatcher.Invoke(() => ProgressReported?.Invoke(this, ProgressReportedEventArgs.PROnlyPercent(val)));
        }
        private void OnStatusChanged(WorkerStatus s)
        {
            Status = s;
            if (StatusChanged != null)
                FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
        }
    }
}

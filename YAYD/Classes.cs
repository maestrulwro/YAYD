// TODO: comment
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Threading;
using System;
namespace YAYD
{
    public delegate void TrimmedDataReceivedEventHandler(object sender, AdvancedDataReceivedEventArgs e);
    public delegate void ProgressReportedEventHandler(object sender, ProgressReportedEventArgs e);
    /// <summary>
    /// The status a task can have at any moment. Strongly related to <c>IReturnData</c>.
    /// </summary>
    public enum WorkerStatus
    {
        /// <summary>
        /// The task is initialized, but is not ready to be run (something else has to be done before).
        /// Execute <c>IReturnData.Ready()</c> to check if the task is ready to be run.
        /// </summary>
        Pending,
        /// <summary>
        /// The task is ready to be run, execute <c>IReturnData.Start()</c>.
        /// </summary>
        Ready,
        /// <summary>
        /// The task is currently running.
        /// </summary>
        Running,
        /// <summary>
        /// The task ended successfully.
        /// </summary>
        Successful,
        /// <summary>
        /// An error occured.
        /// </summary>
        Error
    }
    /// <summary>
    /// Implements events related to working in the background.
    /// </summary>
    public interface IReturnData
    {
        /// <summary>
        /// The status of the task.
        /// </summary>
        WorkerStatus Status { get; }
        /// <summary>
        /// The dispatcher of the <c>Thread</c> on which to raise the events.
        /// </summary>
        Dispatcher FinalDispatcher { get; }
        /// <summary>
        /// Raised when the worker outputs a non-empty line on <c>stdout</c>.
        /// </summary>
        event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
        /// <summary>
        /// Raised when the worker outputs a non-empty line on <c>stderr</c>.
        /// </summary>
        event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
        /// <summary>
        /// Raised when the the worker returns a line that shows current progress.
        /// </summary>
        event ProgressReportedEventHandler ProgressReported;
        /// <summary>
        /// Raised when the the worker finishes.
        /// </summary>
        event EventHandler Finished;
        /// <summary>
        /// Raised when <c>Status</c> changes.
        /// </summary>
        event EventHandler StatusChanged;
        /// <summary>
        /// Starts the work. <c>Status</c> should be <c>WorkerStatus.Ready</c> for the function to be successful.
        /// </summary>
        /// <returns><c>True</c> if work is started successfully.
        /// <c>False</c> if there is an error or if <c>Status != WorkerStatus.Ready</c>.</returns>
        /// <remarks>Should be successfully called only once (only once should it return <c>True</c>).</remarks>
        bool Start();
        /// <summary>
        /// <c>Status</c> goes from <c>Pending</c> to <c>Ready</c>. Depending on implementation, additional condition checking and preprocessing can be made.
        /// </summary>
        /// <returns><c>True</c> if <c>Status</c> is successfully changed, additionals conditions are met, preprocessing is successful and the thing is ready.
        /// <c>False</c> if something is wrong.</returns>
        /// <remarks>Should be successfully called multiple times (multiple times should it return <c>True</c>).
        /// When called the second, third, fourth time after first successful call (called while <c>Status</c> is <c>Ready</c> already), it should only return true (and do nothing more).</remarks>
        bool Ready();
    }
    /// <summary>
    /// Organizes <c>IReturnData</c> workers.
    /// </summary>
    public class Scheduler
    {
        /// <summary>
        /// List of tasks/workers.
        /// </summary>
        public List<IReturnData> Tasks { get; private set; }
        /// <summary>
        /// The timer that starts workers.
        /// </summary>
        public DispatcherTimer Timer { get; private set; }
        /// <summary>
        /// The number of workers with <c>Status == WorkerStatus.Running</c> at any time.
        /// </summary>
        public int MaxRunningTaskCount { get; private set; }
        /// <summary>
        /// Initializes a new Scheduler instance.
        /// </summary>
        /// <param name="maxtasksrunning">The (maximum) number of tasks/workers running at a given time.</param>
        /// <param name="ts">Period between checks to try to run a new worker.</param>
        /// <param name="dispatcher">The dispatcher on which to run the timer and do <c>Worker.Start()</c>.</param>
        public Scheduler(int maxtasksrunning = 0, TimeSpan? ts = null, Dispatcher dispatcher = null)
        {
            if (maxtasksrunning == 0)
                MaxRunningTaskCount = Properties.Settings.Default.MaxWorkUnitsInScheduler;
            else
                MaxRunningTaskCount = maxtasksrunning;
            TimeSpan tsp;
            if (dispatcher == null)
                dispatcher = Dispatcher.CurrentDispatcher;
            if (ts == null)
                tsp = new TimeSpan(0, 0, 5);
            else
                tsp = ((TimeSpan)ts);
            Timer = new DispatcherTimer(tsp, DispatcherPriority.Background, Timer_Tick, dispatcher);
            Tasks = new List<IReturnData>();
            Timer.IsEnabled = true;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            StartNextTask();
        }
        private int GetNumberOfTasksWithGivenStatus(WorkerStatus status)
        {
            int ret = 0;
            for (int i = 0; i < Tasks.Count; i++)
                if (Tasks[i].Status == status)
                    ret++;
            return ret;
        }
        private void StartNextTask()
        {
            if (GetNumberOfTasksWithGivenStatus(WorkerStatus.Running) >= MaxRunningTaskCount) return; // if the number of running tasks is greater or equal to the number of max running tasks, exit
            for (int i = 0; i < Tasks.Count; i++) // iterate through the tasks
                if (Tasks[i].Ready()) // if a task is Ready
                {
                    Tasks[i].Start();
                    Tasks[i].Finished += SomeTask_Finished;
                    return; // start the task and exit the function
                }
        }
        private void SomeTask_Finished(object sender, EventArgs e)
        {
            Tasks.Remove(((IReturnData)sender));
        }
    }
    /// <summary>
    /// Class for use in the <c>ProgressReportedEvent</c>.
    /// </summary>
    public class ProgressReportedEventArgs : EventArgs
    {
        /// <summary>
        /// The % of work done, between 0 and 100.
        /// </summary>
        /// <remarks>Default value is 0.</remarks>
        public double Percent { get; protected set; } = 0;
        /// <summary>
        /// The speed of the download (bits per second and multiples) or the rate of conversion (formatted as: [int].[int]x);
        /// </summary>
        /// <remarks>The speed applies to youtube-dl, while the rate applies to FFmpeg.
        /// Default value is "N/A".</remarks>
        public string Speed { get; protected set; } = "N/A";
        /// <summary>
        /// Estimated remaining time (ETA) by the program or the current time of the encoded file.
        /// </summary>
        /// <remarks>ETA applies to youtube-dl, while the current time of the encoded file applies to FFmpeg.
        /// Default value is 0 ticks.</remarks>
        public TimeSpan Time { get; protected set; } = new TimeSpan(0);
        /// <summary>
        /// Current frame encoded.
        /// </summary>
        /// <remarks>Default value is 0.</remarks>
        public int Frame { get; protected set; } = 0;
        /// <summary>
        /// Frames encoded per second.
        /// </summary>
        /// <remarks>Default value is 0.</remarks>
        public double FPS { get; protected set; } = 0;
        /// <summary>
        /// I don't really know.
        /// </summary>
        /// <remarks>Default value is 0.</remarks>
        public double Q { get; protected set; } = 0;
        /// <summary>
        /// The total size of the final file being downloaded or the current size of the encoded file ,in Bytes (or multiples).
        /// </summary>
        /// <remarks>Total size applies to youtube-dl, while the current size applies to FFmpeg.
        /// <para>Default value is "N/A".</para></remarks>
        public string Size { get; protected set; } = "N/A";
        /// <summary>
        /// Current bitrate encoded.
        /// </summary>
        /// <remarks>Default value is "N/A".</remarks>
        public string Bitrate { get; protected set; } = "N/A";
        /// <summary>
        /// Returns a <c>ProgressReportedEventArgs</c> based on a message from youtube-dl.
        /// </summary>
        /// <param name="source">The line to be interpreted (parsed).</param>
        /// <returns>A <c>ProgressReportedEventArgs</c> based on the <c>source</c>.</returns>
        static public ProgressReportedEventArgs PRFromYTDLDownloader(string source)
        {
            // [download]   9.3% of 146.80MiB at  4.22MiB/s ETA 00:31
            ProgressReportedEventArgs ret = new ProgressReportedEventArgs();
            System.Text.RegularExpressions.Regex r;
            r = new System.Text.RegularExpressions.Regex(@"([0-9.]+)%");
            if (r.IsMatch(source))
                if (double.TryParse(r.Match(source).Groups[1].Value.Replace('.', ','), out double res))
                    ret.Percent = res;
            r = new System.Text.RegularExpressions.Regex(@"of\s+([0-9a-zA-Z.]+B)");
            if (r.IsMatch(source)) ret.Size = r.Match(source).Groups[1].Value;
            r = new System.Text.RegularExpressions.Regex(@"at\s+([0-9a-zA-Z.]+/s)");
            if (r.IsMatch(source)) ret.Speed = r.Match(source).Groups[1].Value;
            r = new System.Text.RegularExpressions.Regex(@"ETA\s+([0-9]+):([0-9]+)");
            if (r.IsMatch(source))
                if ((int.TryParse(r.Match(source).Groups[1].Value, out int mm) && (int.TryParse(r.Match(source).Groups[2].Value, out int ss))))
                    ret.Time = new TimeSpan(0, mm, ss);
            return ret;
        }
        /// <summary>
        /// A general <c>ProgressReportedEventArgs</c> for a finised program.
        /// </summary>
        /// <returns>A <c>ProgressReportedEventArgs</c> with <c>Percent</c> equal to 100.</returns>
        static public ProgressReportedEventArgs PRFinished()
        {
            return PROnlyPercent(100);
        }
        /// <summary>
        /// Returns a <c>ProgressReportedEventArgs</c> only with <c>Percent</c> completed.
        /// </summary>
        /// <param name="percent">The value.</param>
        /// <param name="completeothers">The value to complete the other properties with.</param>
        /// <returns>A <c>ProgressReportedEventArgs</c> with the specified <c>Percent</c> value.</returns>
        static public ProgressReportedEventArgs PROnlyPercent(double percent)
        {
            return new ProgressReportedEventArgs()
            {
                Percent = percent
            };
        }
        /// <summary>
        /// Returns a <c>ProgressReportedEventArgs</c> based on a message from FFmpeg.
        /// </summary>
        /// <param name="source">The line to be interpreted (parsed).</param>
        /// <returns>A <c>ProgressReportedEventArgs</c> based on the <c>source</c>.</returns>
        static public ProgressReportedEventArgs PRFromFFMPEGConverter(string source)
        {
            // frame = 1 fps = 0.1 q = 0.0 size = 0kB time = 00:03:14.88 bitrate = 0.0kbits / s speed = 14.4x
            ProgressReportedEventArgs ret = new ProgressReportedEventArgs();
            System.Text.RegularExpressions.Regex r;
            r = new System.Text.RegularExpressions.Regex(@"frame\s*=\s*([0-9]+)");
            if (r.IsMatch(source))
                if (int.TryParse(r.Match(source).Groups[1].Value, out int res))
                    ret.Frame = res;
            r = new System.Text.RegularExpressions.Regex(@"fps\s*=\s*([0-9]+.[0-9]+)");
            if (r.IsMatch(source))
                if (double.TryParse(r.Match(source).Groups[1].Value, out double res))
                    ret.FPS = res;
            r = new System.Text.RegularExpressions.Regex(@"q\s*=\s*([0-9]+.[0-9]+)");
            if (r.IsMatch(source))
                if (double.TryParse(r.Match(source).Groups[1].Value, out double res))
                    ret.Q = res;
            r = new System.Text.RegularExpressions.Regex(@"size\s*=\s*([0-9]+[a-zA-Z]*B)");
            if (r.IsMatch(source))
                ret.Size = r.Match(source).Groups[1].Value;
            r = new System.Text.RegularExpressions.Regex(@"time\s*=\s*([0-9]+):([0-9]+):([0-9]+).([0-9]+)");
            if (r.IsMatch(source))
                if ((int.TryParse(r.Match(source).Groups[1].Value, out int hh) && (int.TryParse(r.Match(source).Groups[2].Value, out int mm) && (int.TryParse(r.Match(source).Groups[3].Value, out int ss) && (int.TryParse(r.Match(source).Groups[4].Value, out int ms))))))
                    ret.Time = new TimeSpan(0, hh, mm, ss, ms);
            r = new System.Text.RegularExpressions.Regex(@"bitrate\s*=\s*([a-zA-Z0-9. ]+/\s*s)");
            if (r.IsMatch(source))
                ret.Bitrate = r.Match(source).Groups[1].Value;
            r = new System.Text.RegularExpressions.Regex(@"speed\s*=\s*([0-9]+.[0-9]+x)");
            if (r.IsMatch(source))
                ret.Speed = r.Match(source).Groups[1].Value;
            return ret;
        }
        /// <summary>
        /// Returns a <c>ProgressReportedEventArgs</c> based on a message from FFmpeg, additionally having <c>Percent</c> completed based on <c>totaltime</c>.
        /// </summary>
        /// <param name="source">The line to be interpreted (parsed).</param>
        /// <param name="totaltime">The total time of the file being parsed.</param>
        /// <returns>A <c>ProgressReportedEventArgs</c> based on the <c>source</c>.</returns>
        static public ProgressReportedEventArgs PRFromFFMPEGConverter(string source, TimeSpan totaltime)
        {
            ProgressReportedEventArgs ret = PRFromFFMPEGConverter(source);
            if (totaltime.Ticks != 0) ret.Percent = ((double)ret.Time.Ticks) / ((double)totaltime.Ticks) * 100d;
            return ret;
        }
        /// <summary>
        /// Creates an empty instance. Used internally by other constructors.
        /// </summary>
        private ProgressReportedEventArgs() { }
    }
    /// <summary>
    /// Class for use in various <c>DataReceived</c> events.
    /// </summary>
    public class AdvancedDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The date and time at which the event was raised.
        /// </summary>
        /// <remarks>This is the time of the first raising, corresponding to the last sender.</remarks>
        public DateTime Time { get; }
        /// <summary>
        /// The <c>DataReceivedEventArgs.Data</c> part.
        /// </summary>
        public string Data { get; }
        /// <summary>
        /// All senders, starting with the upmost (last) one.
        /// </summary>
        public object[] Senders { get; }
        /// <summary>
        /// The descriptors of the senders, starting with the upmost (last) sender.
        /// </summary>
        public string[] SenderDescriptions { get; }
        /// <summary>
        /// Returns a string showing the <c>Data</c> and all the <c>SenderDescription</c> members.
        /// </summary>
        public string FullData
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                string msec = Time.Millisecond.ToString();
                while (msec.Length < 3)
                    msec = "0" + msec;
                sb.Append("[" + Time.ToString() + "." + msec + "]");
                for (int i = 0; i < SenderDescriptions.Length; i++)
                    sb.Append(SenderDescriptions[i] + "<");
                sb.Append(Data);
                return sb.ToString();
            }
        }
        /// <summary>
        /// Constructor for a brand new instance.
        /// </summary>
        /// <param name="data">Self explanatory.</param>
        /// <param name="sender">Self explanatory.</param>
        /// <param name="senderdesc">Also self explanatory.</param>
        public AdvancedDataReceivedEventArgs(string data, object sender, string senderdesc)
        {
            Time = DateTime.Now;
            this.Data = data;
            Senders = new object[1] { sender };
            SenderDescriptions = new string[1] { senderdesc };
        }
        /// <summary>
        /// A fancier constructor for a brand new instance.
        /// </summary>
        /// <param name="data">Self explanatory.</param>
        /// <param name="senders">Self explanatory. First element is the upmost sender.</param>
        /// <param name="senderdescs">Also self explanatory. First element is the upmost sender.</param>
        public AdvancedDataReceivedEventArgs(string data, object[] senders, string[] senderdescs)
        {
            Time = DateTime.Now;
            Data = data;
            Senders = senders;
            SenderDescriptions = senderdescs;
            if (Senders.Length != SenderDescriptions.Length) throw new ArgumentException("Senders and SenderDescriptions must have the same number of elements! Current values:" + Senders.Length + "/" + SenderDescriptions.Length, "SenderDescriptions");
        }
        /// <summary>
        /// Constructor for adding a newer <c>Senders</c> element to the game.
        /// </summary>
        /// <param name="precedent">The base <c>AdvancedDataReceivedEventArgs</c>.</param>
        /// <param name="currentsender">The current sender, recommended to be <c>this</c>.</param>
        /// <param name="currentsenderdesc">The descriptor of the upmost sender.</param>
        public AdvancedDataReceivedEventArgs(AdvancedDataReceivedEventArgs precedent, object currentsender, string currentsenderdesc)
        {
            Time = DateTime.Now;
            Data = precedent.Data;
            Senders = new object[precedent.Senders.Length + 1];
            Senders[0] = currentsender;
            for (int i = 0; i < precedent.Senders.Length; i++) Senders[i + 1] = precedent.Senders[i];
            SenderDescriptions = new string[precedent.SenderDescriptions.Length + 1];
            SenderDescriptions[0] = currentsenderdesc;
            for (int i = 0; i < precedent.SenderDescriptions.Length; i++) SenderDescriptions[i + 1] = precedent.SenderDescriptions[i];
        }
    }
    /// <summary>
    /// Namespace for interaction with <c>youtube-dl</c>.
    /// </summary>
    namespace YTDLInteract
    {
        /// <summary>
        /// Class for getting meta information from youtube.
        /// </summary>
        public class GetMeta : IReturnData
        {
            private bool AnyErrorYet = false;
            private List<Process> Processes;
            private List<string> formatsasstring = null; // will be initialized after format descriptor table header is read
            private string BV;
            private string BA;
            private string BF;
            private int finishedtasks = 0;
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            /// <summary>
            /// The URL on which the operations are executed.
            /// </summary>
            public string URL { get; }
            /// <summary>
            /// The identifier of the URL.
            /// </summary>
            public string ID { get; private set; }
            /// <summary>
            /// The title of the URL.
            /// </summary>
            public string Title { get; private set; }
            /// <summary>
            /// The duration of the URL (reported by YTDL).
            /// </summary>
            public string Duration { get; private set; }
            /// <summary>
            /// Link to the thumbnail of the video.
            /// </summary>
            public string ThumbnailURL { get; private set; }
            /// <summary>
            /// Available formats.
            /// </summary>
            public FormatList Formats { get; private set; }
            public Dispatcher FinalDispatcher { get; }
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
                    OnStatusChanged(WorkerStatus.Ready);
                return (Status == WorkerStatus.Ready);
            }
            public bool Start()
            {
                if (Status != WorkerStatus.Ready)
                    return false;
                OnStatusChanged(WorkerStatus.Running);
                System.Threading.Thread t = new System.Threading.Thread(() =>
                {
                    foreach (Process elem in Processes)
                    {
                        elem.Start();
                        elem.BeginOutputReadLine();
                        elem.BeginErrorReadLine();
                        System.Threading.Thread.Sleep(1000);
                    }
                });
                t.Start();
                FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs("Inner processes started.", this, "ytdl_getmeta" )));
                return true;
            }
            /// <summary>
            /// Initializes a new instance of the GetMeta class.
            /// </summary>
            /// <param name="url">The URL to get the meta of.</param>
            /// <param name="finaldispatcher">The dispatcher of the thread on which the final events will be raised.</param>
            public GetMeta(string url, System.Windows.Threading.Dispatcher finaldispatcher = null)
            {
                if (finaldispatcher == null) FinalDispatcher = Dispatcher.CurrentDispatcher;
                else FinalDispatcher = finaldispatcher;
                this.URL = url;
                Processes = new List<Process>();
                Process p_id = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " --get-id",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_id.OutputDataReceived += P_id_OutputDataReceived;
                p_id.OutputDataReceived += P_id_OutputDataReceivedGeneric;
                p_id.ErrorDataReceived += P_id_ErrorDataReceivedGeneric;
                p_id.Exited += P_id_Exited;
                Processes.Add(p_id);
                Process p_title = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " --get-title",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_title.OutputDataReceived += P_title_OutputDataReceived;
                p_title.OutputDataReceived += P_title_OutputDataReceivedGeneric;
                p_title.ErrorDataReceived += P_title_ErrorDataReceivedGeneric;
                p_title.Exited += P_title_Exited;
                Processes.Add(p_title);
                Process p_duration = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " --get-duration",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_duration.OutputDataReceived += P_duration_OutputDataReceived;
                p_duration.OutputDataReceived += P_duration_OutputDataReceivedGeneric;
                p_duration.ErrorDataReceived += P_duration_ErrorDataReceivedGeneric;
                p_duration.Exited += P_duration_Exited;
                Processes.Add(p_duration);
                Process p_thumbnail = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " --get-thumbnail",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_thumbnail.OutputDataReceived += P_thumbnail_OutputDataReceived;
                p_thumbnail.OutputDataReceived += P_thumbnail_OutputDataReceivedGeneric;
                p_thumbnail.ErrorDataReceived += P_thumbnail_ErrorDataReceivedGeneric;
                p_thumbnail.Exited += P_thumbnail_Exited;
                Processes.Add(p_thumbnail);
                Process p_formats = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " -F",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_formats.OutputDataReceived += P_formats_OutputDataReceived;
                p_formats.OutputDataReceived += P_formats_OutputDataReceivedGeneric;
                p_formats.ErrorDataReceived += P_formats_ErrorDataReceivedGeneric;
                p_formats.Exited += P_formats_Exited;
                Processes.Add(p_formats);
                Process p_bf = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " -f best --get-format",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_bf.OutputDataReceived += P_bf_OutputDataReceived;
                p_bf.OutputDataReceived += P_bf_OutputDataReceivedGeneric;
                p_bf.ErrorDataReceived += P_bf_ErrorDataReceivedGeneric;
                p_bf.Exited += P_bf_Exited;
                Processes.Add(p_bf);
                Process p_ba = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " -f bestaudio --get-format",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_ba.OutputDataReceived += P_ba_OutputDataReceived;
                p_ba.OutputDataReceived += P_ba_OutputDataReceivedGeneric;
                p_ba.ErrorDataReceived += P_ba_ErrorDataReceivedGeneric;
                p_ba.Exited += P_ba_Exited;
                Processes.Add(p_ba);
                Process p_bv = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " -f bestvideo --get-format",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p_bv.OutputDataReceived += P_bv_OutputDataReceived;
                p_bv.OutputDataReceived += P_bv_OutputDataReceivedGeneric;
                p_bv.ErrorDataReceived += P_bv_ErrorDataReceivedGeneric;
                p_bv.Exited += P_bv_Exited;
                Processes.Add(p_bv);
            }
            private void P_bv_Exited(object sender, EventArgs e)
            {
                Process p_bv = (Process)sender;
                //p_bv.OutputDataReceived -= P_bv_OutputDataReceived;
                p_bv.OutputDataReceived -= P_bv_OutputDataReceivedGeneric;
                p_bv.ErrorDataReceived -= P_bv_ErrorDataReceivedGeneric;
                p_bv.Exited -= P_bv_Exited;
                OnTrimmedOutputDataReceived(sender, "p_bv","Exit code=" + p_bv.ExitCode);
                if (p_bv.ExitCode != 0) AnyErrorYet = true;
                p_bv.Close();
                P_All_Exited();
            }
            private void P_ba_Exited(object sender, EventArgs e)
            {
                Process p_ba = (Process)sender;
                //p_ba.OutputDataReceived -= P_ba_OutputDataReceived;
                p_ba.OutputDataReceived -= P_ba_OutputDataReceivedGeneric;
                p_ba.ErrorDataReceived -= P_ba_ErrorDataReceivedGeneric;
                p_ba.Exited -= P_ba_Exited;
                OnTrimmedOutputDataReceived(sender, "p_ba", "Exit code=" + p_ba.ExitCode);
                if (p_ba.ExitCode != 0) AnyErrorYet = true;
                p_ba.Close();
                P_All_Exited();
            }
            private void P_bf_Exited(object sender, EventArgs e)
            {
                Process p_bf = (Process)sender;
                //p_bf.OutputDataReceived -= P_bf_OutputDataReceived;
                p_bf.OutputDataReceived -= P_bf_OutputDataReceivedGeneric;
                p_bf.ErrorDataReceived -= P_bf_ErrorDataReceivedGeneric;
                p_bf.Exited -= P_bf_Exited;
                OnTrimmedOutputDataReceived(sender, "p_bf", "Exit code=" + p_bf.ExitCode);
                if (p_bf.ExitCode != 0) AnyErrorYet = true;
                p_bf.Close();
                P_All_Exited();
            }
            private void P_formats_Exited(object sender, EventArgs e)
            {
                Process p_formats = (Process)sender;
                p_formats.OutputDataReceived -= P_formats_OutputDataReceived;
                p_formats.OutputDataReceived -= P_formats_OutputDataReceivedGeneric;
                p_formats.ErrorDataReceived -= P_formats_ErrorDataReceivedGeneric;
                p_formats.Exited -= P_formats_Exited;
                OnTrimmedOutputDataReceived(sender, "p_formats", "Exit code=" + p_formats.ExitCode);
                if (p_formats.ExitCode != 0) AnyErrorYet = true;
                p_formats.Close();
                P_All_Exited();
            }
            private void P_thumbnail_Exited(object sender, EventArgs e)
            {
                Process p_thumbnail = (Process)sender;
                //p_thumbnail.OutputDataReceived -= P_thumbnail_OutputDataReceived;
                p_thumbnail.OutputDataReceived -= P_thumbnail_OutputDataReceivedGeneric;
                p_thumbnail.ErrorDataReceived -= P_thumbnail_ErrorDataReceivedGeneric;
                p_thumbnail.Exited -= P_thumbnail_Exited;
                OnTrimmedOutputDataReceived(sender, "p_thumbnail", "Exit code=" + p_thumbnail.ExitCode);
                if (p_thumbnail.ExitCode != 0) AnyErrorYet = true;
                p_thumbnail.Close();
                P_All_Exited();
            }
            private void P_duration_Exited(object sender, EventArgs e)
            {
                Process p_duration = (Process)sender;
                //p_duration.OutputDataReceived -= P_duration_OutputDataReceived;
                p_duration.OutputDataReceived -= P_duration_OutputDataReceivedGeneric;
                p_duration.ErrorDataReceived -= P_duration_ErrorDataReceivedGeneric;
                p_duration.Exited -= P_duration_Exited;
                OnTrimmedOutputDataReceived(sender, "p_duration", "Exit code=" + p_duration.ExitCode);
                if (p_duration.ExitCode != 0) AnyErrorYet = true;
                p_duration.Close();
                P_All_Exited();
            }
            private void P_title_Exited(object sender, EventArgs e)
            {
                Process p_title = (Process)sender;
                //p_title.OutputDataReceived -= P_title_OutputDataReceived;
                p_title.OutputDataReceived -= P_title_OutputDataReceivedGeneric;
                p_title.ErrorDataReceived -= P_title_ErrorDataReceivedGeneric;
                p_title.Exited -= P_title_Exited;
                OnTrimmedOutputDataReceived(sender, "p_title", "Exit code=" + p_title.ExitCode);
                if (p_title.ExitCode != 0) AnyErrorYet = true;
                p_title.Close();
                P_All_Exited();
            }
            private void P_id_Exited(object sender, EventArgs e)
            {
                Process p_id = (Process)sender;
                //p_id.OutputDataReceived -= P_id_OutputDataReceived;
                p_id.OutputDataReceived -= P_id_OutputDataReceivedGeneric;
                p_id.ErrorDataReceived -= P_id_ErrorDataReceivedGeneric;
                p_id.Exited -= P_id_Exited;
                OnTrimmedOutputDataReceived(sender, "p_id", "Exit code=" + p_id.ExitCode);
                if (p_id.ExitCode != 0) AnyErrorYet = true;
                p_id.Close();
                P_All_Exited();
            }
            private void P_bv_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_bv", e);
            }
            private void P_ba_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_ba", e);
            }
            private void P_bf_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_bf", e);
            }
            private void P_formats_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_formats", e);
            }
            private void P_thumbnail_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_thumbnail", e);
            }
            private void P_duration_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_duration", e);
            }
            private void P_title_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_title", e);
            }
            private void P_id_ErrorDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(sender, "p_id", e);
            }
            private void P_bv_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_bv", e);
            }
            private void P_ba_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_ba", e);
            }
            private void P_bf_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_bf", e);
            }
            private void P_formats_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_formats", e);
            }
            private void P_thumbnail_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_thumbnail", e);
            }
            private void P_duration_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_duration", e);
            }
            private void P_title_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_title", e);
            }
            private void P_id_OutputDataReceivedGeneric(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(sender, "p_id", e);
            }
            private void P_All_Exited()
            {
                finishedtasks++;
                OnProgressReported(((double)finishedtasks) / 8 * 100);
                if (finishedtasks != 8) return;
                Formats = new FormatList(formatsasstring,BA,BV,BF);
                if (AnyErrorYet) OnStatusChanged(WorkerStatus.Error);
                else OnStatusChanged(WorkerStatus.Successful);
                FinalDispatcher.Invoke(() =>
                    Finished?.Invoke(this, EventArgs.Empty)
                );
            }
            private void P_bv_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        BV = e.Data.Split(' ')[0];
                        ((Process)sender).OutputDataReceived -= P_bv_OutputDataReceived;
                    }
            }
            private void P_ba_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        BA = e.Data.Split(' ')[0];
                        ((Process)sender).OutputDataReceived -= P_ba_OutputDataReceived;
                    }
            }
            private void P_bf_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        BF = e.Data.Split(' ')[0];
                        ((Process)sender).OutputDataReceived -= P_bf_OutputDataReceived;
                    }
            }
            private void P_formats_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        if (formatsasstring == null)
                        {
                            if (e.Data.StartsWith("format code"))
                                formatsasstring = new List<string>();
                        }
                        else
                        {
                            formatsasstring.Add(e.Data);
                        } 
                    }
                }
            }
            private void P_thumbnail_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ThumbnailURL = e.Data;
                        ((Process)sender).OutputDataReceived -= P_thumbnail_OutputDataReceived;
                    }
            }
            private void P_duration_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Duration = e.Data;
                        ((Process)sender).OutputDataReceived -= P_duration_OutputDataReceived;
                    }
            }
            private void P_title_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        Title = e.Data;
                        ((Process)sender).OutputDataReceived -= P_title_OutputDataReceived;
                    }
            }
            private void P_id_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ID = e.Data;
                        ((Process)sender).OutputDataReceived -= P_id_OutputDataReceived;
                    }
            }
            private void OnProgressReported(double percent)
            {
                if (ProgressReported != null)
                    FinalDispatcher.Invoke(() => ProgressReported?.Invoke(this, ProgressReportedEventArgs.PROnlyPercent(percent)));
            }
            private void OnTrimmedErrorDataReceived(object basesender, string basesenderdesc, DataReceivedEventArgs data)
            {
                if (data != null)
                    OnTrimmedErrorDataReceived(basesender, basesenderdesc, data.Data);
            }
            private void OnTrimmedErrorDataReceived(object basesender, string basesenderdesc, string data)
            {
                if (!string.IsNullOrWhiteSpace(data))
                    if (TrimmedErrorDataReceived != null)
                        FinalDispatcher.Invoke(() => TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, new object[] { this, basesender }, new string[] { "ytdl_getmeta", basesenderdesc })));
            }
            private void OnTrimmedOutputDataReceived(object basesender, string basesenderdesc, DataReceivedEventArgs data)
            {
                if (data != null)
                    OnTrimmedOutputDataReceived(basesender, basesenderdesc, data.Data);
            }
            private void OnTrimmedOutputDataReceived(object basesender, string basesenderdesc, string data)
            {
                if (!string.IsNullOrWhiteSpace(data))
                    if (TrimmedOutputDataReceived != null)
                        FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, new object[] { this, basesender }, new string[] { "ytdl_getmeta", basesenderdesc })));
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
        }
        /// <summary>
        /// Class for downloading files from youtube.
        /// </summary>
        public class Download : IReturnData
        {
            public Process YTDLInstance { get; }
            public string URL { get; }
            public string FormatToDownload { get; }
            public string LocationOfFinalFile { get; }
            public Dispatcher FinalDispatcher { get; }
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public bool Start()
            {
                if (Status != WorkerStatus.Ready)
                    return false;
                OnStatusChanged(WorkerStatus.Running);
                YTDLInstance.Start();
                YTDLInstance.BeginOutputReadLine();
                YTDLInstance.BeginErrorReadLine();
                return true;
            }
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
                {
                    OnStatusChanged(WorkerStatus.Ready);
                }
                return (Status == WorkerStatus.Ready);
            }
            public Download(string url, string format, string locationoffinalfile = "%(title)s.%(ext)s", Dispatcher finalthread = null)
            {
                URL = url;
                FormatToDownload = format;
                LocationOfFinalFile = locationoffinalfile;
                if (finalthread == null) FinalDispatcher = Dispatcher.CurrentDispatcher;
                else finalthread = FinalDispatcher;
                YTDLInstance = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " -f " + FormatToDownload + @" -o """ + LocationOfFinalFile + @"""",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                YTDLInstance.OutputDataReceived += YTDLInstance_OutputDataReceived;
                YTDLInstance.ErrorDataReceived += YTDLInstance_ErrorDataReceived;
                YTDLInstance.Exited += YTDLInstance_Exited;
            }
            private void YTDLInstance_Exited(object sender, EventArgs e)
            {
                Process p = (Process)sender;
                if (p.ExitCode == 0) OnStatusChanged(WorkerStatus.Successful);
                else OnStatusChanged(WorkerStatus.Error);
                if (TrimmedOutputDataReceived != null)
                    FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs("Exit code=" + p.ExitCode, new object[] { this, sender }, new string[] { "ytdl_dl", "innerprocess" })));
                if (Finished != null)
                    FinalDispatcher.Invoke(() => Finished?.Invoke(this, e));
                YTDLInstance.OutputDataReceived -= YTDLInstance_OutputDataReceived;
                YTDLInstance.ErrorDataReceived -= YTDLInstance_ErrorDataReceived;
                YTDLInstance.Exited -= YTDLInstance_Exited;
                p.Close();
            }
            private void YTDLInstance_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e != null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        if (TrimmedErrorDataReceived != null)
                            FinalDispatcher.Invoke(() => { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, sender }, new string[] { "ytdl_dl", "innerprocess" })); });
            }
            private void YTDLInstance_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if(e!=null)
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ProgressReportedEventArgs p = null;
                        try
                        {
                            p = ProgressReportedEventArgs.PRFromYTDLDownloader(e.Data);
                            if (p.Percent == 0) p = null;
                        }
                        catch { }
                        if (p != null)
                            if (ProgressReported != null)
                                FinalDispatcher.Invoke(() => { ProgressReported?.Invoke(this, p); });
                        if (TrimmedOutputDataReceived != null)
                            FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, sender }, new string[] { "ytdl_dl", "innerprocess" })); });
                    }
            }
            public Download(string url, int format, string locationoffinalfile, Dispatcher finalthread = null) : this(url, format.ToString(), locationoffinalfile, finalthread)
            {

            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
        }
        /// <summary>
        /// Class for downloading thumbnails from youtube.
        /// </summary>
        public class DownloadThumbNail : IReturnData
        {
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            /// <summary>
            /// Raised when the underlying <c>youtube-dl</c> instance has finished its work.
            /// </summary>
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public Process YTDLInstance { get; }
            public string URL { get; }
            public string LocationOfFinalFile { get; private set; }
            public Dispatcher FinalDispatcher { get; }
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
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
                YTDLInstance.Start();
                YTDLInstance.BeginOutputReadLine();
                YTDLInstance.BeginErrorReadLine();
                return true;
            }
            public DownloadThumbNail(string url, string locationoffinalfile, Dispatcher finalthread = null)
            {
                URL = url;
                LocationOfFinalFile = locationoffinalfile;
                if (finalthread == null) FinalDispatcher = Dispatcher.CurrentDispatcher;
                else finalthread = FinalDispatcher;
                YTDLInstance = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = "--no-playlist " + URL + " --write-thumbnail --skip-download " + @" -o """ + LocationOfFinalFile + @"""",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                YTDLInstance.OutputDataReceived += (obj, e) =>
                {
                    if (e != null)
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            if (TrimmedOutputDataReceived != null)
                                FinalDispatcher.Invoke(() =>
                                    TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, obj }, new string[] { "ytdl_dlthumbnail", "innerprocess" })));
                            if (e.Data.Contains("Writing thumbnail to:"))
                                LocationOfFinalFile = e.Data.Substring(e.Data.IndexOf("Writing thumbnail to:") + 21).Trim();
                        }
                };
                YTDLInstance.ErrorDataReceived += (obj, e) =>
                {
                    if (e != null)
                        if (!string.IsNullOrWhiteSpace(e.Data))
                            if (TrimmedErrorDataReceived != null)
                                FinalDispatcher.Invoke(() =>
                                    TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, obj }, new string[] { "ytdl_dlthumbnail", "innerprocess" })));
                };
                YTDLInstance.Exited += (obj, e) =>
                {
                    Process p = (Process)obj;
                    if (p.ExitCode == 0) OnStatusChanged(WorkerStatus.Successful);
                    else OnStatusChanged(WorkerStatus.Error);
                    if (TrimmedOutputDataReceived != null)
                        FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs("Exit code=" + p.ExitCode, new object[] { this, obj }, new string[] { "ytdl_dl", "innerprocess" })));
                    if (Finished != null)
                        FinalDispatcher.Invoke(() => Finished?.Invoke(this, e));
                    p.Close();
                };
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
        }
        /// <summary>
        /// Class for storing a list of available formats for a given link.
        /// </summary>
        public class FormatList
        {
            /// <summary>
            /// The lists of formats, as they are gotten from youtube-dl -f.
            /// Also contains the best formats in lines starting with <c>BF</c>, <c>BV</c>, <c>BA</c>.
            /// </summary>
            public List<string> FormatsAsString { get; private set; }
            /// <summary>
            /// The list of formats, beautifully characterized.
            /// </summary>
            public List<Format> Formats { get; private set; }
            /// <summary>
            /// The ID of the best overall format.
            /// </summary>
            public string Best { get; }
            /// <summary>
            /// The ID of the best video format.
            /// </summary>
            public string BestVideo { get; }
            /// <summary>
            /// The ID of the best audio format.
            /// </summary>
            public string BestAudio { get; }
            /// <summary>
            /// Returns the list as a string, printing each <c>FormatsAsString</c> element as a new line.
            /// <para>This also includes the <c>BF</c>, <c>BV</c>, <c>BA</c> lines.</para>
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Formats.Count; i++) sb.Append(Formats[i].ToString()).Append(Environment.NewLine);
                return sb.ToString();
            }
            public FormatList(List<string> fas, string ba, string bv, string bf)
            {
                BestVideo = bv;
                BestAudio = ba;
                Best = bf;
                FormatsAsString = fas;
                Formats = new List<Format>();
                if(FormatsAsString!=null)
                    for (int i = 0; i < FormatsAsString.Count; i++)
                    {
                        string id = FormatsAsString[i].Split(' ')[0];
                        Formats.Add(new Format(FormatsAsString[i], (id == BestVideo), (id == BestAudio), (id == Best)));
                    }
            }
        }
        /// <summary>
        /// Class for detailing an instance of a given format.
        /// </summary>
        public class Format
        {
            public string ID { get; private set; }
            public string Definition { get; private set; }
            public string Extension { get; private set; }
            public string Resolution { get; private set; }
            public string ResolutionDesc { get; private set; }
            public string Bitrate { get; private set; }
            public string FPS { get; private set; }
            public string Container { get; private set; }
            public string VideoEncoding { get; private set; }
            public string AudioEncoding { get; private set; }
            public string TotalSize { get; private set; }
            public bool? IsVideo { get; private set; }
            public bool? IsAudio { get; private set; }
            public bool IsBest { get; private set; }
            public bool IsBestVideo { get; private set; }
            public bool IsBestAudio { get; private set; }
            public string InitialNote { get; private set; }
            public override string ToString()
            {
                return ID.ToString() + " " + InitialNote;
            }
            public Format(string d, bool isbestvideo = false, bool isbestaudio = false, bool isbest = false)
            {
                Definition = d;
                ID = d.Substring(0, 13).Trim();
                Extension = d.Substring(13, 11).Trim();
                InitialNote = d.Substring(24);
                IsBest = isbest;
                IsBestVideo = isbestvideo;
                IsBestAudio = isbestaudio;
            }
        }
        /// <summary>
        /// Class for getting a list of IDs from the link.
        /// </summary>
        public class GetID : IReturnData
        {
            private Process P;
            public string URL { get; }
            public List<string> IDList { get; private set; }
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            public Dispatcher FinalDispatcher { get; }
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
                    OnStatusChanged(WorkerStatus.Ready);
                return (Status == WorkerStatus.Ready);
            }
            public bool Start()
            {
                if (Status != WorkerStatus.Ready)
                    return false;
                OnStatusChanged(WorkerStatus.Running);
                IDList = new List<string>();
                P.Start();
                P.BeginErrorReadLine();
                P.BeginOutputReadLine();
                OnTrimmedOutputDataReceived("Inner process started.");
                OnTrimmedOutputDataReceived(URL);
                return true;
            }
            public GetID(string url, Dispatcher finaldispatcher = null)
            {
                if (finaldispatcher == null) FinalDispatcher = Dispatcher.CurrentDispatcher;
                else FinalDispatcher = finaldispatcher;
                this.URL = url;
                P = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = " " + URL + "-i --get-id",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                P.OutputDataReceived += P_OutputDataReceived;
                P.ErrorDataReceived += P_ErrorDataReceived; 
                P.Exited += P_Exited; 
            }
            private void P_Exited(object sender, EventArgs e)
            {
                P.OutputDataReceived -= P_OutputDataReceived;
                P.ErrorDataReceived -= P_ErrorDataReceived;
                P.Exited -= P_Exited;
                OnTrimmedOutputDataReceived("Inner process Exit code=" + P.ExitCode);
                if (P.ExitCode != 0) OnStatusChanged(WorkerStatus.Error);
                else OnStatusChanged(WorkerStatus.Successful);
                P.Close();
                if (Finished != null)
                    FinalDispatcher.Invoke(() => { Finished?.Invoke(this, EventArgs.Empty); });
            }
            private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(e);
            }
            private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                if(!string.IsNullOrWhiteSpace(e.Data))
                    IDList.Add(e.Data);
                OnTrimmedOutputDataReceived(e);
            }
            private void OnTrimmedErrorDataReceived(DataReceivedEventArgs data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data.Data))
                        if (TrimmedErrorDataReceived != null)
                            FinalDispatcher.Invoke(() => TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data.Data, new object[] { this, P } ,new string[] { "ytdl_getID", "innerpocess" })));
            }
            private void OnTrimmedErrorDataReceived(string data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data))
                        if (TrimmedErrorDataReceived != null)
                            FinalDispatcher.Invoke(()=> { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "ytdl_getID")); });
            }
            private void OnTrimmedOutputDataReceived(DataReceivedEventArgs data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data.Data))
                        if (TrimmedOutputDataReceived != null)
                            FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data.Data, new object[] { this, P }, new string[] { "ytdl_getID", "innerpocess" })));
            }
            private void OnTrimmedOutputDataReceived(string data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data))
                        if (TrimmedOutputDataReceived != null)
                            FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "ytdl_getID")); });
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
        }
        /// <summary>
        /// Class for updating the youtube-dl executable.
        /// </summary>
        public class UpdateYAYD : IReturnData
        {
            private Process P;
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            public Dispatcher FinalDispatcher { get; }
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
                    OnStatusChanged(WorkerStatus.Ready);
                return (Status == WorkerStatus.Ready);
            }
            public bool Start()
            {
                if (Status != WorkerStatus.Ready)
                    return false;
                OnStatusChanged(WorkerStatus.Running);
                P.Start();
                P.BeginErrorReadLine();
                P.BeginOutputReadLine();
                OnTrimmedOutputDataReceived("Inner process started.");
                return true;
            }
            public UpdateYAYD(Dispatcher finaldispatcher = null)
            {
                if (finaldispatcher == null) FinalDispatcher = Dispatcher.CurrentDispatcher;
                else FinalDispatcher = finaldispatcher;
                P = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"youtube-dl.exe",
                        Arguments = " -U",
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                P.OutputDataReceived += P_OutputDataReceived;
                P.ErrorDataReceived += P_ErrorDataReceived;
                P.Exited += P_Exited;
            }
            private void P_Exited(object sender, EventArgs e)
            {
                P.OutputDataReceived -= P_OutputDataReceived;
                P.ErrorDataReceived -= P_ErrorDataReceived;
                P.Exited -= P_Exited;
                OnTrimmedOutputDataReceived("Inner process Exit code=" + P.ExitCode);
                if (P.ExitCode != 0) OnStatusChanged(WorkerStatus.Error);
                else OnStatusChanged(WorkerStatus.Successful);
                P.Close();
                if (Finished != null)
                    FinalDispatcher.Invoke(() => { Finished?.Invoke(this, EventArgs.Empty); });
            }
            private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedErrorDataReceived(e);
            }
            private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                OnTrimmedOutputDataReceived(e);
            }
            private void OnTrimmedErrorDataReceived(DataReceivedEventArgs data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data.Data))
                        if (TrimmedErrorDataReceived != null)
                            FinalDispatcher.Invoke(() => TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data.Data, new object[] { this, P }, new string[] { "ytdl_getID", "innerpocess" })));
            }
            private void OnTrimmedErrorDataReceived(string data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data))
                        if (TrimmedErrorDataReceived != null)
                            FinalDispatcher.Invoke(() => { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "ytdl_getID")); });
            }
            private void OnTrimmedOutputDataReceived(DataReceivedEventArgs data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data.Data))
                        if (TrimmedOutputDataReceived != null)
                            FinalDispatcher.Invoke(() => TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data.Data, new object[] { this, P }, new string[] { "ytdl_getID", "innerpocess" })));
            }
            private void OnTrimmedOutputDataReceived(string data)
            {
                if (data != null)
                    if (!string.IsNullOrWhiteSpace(data))
                        if (TrimmedOutputDataReceived != null)
                            FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(data, this, "ytdl_getID")); });
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
        }
    }
    /// <summary>
    /// Namespace for interaction with <c>FFmpeg</c>.
    /// </summary>
    namespace FFMPEGInteract
    {
        /// <summary>
        /// Namespace for converting to mp3.
        /// </summary>
        namespace MP3Interact
        {
            /// <summary>
            /// Represents a mp3 quality format.
            /// </summary>
            public class OutputFormat
            {
                /// <summary>
                /// Possibble encoding methods.
                /// </summary>
                public enum EncodingMethod
                {
                    VBR,
                    ABR,
                    CBR
                }
                /// <summary>
                /// The <c>EncodingMethod</c> to use.
                /// </summary>
                public EncodingMethod Method { get; }
                /// <summary>
                /// The bitrate of output.
                /// </summary>
                public int BitRate { get; }
                /// <summary>
                /// The LAME option, used for <c>EncodingMethod.VBR-</c>. Lower is better.
                /// </summary>
                public int LAMEOption { get; }
                /// <summary>
                /// The sampling rate of output.
                /// </summary>
                public int SamplingRate { get; }
                /// <summary>
                /// Represents a VBR encoding with LAME option 0 and sampling rate 48000 Hz.
                /// </summary>
                public static OutputFormat DefaultOutputFormat
                {
                    get
                    {
                        return new OutputFormat(EncodingMethod.VBR, 0);
                    }
                }
                /// <summary>
                /// Outputs a string formatted for parameter parsing.
                /// </summary>
                /// <returns></returns>
                public override string ToString()
                {
                    switch (Method)
                    {
                        case EncodingMethod.VBR:
                            if ((LAMEOption > 9) || (LAMEOption < 0)) return "-q:a 0 -ar " + SamplingRate.ToString();
                            return "-c:a libmp3lame -q:a " + LAMEOption.ToString() + " -ar " + SamplingRate.ToString();
                        case EncodingMethod.ABR:
                            return "-c:a libmp3lame -abr 1 -b:a " + BitRate + " -ar " + SamplingRate.ToString();
                        case EncodingMethod.CBR:
                            return "-c:a libmp3lame -b:a " + BitRate + " -ar " + SamplingRate.ToString();
                        default:
                            return "";
                    }
                }
                /// <summary>
                /// Creates a new <c>OutputFormat</c>.
                /// </summary>
                /// <param name="em">The encoding method to use.</param>
                /// <param name="value">The value: btrate for ABR/CBR, LAME option for VBR.</param>
                /// <param name="sampling_rate">The sampling rate, default 48000 Hz.</param>
                public OutputFormat(EncodingMethod em, int value, int sampling_rate = 48000)
                {
                    Method = em;
                    SamplingRate = sampling_rate;
                    switch (em)
                    {
                        case EncodingMethod.VBR:
                            LAMEOption = value;
                            break;
                        case EncodingMethod.ABR:
                        case EncodingMethod.CBR:
                            BitRate = value;
                            break;
                        default:
                            Method = EncodingMethod.VBR;
                            LAMEOption = 0;
                            break;
                    }
                }
            }
            /// <summary>
            /// Represents the meta tags of a mp3 file.
            /// </summary>
            public class Meta
            {
                private Dictionary<string, string> guts = new Dictionary<string, string>() // why not?
                {
                    {"album", null },
                    {"composer",null },
                    {"genre",null },
                    {"copyright",null },
                    {"encoded_by", "FFmpeg with Un alt program de descarcat muzica de pe Youtube/Yet another Youtube Downloader (Vlad-Florin Chelaru)" },
                    {"title",null },
                    {"language",null },
                    //artists in their list
                    {"album_artist",null },
                    {"performer",null },
                    {"disc",null },
                    {"publisher",null },
                    {"track",null },
                    {"encoder","FFmpeg with Un alt program de descarcat muzica de pe Youtube/Yet another Youtube Downloader (Vlad-Florin Chelaru)" },
                    {"lyrics",null }
                };
                public string Album
                {
                    get
                    {
                        return guts["album"];
                    }
                    set
                    {
                        if (value != "") guts["album"] = value;
                        else guts["album"] = null;
                    }
                }
                public string Composer
                {
                    get
                    {
                        return guts["composer"];
                    }
                    set
                    {
                        if (value != "") guts["composer"] = value;
                        else guts["composer"] = null;
                    }
                }
                public string Genre
                {
                    get
                    {
                        return guts["genre"];
                    }
                    set
                    {
                        if (value != "") guts["genre"] = value;
                        else guts["genre"] = null;
                    }
                }
                public string Copyright
                {
                    get
                    {
                        return guts["copyright"];
                    }
                    set
                    {
                        if (value != "") guts["copyright"] = value;
                        else guts["copyright"] = null;
                    }
                }
                public string EncodedBy
                {
                    get
                    {
                        return guts["encoded_by"];
                    }
                    set
                    {
                        if (value != "" && value != null) guts["encoded_by"] = value;
                        else guts["encoded_by"] = "FFmpeg with Un alt program de descarcat muzica de pe Youtube/Yet another Youtube Downloader (Vlad-Florin Chelaru)";
                    }
                }
                public string Title
                {
                    get
                    {
                        return guts["title"];
                    }
                    set
                    {
                        if (value != "") guts["title"] = value;
                        else guts["title"] = null;
                    }
                }
                public string Language
                {
                    get
                    {
                        return guts["language"];
                    }
                    set
                    {
                        if (value != "") guts["language"] = value;
                        else guts["language"] = null;
                    }
                }
                public List<string> ArtistList { get; set; } = new List<string>();
                public string AlbumArtist
                {
                    get
                    {
                        return guts["album_artist"];
                    }
                    set
                    {
                        if (value != "") guts["album_artist"] = value;
                        else guts["album_artist"] = null;
                    }
                }
                public string Performer
                {
                    get
                    {
                        return guts["performer"];
                    }
                    set
                    {
                        if (value != "") guts["performer"] = value;
                        else guts["performer"] = null;
                    }
                }
                public string Disc
                {
                    get
                    {
                        return guts["disc"];
                    }
                    set
                    {
                        if (value != "") guts["disc"] = value;
                        else guts["disc"] = null;
                    }
                }
                public string Publisher
                {
                    get
                    {
                        return guts["publisher"];
                    }
                    set
                    {
                        if (value != "") guts["publisher"] = value;
                        else guts["publisher"] = null;
                    }
                }
                public string Track
                {
                    get
                    {
                        return guts["track"];
                    }
                    set
                    {
                        if (value != "") guts["track"] = value;
                        else guts["track"] = null;
                    }
                }
                public string Encoder
                {
                    get
                    {
                        return guts["encoder"];
                    }
                    set
                    {
                        if (value != "" && value != null) guts["encoder"] = value;
                        else guts["encoder"] = "FFmpeg with Un alt program de descarcat muzica de pe Youtube/Yet another Youtube Downloader (Vlad-Florin Chelaru)";
                    }
                }
                public string Lyrics
                {
                    get
                    {
                        return guts["lyrics"];
                    }
                    set
                    {
                        if (value != "") guts["lyrics"] = value;
                        else guts["lyrics"] = null;
                    }
                }
                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("-id3v2_version 3");
                    for (int i = 0; i < guts.Count; i++) if (!string.IsNullOrWhiteSpace(guts.ElementAt(i).Value)) sb.Append(" -metadata " + guts.ElementAt(i).Key + @"=""" + guts.ElementAt(i).Value + @"""");
                    if (ArtistList != null && ArtistList.Count != 0)
                    {
                        sb.Append(@" -metadata artist=""");
                        for (int i = 0; i < ArtistList.Count - 1; i++) sb.Append(ArtistList[i] + "/");
                        sb.Append(ArtistList[ArtistList.Count - 1] + @"""");
                    }
                    return sb.ToString();
                }
            }
            /// <summary>
            /// Class for converting files to mp3.
            /// </summary>
           public class Convert : IReturnData
            {
                public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
                public event EventHandler Finished;
                public event EventHandler StatusChanged;
                public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
                public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
                public event ProgressReportedEventHandler ProgressReported;
                public Process FFMPEGInstance { get; }
                public string OriginalFile { get; }
                public string FinalFile { get; }
                public string ThumbNailFile { get; }
                public OutputFormat Format { get; }
                public Meta MetaData { get; }
                public Dispatcher FinalDispatcher { get; }
                public TimeSpan TotalFileTime { get; private set; } = new TimeSpan(0);
                public bool Ready()
                {
                    if (Status == WorkerStatus.Pending)
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
                    FFMPEGInstance.Start();
                    FFMPEGInstance.BeginErrorReadLine();
                    FFMPEGInstance.BeginOutputReadLine();
                    return true;
                }
                public Convert(string original, string final, OutputFormat format, Meta meta = null, string thumb = "", Dispatcher finalthread = null)
                {
                    OriginalFile = original;
                    FinalFile = final;
                    ThumbNailFile = thumb;
                    Format = format;
                    if (meta == null) MetaData = new Meta();
                    else MetaData = meta;
                    if (finalthread != null) FinalDispatcher = finalthread;
                    else FinalDispatcher = Dispatcher.CurrentDispatcher;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"-i """ + OriginalFile + @""" ");
                    if (!string.IsNullOrWhiteSpace(ThumbNailFile)) sb.Append(@"-i """ + ThumbNailFile + @""" -map 0:a -map 1:0 -metadata:s:v title=""Album cover"" -metadata:s:v comment=""Cover(Front)"" ");
                    sb.Append(MetaData.ToString() + @" " + Format.ToString());
                    sb.Append(@" """ + FinalFile + @""" -y");
                    FFMPEGInstance = new Process()
                    {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = @"ffmpeg.exe",
                            Arguments = sb.ToString(),
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    FFMPEGInstance.OutputDataReceived += FFMPEGInstance_OutputDataReceived;
                    FFMPEGInstance.ErrorDataReceived += FFMPEGInstance_ErrorDataReceived;
                    FFMPEGInstance.Exited += FFMPEGInstance_Exited;
                }
                private void FFMPEGInstance_OutputDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (TrimmedOutputDataReceived != null)
                        if (e != null)
                            if (!string.IsNullOrWhiteSpace(e.Data))
                                FinalDispatcher.Invoke(() => TrimmedOutputDataReceived.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, sender }, new string[] { "FFmpeg_tomp3", "innerprocess" })));
                }
                private void FFMPEGInstance_ErrorDataReceived(object sender, DataReceivedEventArgs e)
                {
                    if (e != null)
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"Duration:\s*([0-9]*):([0-9]*):([0-9]*).([0-9]*)");
                            if (r.IsMatch(e.Data))
                            {
                                TimeSpan dts = new TimeSpan(0, int.Parse(r.Match(e.Data).Groups[1].Value), int.Parse(r.Match(e.Data).Groups[2].Value), int.Parse(r.Match(e.Data).Groups[3].Value), int.Parse(r.Match(e.Data).Groups[4].Value));
                                if (dts.Ticks > TotalFileTime.Ticks) TotalFileTime = dts;
                            }
                            FinalDispatcher.Invoke(() =>
                            {
                                TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e.Data, new object[] { this, sender }, new string[] { "FFmpeg_tomp3", "innerprocess" }));
                                ProgressReportedEventArgs p = ProgressReportedEventArgs.PRFromFFMPEGConverter(e.Data, TotalFileTime);
                                if (p.Percent != 0)
                                    ProgressReported?.Invoke(this, p);
                            });
                        }
                }
                private void FFMPEGInstance_Exited(object sender, EventArgs e)
                {
                    if (FFMPEGInstance.ExitCode == 0) OnStatusChanged(WorkerStatus.Successful);
                    else OnStatusChanged(WorkerStatus.Error);
                    if (TrimmedOutputDataReceived != null)
                        FinalDispatcher.Invoke(() => TrimmedOutputDataReceived.Invoke(this, new AdvancedDataReceivedEventArgs("Exit code=" + FFMPEGInstance.ExitCode, new object[] { this, sender }, new string[] { "FFmpeg_tomp3", "innerprocess" })));
                    if (Finished != null)
                        FinalDispatcher.Invoke(() => { Finished?.Invoke(this, e); });
                    FFMPEGInstance.OutputDataReceived -= FFMPEGInstance_OutputDataReceived;
                    FFMPEGInstance.ErrorDataReceived -= FFMPEGInstance_ErrorDataReceived;
                    FFMPEGInstance.Exited -= FFMPEGInstance_Exited;
                    FFMPEGInstance.Close();
                }
                private void OnStatusChanged(WorkerStatus s)
                {
                    Status = s;
                    if (StatusChanged != null)
                        FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
                }
            }
        }
    }
    namespace WebInteract
    {
        public class DownloadThumbNail : IReturnData
        {
            public WorkerStatus Status { get; private set; } = WorkerStatus.Pending;
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public System.Net.WebClient WebClient { get; }
            public string URL { get; }
            public string LocationOfFinalFile { get; private set; }
            public Dispatcher FinalDispatcher { get; }
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
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
                WebClient.DownloadFileAsync(new Uri(URL), LocationOfFinalFile);
                return true;
            }
            public DownloadThumbNail(string url, string locationoffinalfile, Dispatcher finalthread = null)
            {
                if (finalthread == null)
                    FinalDispatcher = Dispatcher.CurrentDispatcher;
                else
                    FinalDispatcher = finalthread;
                URL = url;
                LocationOfFinalFile = locationoffinalfile;
                WebClient = new System.Net.WebClient();
                WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                WebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            }
            private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                if (e.Error!=null)
                {
                    OnTrimmedErrorDataReceived(e.ToString());
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                }
                else
                {
                    OnTrimmedOutputDataReceived("Finished!");
                    WebClient.Dispose();
                    WebClient.DownloadProgressChanged -= WebClient_DownloadProgressChanged;
                    WebClient.DownloadFileCompleted -= WebClient_DownloadFileCompleted;
                    OnStatusChanged(WorkerStatus.Successful);
                    OnFinished();
                }
            }
            private void WebClient_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
            {
                OnTrimmedOutputDataReceived("Downloaded " + e.BytesReceived + " bytes out of " + e.TotalBytesToReceive + " bytes, finished " + e.ProgressPercentage + "%");
                if (ProgressReported != null)
                    FinalDispatcher.Invoke(() => { ProgressReported?.Invoke(this, ProgressReportedEventArgs.PROnlyPercent(e.ProgressPercentage)); });
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
            private void OnFinished()
            {
                if (Finished != null)
                    FinalDispatcher.Invoke(() => { Finished?.Invoke(this, EventArgs.Empty); });
            }
            private void OnTrimmedOutputDataReceived(string message)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    if (TrimmedOutputDataReceived != null)
                        FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(message, this, "WebClient_dlthumbnail")); });
            }
            private void OnTrimmedErrorDataReceived(string message)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    if (TrimmedErrorDataReceived != null)
                        FinalDispatcher.Invoke(() => { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(message, this,  "WebClient_dlthumbnail")); });
            }
        }
    }
    namespace Orchestrators
    {
        public class AdHocDownLoadAndConvertMP3 : IReturnData
        {
            public WorkerStatus Status { get; private set; }
            public Dispatcher FinalDispatcher { get; }
            public event TrimmedDataReceivedEventHandler TrimmedOutputDataReceived;
            public event TrimmedDataReceivedEventHandler TrimmedErrorDataReceived;
            public event ProgressReportedEventHandler ProgressReported;
            public event EventHandler Finished;
            public event EventHandler StatusChanged;
            public event EventHandler GetMetaFinished;
            public event EventHandler DownloadFinished;
            public event EventHandler DownloadThumbNailFinished;
            public event EventHandler ConvertFinished;
            public YAYD.YTDLInteract.GetMeta GetMeta { get; private set; }
            public YAYD.YTDLInteract.Download Download { get; private set; }
            public YAYD.WebInteract.DownloadThumbNail DownloadThumbNail { get; private set; }
            public YAYD.FFMPEGInteract.MP3Interact.Convert Convert { get; private set; }
            public System.IO.FileInfo LocationOfFinalFile { get; private set; }
            public System.IO.DirectoryInfo LocationOfTempDirectory { get; private set; }
            public string URL { get; }
            public bool Ready()
            {
                if (Status == WorkerStatus.Pending)
                    Status = WorkerStatus.Ready;
                return (Status == WorkerStatus.Ready);
            }
            public bool Start()
            {
                if (Status != WorkerStatus.Ready)
                    return false;
                OnStatusChanged(WorkerStatus.Running);
                GetMeta.Start();
                return true;
            }
            public AdHocDownLoadAndConvertMP3(string url, string loc, Dispatcher fidisp = null)
            {
                if (fidisp == null)
                    FinalDispatcher = Dispatcher.CurrentDispatcher;
                else
                    FinalDispatcher = fidisp;
                URL = url;
                LocationOfFinalFile = new System.IO.FileInfo(loc);
                OnTrimmedOutputDataReceived("Init URL " + URL + " LOFF " + LocationOfFinalFile.FullName);
                GetMeta = new YTDLInteract.GetMeta(URL, FinalDispatcher);
                GetMeta.TrimmedOutputDataReceived += OnTrimmedOutputDataReceived;
                GetMeta.TrimmedErrorDataReceived += OnTrimmedErrorDataReceived;
                GetMeta.ProgressReported += GetMeta_ProgressReported;
                GetMeta.Finished += GetMeta_Finished;
                GetMeta.Ready();
            }
            private void GetMeta_ProgressReported(object sender, ProgressReportedEventArgs e)
            {
                OnProgressReported(e.Percent * 0.25);
            }
            private void GetMeta_Finished(object sender, EventArgs e)
            {
                GetMeta.TrimmedOutputDataReceived -= OnTrimmedOutputDataReceived;
                GetMeta.TrimmedErrorDataReceived -= OnTrimmedErrorDataReceived;
                GetMeta.ProgressReported -= GetMeta_ProgressReported;
                GetMeta.Finished -= GetMeta_Finished;
                if (GetMeta.Status == WorkerStatus.Successful)
                    OnTrimmedOutputDataReceived("GetMeta is successful and I'm happy.😊");
                else
                {
                    OnTrimmedErrorDataReceived("GetMeta failed.");
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                LocationOfTempDirectory = new System.IO.DirectoryInfo(LocationOfFinalFile.DirectoryName + @"\YAYD_TEMP3DL_" + GetMeta.ID);
                if (GetMetaFinished != null)
                    FinalDispatcher.Invoke(() => { GetMetaFinished?.Invoke(this, EventArgs.Empty); });
                if (LocationOfFinalFile.Name == "default" || LocationOfFinalFile.Name == "default.mp3")
                    OnTrimmedOutputDataReceived("The LOFF is default, skipping check of preexistent final file.");
                else if (LocationOfFinalFile.Exists)
                {
                    OnTrimmedOutputDataReceived("Final file already exists. Deleting it.");
                    try
                    {
                        LocationOfFinalFile.Delete();
                    }
                    catch (Exception exc)
                    {
                        OnTrimmedErrorDataReceived("Unable to delete final file already present: " + exc.ToString());
                        OnStatusChanged(WorkerStatus.Error);
                        OnFinished();
                        return;
                    }
                }
                else
                    OnTrimmedOutputDataReceived("Final file doesn't exist and I'm happy.😊");
                if (LocationOfTempDirectory.Exists)
                {
                    OnTrimmedErrorDataReceived("Temp dir exists already. This might mean that another download of the same video is already running.");
                    OnTrimmedErrorDataReceived("If this is not the case, manually delete the temp folder and try again.");
                    OnTrimmedErrorDataReceived("Location: " + LocationOfTempDirectory.FullName);
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                // COMMENTED OUT: the fact that this temp dir already exists means that there is another download of the same file running already. The current download needs to be aborted.
                //{
                //    OnTrimmedOutputDataReceived("Temp directory already exists. Deleting it.");
                //    try
                //    {
                //        LocationOfTempDirectory.Delete(true);
                //    }
                //    catch (Exception exc)
                //    {
                //        OnTrimmedErrorDataReceived("Unable to delete temp dir already present: " + exc.ToString());
                //        OnStatusChanged(WorkerStatus.Error);
                //        OnFinished();
                //        return;
                //    }
                //}
                else
                    OnTrimmedOutputDataReceived("Temp directory doesn't exist and I'm happy.😊");
                OnTrimmedOutputDataReceived("Creating new temp directory.");
                try
                {
                    LocationOfTempDirectory.Create();
                }
                catch (Exception exc)
                {
                    OnTrimmedErrorDataReceived("Unable to create new temp dir: " + exc.ToString());
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                OnTrimmedOutputDataReceived("Temp dir created and I'm happy.😊");
                try
                {
                    System.IO.Directory.CreateDirectory(LocationOfTempDirectory.FullName + @"\audio");
                }
                catch (Exception exc)
                {
                    OnTrimmedErrorDataReceived("Unable to create audio folder in temp dir: " + exc.ToString());
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                OnTrimmedOutputDataReceived("Audio folder in temp dir created and I'm happy.😊");
                Download = new YTDLInteract.Download(URL, "bestaudio", LocationOfTempDirectory.FullName + @"\audio\%(title)s");
                Download.TrimmedOutputDataReceived += OnTrimmedOutputDataReceived;
                Download.TrimmedErrorDataReceived += OnTrimmedErrorDataReceived;
                Download.ProgressReported += Download_ProgressReported;
                Download.Finished += Download_Finished;
                Download.Ready();
                Download.Start();
            }
            private void Download_Finished(object sender, EventArgs e)
            {
                Download.TrimmedOutputDataReceived -= OnTrimmedOutputDataReceived;
                Download.TrimmedErrorDataReceived -= OnTrimmedErrorDataReceived;
                Download.ProgressReported -= Download_ProgressReported;
                Download.Finished -= Download_Finished;
                if (Download.Status == WorkerStatus.Successful)
                    OnTrimmedOutputDataReceived("Download is successful and I'm happy.😊");
                else
                {
                    OnTrimmedErrorDataReceived("Download failed.");
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                if(LocationOfFinalFile.Name=="default" || LocationOfFinalFile.Name == "default.mp3")
                {
                    string realname = new System.IO.FileInfo(System.IO.Directory.GetFiles(LocationOfTempDirectory.FullName + @"\audio")[0]).Name;
                    LocationOfFinalFile = new System.IO.FileInfo(LocationOfFinalFile.Directory + @"\" + realname + ".mp3");
                    OnTrimmedOutputDataReceived("LOFF was default, now changed to:" + LocationOfFinalFile.FullName);
                }
                if (DownloadFinished != null)
                    FinalDispatcher.Invoke(() => { DownloadFinished?.Invoke(this, EventArgs.Empty); });
                if (Properties.Settings.Default.IncludeThumbnailInAdHocDownloadAndConvert)
                {
                    DownloadThumbNail = new WebInteract.DownloadThumbNail(GetMeta.ThumbnailURL, LocationOfTempDirectory.FullName + @"\thumbnail");
                    DownloadThumbNail.TrimmedOutputDataReceived += OnTrimmedOutputDataReceived;
                    DownloadThumbNail.TrimmedErrorDataReceived += OnTrimmedErrorDataReceived;
                    DownloadThumbNail.ProgressReported += DownloadThumbNail_ProgressReported;
                    DownloadThumbNail.Finished += DownloadThumbNail_Finished;
                    DownloadThumbNail.Ready();
                    DownloadThumbNail.Start();
                }
                else
                {
                    Convert = new FFMPEGInteract.MP3Interact.Convert(LocationOfTempDirectory.FullName + @"\audio\" + System.IO.Path.GetFileNameWithoutExtension(LocationOfFinalFile.FullName), LocationOfFinalFile.FullName, FFMPEGInteract.MP3Interact.OutputFormat.DefaultOutputFormat, null);
                    Convert.TrimmedOutputDataReceived += OnTrimmedOutputDataReceived;
                    Convert.TrimmedErrorDataReceived += OnTrimmedErrorDataReceived;
                    Convert.ProgressReported += Convert_ProgressReported;
                    Convert.Finished += Convert_Finished;
                    Convert.Ready();
                    Convert.Start();
                }
            }
            private void DownloadThumbNail_Finished(object sender, EventArgs e)
            {
                DownloadThumbNail.TrimmedOutputDataReceived -= OnTrimmedOutputDataReceived;
                DownloadThumbNail.TrimmedErrorDataReceived -= OnTrimmedErrorDataReceived;
                DownloadThumbNail.ProgressReported -= DownloadThumbNail_ProgressReported;
                DownloadThumbNail.Finished -= DownloadThumbNail_Finished;
                if (DownloadThumbNail.Status == WorkerStatus.Successful)
                    OnTrimmedOutputDataReceived("Download of the thumbnail is successful and I'm happy.😊");
                else
                {
                    OnTrimmedErrorDataReceived("Download failed.");
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                if (DownloadThumbNailFinished != null)
                    FinalDispatcher.Invoke(() => { DownloadThumbNailFinished?.Invoke(this, EventArgs.Empty); });
                Convert = new FFMPEGInteract.MP3Interact.Convert(LocationOfTempDirectory.FullName + @"\audio\" + System.IO.Path.GetFileNameWithoutExtension(LocationOfFinalFile.FullName), LocationOfFinalFile.FullName, FFMPEGInteract.MP3Interact.OutputFormat.DefaultOutputFormat, null, LocationOfTempDirectory.FullName + @"\thumbnail");
                Convert.TrimmedOutputDataReceived += OnTrimmedOutputDataReceived;
                Convert.TrimmedErrorDataReceived += OnTrimmedErrorDataReceived;
                Convert.ProgressReported += Convert_ProgressReported;
                Convert.Finished += Convert_Finished;
                Convert.Ready();
                Convert.Start();
            }
            private void Convert_Finished(object sender, EventArgs e)
            {
                Convert.TrimmedOutputDataReceived -= OnTrimmedOutputDataReceived;
                Convert.TrimmedErrorDataReceived -= OnTrimmedErrorDataReceived;
                Convert.ProgressReported -= Convert_ProgressReported;
                Convert.Finished -= Convert_Finished;
                if (Convert.Status == WorkerStatus.Successful)
                    OnTrimmedOutputDataReceived("Conversion is successful and I'm happy.😊");
                else
                {
                    OnTrimmedErrorDataReceived("Convertion failed.");
                    OnStatusChanged(WorkerStatus.Error);
                    OnFinished();
                    return;
                }
                OnTrimmedOutputDataReceived("Everything is finished and I'm happy.😊");
                try
                {
                    LocationOfTempDirectory.Delete(true);
                    OnTrimmedOutputDataReceived("Temp dir removed.");
                }
                catch (Exception exc)
                {
                    OnTrimmedErrorDataReceived("Unable to delete temp dir, but it doesn't matter for now.");
                    OnTrimmedErrorDataReceived("Future downloads of the same video might fail because of this temp dir.");
                    OnTrimmedErrorDataReceived("Reason: " + exc.ToString());
                    OnTrimmedErrorDataReceived("Location of temp dir: " + LocationOfTempDirectory.FullName);
                }
                if (ConvertFinished != null)
                    FinalDispatcher.Invoke(() => { ConvertFinished?.Invoke(this, EventArgs.Empty); });
                OnStatusChanged(WorkerStatus.Successful);
                OnFinished();
            }
            private void Convert_ProgressReported(object sender, ProgressReportedEventArgs e)
            {
                OnProgressReported(e.Percent * 0.25 + 75);
            }
            private void DownloadThumbNail_ProgressReported(object sender, ProgressReportedEventArgs e)
            {
                OnProgressReported(e.Percent * 0.25 + 50);
            }
            private void Download_ProgressReported(object sender, ProgressReportedEventArgs e)
            {
                OnProgressReported(e.Percent * 0.25+25);
            }
            private void OnTrimmedOutputDataReceived(string message)
            {
                if (TrimmedOutputDataReceived != null)
                    if (!string.IsNullOrWhiteSpace(message))
                        FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(message,this,"AdHocDownLoadAndConvertMP3")); });
            }
            private void OnTrimmedErrorDataReceived(string message)
            {
                if (TrimmedErrorDataReceived != null)
                    if (!string.IsNullOrWhiteSpace(message))
                        FinalDispatcher.Invoke(() => { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(message, this, "AdHocDownLoadAndConvertMP3")); });
            }
            private void OnTrimmedOutputDataReceived(object s, AdvancedDataReceivedEventArgs e)
            {
                if (TrimmedOutputDataReceived != null)
                    FinalDispatcher.Invoke(() => { TrimmedOutputDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e, this, "AdHocDownLoadAndConvertMP3")); });
            }
            private void OnTrimmedErrorDataReceived(object s, AdvancedDataReceivedEventArgs e)
            {
                if (TrimmedErrorDataReceived != null)
                    FinalDispatcher.Invoke(() => { TrimmedErrorDataReceived?.Invoke(this, new AdvancedDataReceivedEventArgs(e, this, "AdHocDownLoadAndConvertMP3")); });
            }
            private void OnStatusChanged(WorkerStatus s)
            {
                Status = s;
                if (StatusChanged != null)
                    FinalDispatcher.Invoke(() => { StatusChanged?.Invoke(this, EventArgs.Empty); });
            }
            private void OnFinished()
            {
                if (Finished != null)
                    FinalDispatcher.Invoke(() => { Finished?.Invoke(this, EventArgs.Empty); });
            }
            private void OnProgressReported(double val)
            {
                if (ProgressReported != null)
                    FinalDispatcher.Invoke(() => { ProgressReported?.Invoke(this, ProgressReportedEventArgs.PROnlyPercent(val)); });
            }
        }
    }
}
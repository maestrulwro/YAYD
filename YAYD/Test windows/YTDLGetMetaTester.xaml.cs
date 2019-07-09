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
    /// Interaction logic for YTDLInteraction_manual_tester.xaml
    /// </summary>
    public partial class YTDLGetMetaTester : Window
    {
        YAYD.YTDLInteract.GetMeta gm;
        public YTDLGetMetaTester()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Output.Text = "";
            DataGotten.Text = "";
            FormatTable.Items.Clear();
            gm = new YTDLInteract.GetMeta(URLInput.Text);
            gm.TrimmedOutputDataReceived += (sender2, e2) => W("out", e2.FullData);
            gm.TrimmedErrorDataReceived += (sender2, e2) => W("err", e2.FullData);
            gm.ProgressReported += (sender2, e2) => Progress.Value = e2.Percent;
            gm.ProgressReported += (sender2, e2) => W(e2.Percent.ToString());
            gm.Finished += Gm_Finished;
            gm.Ready();
            gm.Start();
        }
        private void Gm_Finished(object sender, EventArgs e)
        {
            W("Finished!");
            DataGotten.Text = "Title:" + gm.Title + Environment.NewLine + "ID:" + gm.ID + Environment.NewLine + "Duration:" + gm.Duration + Environment.NewLine + "Thumbnail:" + gm.ThumbnailURL + Environment.NewLine + "BV" + gm.Formats.BestVideo.ToString() + Environment.NewLine + "BA" + gm.Formats.BestAudio.ToString() + Environment.NewLine + "BF" + gm.Formats.Best.ToString()+Environment.NewLine;
            foreach (YAYD.YTDLInteract.Format elem in gm.Formats.Formats) FormatTable.Items.Add(elem);
            foreach (string elem in gm.Formats.FormatsAsString) DataGotten.Text += elem + Environment.NewLine;
        }
        public void W(string s)
        {
            Output.Text += s + Environment.NewLine;
            ((ScrollViewer)Output.Parent).ScrollToBottom();
        }
        public void W(string s1, string s2)
        {
            W(s1 + ">" + s2);
        }
    }
}

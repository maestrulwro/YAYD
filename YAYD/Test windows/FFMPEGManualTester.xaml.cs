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
    /// Interaction logic for ManualFFMPEG.xaml
    /// </summary>
    public partial class FFMPEGManualTester : Window
    {
        public FFMPEGManualTester()
        {
            InitializeComponent();
        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            FFMPEGInteract.MP3Interact.Meta meta = new FFMPEGInteract.MP3Interact.Meta()
            {
                Album = "Merk & Kremont's album",
                Composer = "Merk & Kremont probably",
                Genre = "EDM surely",
                Copyright = "Thank you for no copyright, Spinnin' Records!",
                Title = "GANG",
                Language = "Surely not money's language",
                ArtistList = new List<string>() { "Merk & Kremont", "Kris Kiss" },
                AlbumArtist = "Merk & Kremont",
                Performer = "all of them",
                Disc = "0",
                Publisher = "Spinnin' Copyright Free Music",
                Track = "69",
                Lyrics = "Very good, indeed!"
            };
            FFMPEGInteract.MP3Interact.Convert cnv = new FFMPEGInteract.MP3Interact.Convert("ellow_audio.webm", "ellow_final.mp3", new FFMPEGInteract.MP3Interact.OutputFormat(FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.VBR, 0), meta, "ellow_thumbnail.jpg");
            cnv.TrimmedOutputDataReceived += (s2, e2) => W("out",e2.FullData);
            cnv.TrimmedErrorDataReceived += (s2, e2) => W("err", e2.FullData);
            cnv.ProgressReported += (s2, e2) => {
                Progress.Value = e2.Percent;
                Frame.Text = e2.Frame.ToString();
                FPS.Text = e2.FPS.ToString();
                Q.Text = e2.Q.ToString();
                Size.Text = e2.Size;
                Time.Text = e2.Time.ToString();
                Bitrate.Text = e2.Bitrate;
                Speed.Text = e2.Speed;
            };
            cnv.Finished += (s2, e2) => W("Finished!");
        }
        private void W(string s)
        {
            Log.Text += s + Environment.NewLine;
            ((ScrollViewer)Log.Parent).ScrollToBottom();
        }
        private void W(string s1, string s2)
        {
            W(s1 + ">" + s2);
        }
    }
}

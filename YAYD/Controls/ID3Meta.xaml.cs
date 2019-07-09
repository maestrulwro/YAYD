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

namespace YAYD.Controls
{
    /// <summary>
    /// Interaction logic for ID3Meta.xaml
    /// </summary>
    public partial class ID3Meta : UserControl
    {
        public ID3Meta()
        {
            InitializeComponent();
        }
        public YAYD.FFMPEGInteract.MP3Interact.Meta Meta
        {
            get
            {
                return new FFMPEGInteract.MP3Interact.Meta()
                {
                    Album = ID3Album.Text,
                    Composer = ID3Composer.Text,
                    Genre = ID3Genre.Text,
                    Copyright = ID3Copyright.Text,
                    Title = ID3Title.Text,
                    Language = ID3Lang.Text,
                    ArtistList = new List<string>(new string[] { ID3Artists.Text }),
                    AlbumArtist = ID3AlbumArtist.Text,
                    Performer = ID3Performer.Text,
                    Disc = ID3Disc.Text,
                    Publisher = ID3Publisher.Text,
                    Track = ID3Track.Text,
                    Lyrics = ID3Lyrics.Text
                };
            }
            set
            {
                ID3Album.Text = value.Album;
                ID3Composer.Text = value.Composer;
                ID3Genre.Text = value.Genre;
                ID3Copyright.Text = value.Copyright;
                ID3Title.Text = value.Title;
                ID3Lang.Text = value.Language;
                if (value.ArtistList.Count != 0) ID3Artists.Text = value.ArtistList[0];
                for (int i = 1; i < value.ArtistList.Count; i++) ID3Artists.Text += "/" + value.ArtistList[i];
                ID3AlbumArtist.Text = value.AlbumArtist;
                ID3Performer.Text = value.Performer;
                ID3Disc.Text = value.Disc;
                ID3Publisher.Text = value.Publisher;
                ID3Track.Text = value.Track;
                ID3Lyrics.Text = value.Lyrics;
            }
        }
        public bool UseThumbnail
        {
            get { if (SaveThumbnail.IsChecked == true) return true; else return false; }
            set { SaveThumbnail.IsChecked = value; }
        }
    }
}

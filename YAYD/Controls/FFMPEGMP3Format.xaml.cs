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
    /// Interaction logic for FFMPEGMP3Format.xaml
    /// </summary>
    public partial class FFMPEGMP3Format : UserControl
    {
        public FFMPEGMP3Format()
        {
            InitializeComponent();
            Format = FFMPEGInteract.MP3Interact.OutputFormat.DefaultOutputFormat;
        }
        public FFMPEGInteract.MP3Interact.OutputFormat Format
        {
            get
            {
                if ((int.TryParse(Value.Text, out int val)) && (int.TryParse(SamplingRate.Text, out int sr)))
                    return new FFMPEGInteract.MP3Interact.OutputFormat(EncodingMethod, val, sr);
                else return FFMPEGInteract.MP3Interact.OutputFormat.DefaultOutputFormat;
            }
                set
            {
                switch (value.Method)
                {
                    case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.VBR:
                        EncMeth.SelectedIndex = 1;
                        break;
                    case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.ABR:
                        EncMeth.SelectedIndex = 2;
                        break;
                    case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.CBR:
                        EncMeth.SelectedIndex = 0;
                        break;
                }
                Value.Text = value.LAMEOption.ToString();
                SamplingRate.Text = value.SamplingRate.ToString();
            }
        }

        private FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod EncodingMethod
        {
            get
            {
                switch (EncMeth.SelectedIndex)
                {
                    case 0:
                        return FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.CBR;
                    case 1:
                        return FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.VBR;
                    case 2:
                        return FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.ABR;
                    default:
                        EncMeth.SelectedIndex = 1;
                        return FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.VBR;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (EncodingMethod)
            {
                case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.CBR:
                case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.ABR:
                    LabelForValue.Text = "Bit rate value (bps, bits per second)";
                    Value.Text = "320000";
                    break;
                case FFMPEGInteract.MP3Interact.OutputFormat.EncodingMethod.VBR:
                    LabelForValue.Text = "Quality level (0-9, 0 is best)";
                    Value.Text = "0";
                    break;
            }
        }

        private void Value_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

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
    /// Interaction logic for SimpleTextShow.xaml
    /// </summary>
    public partial class JointTextShow : Window
    {
        public string Text
        {
            get
            {
                return TB.Text;
            }
            set
            {
                TB.Text = value;
            }
        }
        public JointTextShow(string alreadythere = "")
        {
            InitializeComponent();
            TB.Text = alreadythere;
        }
        public void AppendOutput(string appended = "")
        {
            TB.Inlines.Add(appended + Environment.NewLine);
            if (AlwaysLastLine.IsChecked == true)
                TBParent.ScrollToBottom();
        }
        public void AppendError(string appended = "")
        {
            TB.Inlines.Add(new Run(appended + Environment.NewLine) { Foreground = Brushes.Red });
            if (AlwaysLastLine.IsChecked == true)
                TBParent.ScrollToBottom();
        }
        public void ForceClose()
        {
            Closing -= Window_Closing;
            this.Close();
        }
        private void SaveLog_Click(object sender, RoutedEventArgs e)
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
                    sw.Write(Text);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}

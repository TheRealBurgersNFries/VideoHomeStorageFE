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
using System.Drawing;
using System.IO;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for StreamOutputWindow.xaml
    /// </summary>
    public partial class StreamOutputWindow : Window
    {
        private MemoryStream imageStream = new MemoryStream();
        public StreamOutputWindow()
        {
            InitializeComponent();
        }

        public void UpdateImageStream(Bitmap NewImage)
        {
            NewImage.Save(imageStream, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public void UpdateImage(object sender, RoutedEventArgs e)
        {

        }
    }
}

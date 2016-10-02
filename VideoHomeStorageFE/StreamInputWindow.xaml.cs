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
using AForge.Video.DirectShow;
using AForge.Controls;
using AForge.Video;
using System.Drawing;
using System.IO;
using System.Threading;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for StreamInputWindow.xaml
    /// </summary>
    public partial class StreamInputWindow : Window
    {
        private FilterInfoCollection Cards;
        public StreamInputWindow()
        {
            InitializeComponent();
            Cards = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in Cards)
            {
                DeviceComboBox.Items.Add(device.Name);
            }
            DeviceComboBox.SelectedIndex = 0;
            BeginStreamButton.Click += BeginStreamButton_Click;
        }

        private void BeginStreamButton_Click(object sender, RoutedEventArgs e)
        {
            VideoCaptureDevice FinalVideo = new VideoCaptureDevice(Cards[DeviceComboBox.SelectedIndex].MonikerString);
            VideoStreamPlayer.VideoSource = FinalVideo;
            VideoStreamPlayer.NewFrame += VideoStreamPlayer_NewFrame;
            VideoStreamPlayer.Start();
        }

        private void VideoStreamPlayer_NewFrame(object sender, ref Bitmap image)
        {
            
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}

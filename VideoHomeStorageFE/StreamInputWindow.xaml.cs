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
using AForge.Imaging.Filters;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using AForge.Imaging;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for StreamInputWindow.xaml
    /// </summary>
    public partial class StreamInputWindow : Window
    {
        private FilterInfoCollection Cards;
        Bitmap LastFrame;
        private List<Bitmap> Images = new List<Bitmap>();
        private VHSEncoder Encoder = null;
        private int Frames;
        private int Bytes;
        private int LastFrameBytes;
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
            FinalVideo.CrossbarVideoInput = FinalVideo.AvailableCrossbarVideoInputs[0];
            VideoStreamPlayer.VideoSource = FinalVideo;
            VideoStreamPlayer.NewFrame += VideoStreamPlayer_NewFrame;
            VideoStreamPlayer.Start();
        }

        private void VideoStreamPlayer_NewFrame(object sender, ref Bitmap image)
        {
            
            if (LastFrame != null)
            {
                ThresholdedDifference filter = new ThresholdedDifference(20);
                filter.OverlayImage = LastFrame;
                Bitmap resultImage = filter.Apply(image);
                int whiteColor = 0;
                int blackColor = 0;
                for (int x = 0; x < resultImage.Width; x++)
                {
                    for (int y = 0; y < resultImage.Height; y++)
                    {
                        System.Drawing.Color color = resultImage.GetPixel(x, y);

                        if (color.ToArgb() == System.Drawing.Color.White.ToArgb())
                        {
                            whiteColor++;
                        }

                        else
                            if (color.ToArgb() == System.Drawing.Color.White.ToArgb())
                        {
                            blackColor++;
                        }
                    }
                }
                if (Encoder == null)
                {
                    VHSEncoder Header = new VHSEncoder(4, 1, VHSEncoder.BitDepth.nibble, false);
                    int error;
                    byte[] header = Header.Decode(image, 14, out error);
                    byte[] bytesPerFrameByte = new byte[4];
                    byte[] bytesLastFrameByte = new byte[4];
                    byte[] countFrameByte = new byte[4];
                    Array.Copy(header, 0, bytesPerFrameByte, 0, 4);
                    Array.Copy(header, 4, bytesLastFrameByte, 0, 4);
                    Array.Copy(header, 8, countFrameByte, 0, 4);
                    byte blockByte = header[12];
                    byte rowByte = header[13];
                    byte parityByte = header[14];
                    Bytes = BitConverter.ToInt32(bytesPerFrameByte, 0);
                    LastFrameBytes = BitConverter.ToInt32(bytesLastFrameByte, 0);
                    Frames = BitConverter.ToInt32(countFrameByte, 0);
                    int blocks = (int)blockByte;
                    int rows = (int)rowByte;
                    bool parity = Convert.ToBoolean(parityByte);

                    Encoder = new VHSEncoder(blocks, rows, VHSEncoder.BitDepth.byt, parity);
                }
                else if (whiteColor > blackColor)
                {
                    Images.Add(image);
                    if (Images.Count() == Frames)
                    {
                        VideoStreamPlayer.Stop();
                        DecodeStream();
                    }
                }

            }            
            LastFrame = image;
        }

        private void DecodeStream()
        {
            byte[] fullStream = new byte[(Frames - 1) * Bytes + LastFrameBytes];
            for (int imagen = 0; imagen < Images.Count(); imagen++)
            {
                var image = Images[imagen];
                int error;
                if (imagen < Images.Count() - 1)
                {
                    byte[] imageStream = Encoder.Decode(image, Bytes, out error);
                    Array.Copy(imageStream, 0, fullStream, imagen * Bytes, Bytes);
                }
                else
                {
                    byte[] imageStream = Encoder.Decode(image, LastFrameBytes, out error);
                    Array.Copy(imageStream, 0, fullStream, imagen * Bytes, LastFrameBytes);
                }
            }
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllBytes(dlg.FileName, fullStream);
            }
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

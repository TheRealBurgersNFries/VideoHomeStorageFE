﻿using System;
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
            }            
            LastFrame = image;
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
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
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for StreamOutputWindow.xaml
    /// </summary>
    public partial class StreamOutputWindow : Window
    {
        private TaskCompletionSource<bool> StreamBegin = new TaskCompletionSource<bool>();
        private bool StreamHasBegun = false;
        private int RowCount;
        private int BlockCount;
        private bool ParityEnabled;
        private byte[] FileBytes;

        public StreamOutputWindow(int rowCount, int blockCount, bool parityEnabled, byte[] fileBytes)
        {
            InitializeComponent();
            RowCount = rowCount;
            BlockCount = blockCount;
            ParityEnabled = parityEnabled;
            FileBytes = fileBytes;
            BeginStreamButton.Click += BeginStreamButton_Click;

        }

        internal void BeginStreamButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateImage();
        }

        public  async void UpdateImage()
        {
            try
            {
                // Usee the encoder to turn bytes into an image
                VHSEncoder Header = new VHSEncoder(4, 1, VHSEncoder.BitDepth.nibble, false);
                VHSEncoder Encoder = new VHSEncoder(RowCount, BlockCount, VHSEncoder.BitDepth.byt, ParityEnabled);
                byte[] perFrame = BitConverter.GetBytes(Encoder.BytesPerFrame);
                byte[] lastFrame = BitConverter.GetBytes(FileBytes.Length % Encoder.BytesPerFrame);
                byte block = (byte)BlockCount;
                byte row = (byte)RowCount;
                byte parity = Convert.ToByte(ParityEnabled);
                byte[] headerEncoder = new byte[16];
                Array.Copy(perFrame, headerEncoder, 4);
                Array.Copy(lastFrame, 0, headerEncoder, 4, 4);
                headerEncoder[8] = block;
                headerEncoder[9] = row;
                headerEncoder[10] = parity;

                StreamBox.Source = BitmapToImageSource(await Header.Encode(headerEncoder));
                byte[] frame;
                Thread.Sleep(1000);
                for (int bytes = 0; bytes < FileBytes.Count(); bytes += Encoder.BytesPerFrame)
                {
                    frame = new byte[Encoder.BytesPerFrame];
                    Array.Copy(FileBytes, bytes, frame, 0, Encoder.BytesPerFrame);
                    var image = BitmapToImageSource(await Encoder.Encode(frame));
                    await Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            StreamBox.Source = image;
                        }));
                        Thread.Sleep(132);
                    });
                }
            }
            catch (Exception exception) { Debug.WriteLine(exception.Message); }
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

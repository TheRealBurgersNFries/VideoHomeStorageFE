using System;
using System.Drawing;
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
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int RowCount = 0;
        private int BlockCount = 0;
        private bool ParityEnabled = false;
        private string FileName = "";
        private byte[] FileBytes = null;
        private Bitmap OutputImage = null;
        public MainWindow()
        {
            InitializeComponent();
            // Set the Values of the input boxesot the default values of the variables they correspond to.
            BlockCountTextBox.Text = BlockCount.ToString(); 
            RowCountTextBox.Text = RowCount.ToString();
            FileLocationTextBox.Text = FileName;
            ParityCheckBox.IsChecked = ParityEnabled;
            // Set the functions of the increment buttons
            IncrementBlockButton.Click += IncrementBlockButton_Click;
            IncrementRowButton.Click += IncrementRowButton_Click;
            // Set the functions of the decrement buttons
            DecrementBlockButton.Click += DecrementBlockButton_Click;
            DecrementRowButton.Click += DecrementRowButton_Click;
            // Set the function of changing the ParityCheckBox
            ParityCheckBox.Checked += ParityCheckBox_ValueChange;
            ParityCheckBox.Unchecked += ParityCheckBox_ValueChange;
            // Set the functions for the browse and submit values
            BrowseButton.Click += BrowseButton_Click;
            SubmitButton.Click += SubmitButton_Click;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Actually set the variable equal to the input box for FileName
            FileName = FileLocationTextBox.Text;
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

                StreamOutputWindow sOW = new StreamOutputWindow();

                
                Bitmap HeaderImage = Header.Encode(headerEncoder);

                
                FileBytes = File.ReadAllBytes(FileName);

                OutputImage = Encoder.Encode(FileBytes);
                /*SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true) 
                    {
                        {
                            OutputImage.Save(saveFileDialog.FileName);
                        }
                    }*/
            } catch (Exception exception) { Debug.WriteLine(exception.Message); }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Let the user select a file using an OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FileLocationTextBox.Text = openFileDialog.FileName;
            }
        }

        private void ParityCheckBox_ValueChange(object sender, RoutedEventArgs e)
        {
            ParityEnabled = (bool)ParityCheckBox.IsChecked;
        }

        private void IncrementBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockCount++;
            BlockCountTextBox.Text = BlockCount.ToString();
        }

        private void IncrementRowButton_Click(object sender, RoutedEventArgs e)
        {
            RowCount++;
            RowCountTextBox.Text = RowCount.ToString();
        }

        private void DecrementBlockButton_Click(object sender, RoutedEventArgs e)
        {
            BlockCount--;
            BlockCountTextBox.Text = BlockCount.ToString();
        }

        private void DecrementRowButton_Click(object sender, RoutedEventArgs e)
        {
            RowCount--;
            RowCountTextBox.Text = RowCount.ToString();
        }
    }
}

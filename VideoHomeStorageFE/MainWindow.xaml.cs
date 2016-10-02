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
using System.Threading;

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int RowCount = 20;
        private int BlockCount = 4;
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
            SaveButton.Click += SaveButton_Click;
            DecodeButton.Click += DecodeButton_Click;
            StreamButton.Click += StreamButton_Click;
        }

        private void StreamButton_Click(object sender, RoutedEventArgs e)
        {
            StreamInputWindow sIW = new StreamInputWindow();
            sIW.Show();
        }

        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            FileName = FileLocationTextBox.Text;
            RowCount = Int16.Parse(RowCountTextBox.Text);
            BlockCount = Int16.Parse(BlockCountTextBox.Text);
            VHSEncoder Encoder = new VHSEncoder(BlockCount, RowCount, VHSEncoder.BitDepth.byt, ParityEnabled);
            Bitmap InputImage = new Bitmap(FileName);
            int error;
            byte[] OutputBytes = Encoder.Decode(InputImage, Int16.Parse(BytesTextBox.Text), out error);
            Debug.WriteLine(error);
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllBytes(dlg.FileName, OutputBytes);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileName = FileLocationTextBox.Text;
            RowCount = Int16.Parse(RowCountTextBox.Text);
            BlockCount = Int16.Parse(BlockCountTextBox.Text);
            VHSEncoder Encoder = new VHSEncoder(BlockCount, RowCount, VHSEncoder.BitDepth.byt, ParityEnabled);
            FileBytes = File.ReadAllBytes(FileName);
            Bitmap OutputImage = await Encoder.Encode(FileBytes);
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == true)
            {
                OutputImage.Save(dlg.FileName);
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Actually set the variable equal to the input box for FileName
            FileName = FileLocationTextBox.Text;
            RowCount = Int16.Parse(RowCountTextBox.Text);
            BlockCount = Int16.Parse(BlockCountTextBox.Text);
            FileBytes = File.ReadAllBytes(FileName);
            StreamOutputWindow sOW = new StreamOutputWindow(RowCount, BlockCount, ParityEnabled, FileBytes);
            sOW.Show();
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

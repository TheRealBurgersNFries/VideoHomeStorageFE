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

namespace VideoHomeStorage.FE
{
    /// <summary>
    /// Interaction logic for StreamInputWindow.xaml
    /// </summary>
    public partial class StreamInputWindow : Window
    {
        public StreamInputWindow()
        {
            InitializeComponent();
            VideoSourcePlayer StreamPlayer = new VideoSourcePlayer();
            FilterInfoCollection Cards = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            DeviceComboBox.DataContext = Cards;
        }
    }
}

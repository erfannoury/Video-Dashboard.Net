using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFMediaKit.DirectShow.Controls;
namespace SampleApplication
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class myWindow : Window
    {
        StackPanel spanel;
        MediaUriElement player;
        public myWindow()
        {
            InitializeComponent();
            spanel = new StackPanel();
            spanel.Orientation = Orientation.Vertical;
            spanel.Children.Insert(1,lbl);
            player = new MediaUriElement();
            slider.Minimum = 0;
            slider.SmallChange = 0.01;
            slider.LargeChange = 1;
            //slider_value-media_position binding
            Binding positionBinding = new Binding("MediaPosition");
            positionBinding.Source = player;
            slider.SetBinding(Slider.ValueProperty, positionBinding);
            //slider_maxmimum-media_duration binding
            Binding maxBinding = new Binding("MediaDuration");
            maxBinding.Source = player;
            slider.SetBinding(Slider.MaximumProperty, maxBinding);
            //lbl_content-media_position binding
            Binding lblBinding = new Binding("MediaPosition");
            lblBinding.Source = player;
            lbl.SetBinding(Label.ContentProperty, lblBinding);
            //lbl_tooltip-media_position binding
            Binding sliderTooltip = new Binding("MediaPosition");
            sliderTooltip.Source = player;
            slider.SetBinding(Slider.ToolTipProperty, sliderTooltip);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            player.BeginInit();
            player.Source = new Uri(@"C:\Users\Erfan\Downloads\Video\Demi Lovato - Heart Attack (Official Video) - YouTube.mp4");
            panel.Children.Add(player);
            player.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual;
            player.EndInit();
            player.Volume = 0;
            player.MediaOpened += player_MediaOpened;
        }

        void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            player.Play();
            //MessageBox.Show(""+player.MediaDuration);
        }
    }
}

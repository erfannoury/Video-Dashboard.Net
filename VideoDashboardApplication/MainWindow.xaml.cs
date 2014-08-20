using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;
namespace testWPFMediaKit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaUriElement player;
        public MainWindow()
        {
            InitializeComponent();
            player = new MediaUriElement { Width = this.Width, Height = this.Height };
            panel.Width = this.Width;
            panel.Height = this.Height;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("this.Width = " + this.Width + ", this.Height = " + this.Height);
            Debug.WriteLine("player.Width = " + player.Width + ", player.Height = " + player.Height);
            player.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual;
            //player.VideoRenderer = WPFMediaKit.DirectShow.MediaPlayers.VideoRendererType.EnhancedVideoRenderer;
            Debug.WriteLine(player.VideoRenderer + " " + player.AudioRenderer);
            player.BeginInit();
            player.Source = new Uri(@"C:\Users\Erfan\Downloads\Video\Demi Lovato - Heart Attack (Official Video) - YouTube.mp4", UriKind.Absolute);
            Debug.WriteLine(player.HasVideo ? "Has video" : "Has not video");
            //player.Source = new Uri(@"G:\Sonny With a Chance Complete Season 1\Sonny With A Chance E01.XviD-ETACH.avi", UriKind.Absolute);

            //panel.Children.Insert(0, player);
            panel.Children.Insert(panel.Children.Count, player);
            player.EndInit();
            player.MediaOpened += player_MediaOpened;
            //Debug.WriteLine(canvas.IsVisible);
        }

        void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            Debug.WriteLine(player.Source.OriginalString);
            Debug.WriteLine(player.IsInitialized ? "Element is initialized" : "Element is not initialized");
            Debug.WriteLine(player.IsLoaded ? "Element is loaded" : "Element is not loaded");
            player.Play();
            Debug.WriteLine(player.IsPlaying ? "Media is playing" : "Media is not playing");
            Debug.WriteLine(player.IsVisible ? "Media element is visible" : "Media element is not visible");
            Debug.WriteLine(player.IsRenderingEnabled ? "Rendering is enabled" : "Rendering is not enabled");
            Debug.WriteLine(panel.IsVisible ? "panel is visible" : "panel is not visible");
        }
    }
}

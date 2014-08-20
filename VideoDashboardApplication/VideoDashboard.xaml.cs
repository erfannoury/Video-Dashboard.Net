using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using WPFMediaKit.DirectShow.Controls;
using Xceed.Wpf.Toolkit;

namespace SampleApplication
{
    /// <summary>
    /// This is a converter class between the MediaTime media position format of the MediaUriElement and Frame or Second media position format
    /// </summary>
    public class MediaTimeFrameConverter : IMultiValueConverter
    {
        /// <summary>
        /// One second in 100ns units
        /// </summary>
        protected const long DSHOW_ONE_SECOND_UNIT = 10000000;

        /// <summary>
        /// 10ms in 100ns units
        /// </summary>
        protected const long DSHOW_TEN_MILLI_SECOND_UNIT = 100000;

        /// <summary>
        /// Source -> Target
        /// Convert from MediaTime to Second
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="Parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length != 1)
                return 0;
            double seconds;
            try
            {
                seconds = ((long)values[0]) / DSHOW_TEN_MILLI_SECOND_UNIT;
                return seconds;
            }
            finally
            {
                seconds = 0;
            }
            return seconds;
        }

        /// <summary>
        /// Target -> Source
        /// Convert from Second to MediaTime
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="Parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            long mediaTime;
            object[] arr = new object[1];
            try
            {
                mediaTime = (long)((double)value * DSHOW_TEN_MILLI_SECOND_UNIT);
                arr[0] = mediaTime;
                return arr;
            }
            finally
            {
                mediaTime = 0;
            }
            arr[0] = mediaTime;
            return arr;
        }
    }

    /// <summary>
    /// This is an inherited class with an added attribute which is an integer and would be used as the index identifier
    /// </summary>
    public class CustomButton : Button
    {
        public int Index { get; set; }

        public CustomButton(int index)
        {
            this.Content = "Close";
            Index = index;
        }
    }

    /// <summary>
    /// This is the main visual element of this program. This card element includes the media player and controlling element
    /// and implements the logic behind how every media element in this program works
    /// </summary>
    public class Card
    {
        private int Height { get; set; }

        private int index { get; set; }

        private int Width { get; set; }
        /// <summary>
        /// 10ms in 100ns units
        /// </summary>
        protected const long DSHOW_TEN_MILLI_SECOND_UNIT = 100000;
        
        public GroupBox Box {  get; private set; }

        private CustomButton closeBtn { set; get; }

        private MediaUriElement player;
        private StackPanel Panel;
        private WrapPanel bottomPanel;
        private Button Open;
        private Button PlayPause;
        private DoubleUpDown mediaPosition;
        private OpenFileDialog openFile;
        private Slider slider;
        private BusyIndicator busy;

        private bool hasPlayed;
        public static string sPlay = "►";
        public static string sPause = "║";

        public Card(CustomButton close, int _h = 300, int _w = 370)
        {
            closeBtn = close;
            closeBtn.Margin = new Thickness(2);
            Height = _h;
            Width = _w;
            this.index = close.Index;
            hasPlayed = false;

            //UpDown control
            mediaPosition = new DoubleUpDown { Width = 75, Height = 30, Margin = new Thickness(2) };
            mediaPosition.Value = 0;

            //Groupbox
            Box = new GroupBox();
            Box.Width = Width;
            Box.Height = Height;
            Box.Header = new Label { Content = string.Format("Video {0}", index + 1) };
            Box.Margin = new Thickness(5, 2, 5, 2);

            //StackPanels
            Panel = new StackPanel();
            Panel.Orientation = Orientation.Vertical;

            //bottomPanel WrapPanel
            bottomPanel = new WrapPanel();
            bottomPanel.Orientation = Orientation.Horizontal;
            bottomPanel.Height = 50;

            //MediaUriElement
            player = new MediaUriElement();
            player.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual;
            player.PreferedPositionFormat = WPFMediaKit.DirectShow.MediaPlayers.MediaPositionFormat.MediaTime;
            player.Height = 225;
            player.Margin = new Thickness(1);

            //BusyIndicator
            busy = new BusyIndicator();

            //Buttons
            Open = new Button { Content = "Open Video" };
            PlayPause = new Button { Content = sPlay, Height = 30, Width = 30, Margin = new Thickness(2) };

            //Slider
            slider = new Slider { Width = 200, Minimum = 0, SmallChange = 1, LargeChange = 10, Value = 0, Margin = new Thickness(2) };

            PlayPause.IsEnabled = false;
            slider.IsEnabled = false;
            mediaPosition.IsEnabled = false;

            SetLayout();
            Open.Click += Open_Click;
            PlayPause.Click += PlayPause_Click;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetBindings();
            slider.ValueChanged -= slider_ValueChanged;
        }

        public MediaUriElement getPlayer()
        {
            return this.player;
        }

        public void setSpeedRatio(double val)
        {
            player.SpeedRatio = val;
        }

        public void advanceMediaPos(long ms)
        {
            player.MediaPosition += ms * DSHOW_TEN_MILLI_SECOND_UNIT / 10;
        }

        /// <summary>
        /// This event should be called when the video has ended, but it looks like that this event isn't called when video ends playing
        /// This is still buggy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            //!! This event is never raised !!

            System.Windows.MessageBox.Show("Media Ended");
            PlayPause.Content = sPlay;
            BindingOperations.ClearBinding(slider, Slider.MaximumProperty);
            BindingOperations.ClearBinding(slider, Slider.ValueProperty);
            BindingOperations.ClearBinding(slider, Slider.ToolTipProperty);
            BindingOperations.ClearBinding(mediaPosition, DoubleUpDown.ValueProperty);
            //player.MediaPosition = player.MediaDuration - 100;
            Uri source = player.Source;
            player.Stop();
            player.Close();
            player.Source = source;
            //SetBindings();
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (!hasPlayed)
            {
                player.Play();
                PlayPause.Content = sPause;
                hasPlayed = true;
            }
            else
            {
                if (player.IsPlaying)
                {
                    player.Pause();
                    PlayPause.Content = sPlay;
                }
                else
                {
                    player.Play();
                    PlayPause.Content = sPause;
                }
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == true)
            {
                try
                {
                    Panel.Children.Remove(Open);
                    Panel.Children.Insert(0, busy);
                    busy.IsBusy = true;
                    player.BeginInit();
                    player.Source = new Uri(openFile.FileName, UriKind.Absolute);
                    player.EndInit();
                    player.MediaOpened += player_MediaOpened;
                }
                catch (Exception exp)
                {
                    System.Windows.MessageBox.Show(exp.Message);
                    throw;
                }
            }
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            Panel.Children.Remove(busy);
            Panel.Children.Insert(0, player);
            PlayPause.IsEnabled = true;
            slider.IsEnabled = true;
            mediaPosition.IsEnabled = true;
            //System.Windows.MessageBox.Show("MediaPositionFormat = " + player.CurrentPositionFormat);
            //player.PreferedPositionFormat = WPFMediaKit.DirectShow.MediaPlayers.MediaPositionFormat.Frame;
            //System.Windows.MessageBox.Show("MediaPositionFormat2 = " + player.CurrentPositionFormat);
            SetBindings();
        }

        private void SetBindings()
        {
            // !HACK
            /*
             * This is hack to differentiate the binding when the format is frame based and there is no inter frame compression.
             * For now, only *.avi files are known to have separate frames and thus only have simple bindings
             * When individual frames are not accessible, multibinding is applied and MediaTime is expressed in milliseconds
             */
            // !HACK
            //int index = player.Source.OriginalString.Length;
            //while (char.IsLetterOrDigit(player.Source.OriginalString[--index])) ;
            //var frameBased = player.Source.OriginalString.Substring(index + 1).Trim().ToLower().Equals("avi");

            //if (frameBased)
            //{
            //    System.Windows.MessageBox.Show("Traditional Binding");
            //    Binding binding_mediaDuration = new Binding("MediaDuration");
            //    Binding binding_mediaPosition = new Binding("MediaPosition");
            //    //Slider Bindings
            //    binding_mediaDuration.Source = player;
            //    binding_mediaPosition.Source = player;
            //    slider.SetBinding(Slider.MaximumProperty, binding_mediaDuration);
            //    slider.SetBinding(Slider.ValueProperty, binding_mediaPosition);
            //    slider.SetBinding(Slider.ToolTipProperty, binding_mediaPosition);
            //}
            //else
            //{
            //MediaPosition MultiBinding
            MultiBinding multiBindingMediaPosition = new MultiBinding();
            multiBindingMediaPosition.Converter = new MediaTimeFrameConverter();
            Binding binding_mediaPosition = new Binding("MediaPosition");
            binding_mediaPosition.Source = player;
            multiBindingMediaPosition.Bindings.Add(binding_mediaPosition);
            slider.SetBinding(Slider.ValueProperty, multiBindingMediaPosition);
            slider.SetBinding(Slider.ToolTipProperty, multiBindingMediaPosition);

            //MediaDuration MultiBinding
            MultiBinding multiBindingMediaDuration = new MultiBinding();
            multiBindingMediaDuration.Converter = new MediaTimeFrameConverter();
            Binding binding_mediaDuration = new Binding("MediaDuration");
            binding_mediaDuration.Source = player;
            multiBindingMediaDuration.Bindings.Add(binding_mediaDuration);
            slider.SetBinding(Slider.MaximumProperty, multiBindingMediaDuration);
            //}

            //Label Binding
            Binding binding_sliderValue = new Binding("Value");
            binding_sliderValue.Source = slider;
            mediaPosition.SetBinding(DoubleUpDown.ValueProperty, binding_sliderValue);
        }

        private void SetLayout()
        {
            Box.Content = Panel;

            Panel.Children.Add(Open);
            Panel.Children.Add(bottomPanel);

            bottomPanel.Children.Add(PlayPause);
            bottomPanel.Children.Add(mediaPosition);
            bottomPanel.Children.Add(slider);
            bottomPanel.Children.Add(closeBtn);
        }

        public void UpdateIndex()
        {
            this.index--;
            Box.Header = new Label { Content = string.Format("Video {0}", index + 1) };
        }

        public void Play()
        {
            if (!hasPlayed)
            {
                player.Play();
                PlayPause.Content = sPause;
                hasPlayed = true;
            }
            else
            {
                player.Play();
                PlayPause.Content = sPause;
            }
        }

        public void Pause()
        {
            if (hasPlayed)
            {
                player.Pause();
                PlayPause.Content = sPlay;
            }
        }
    }

    /// <summary>
    /// Interaction logic for VideoDashboard.xaml
    /// </summary>
    public partial class VideoDashboard : Window
    {
        private WrapPanel mainWPanel;
        private StackPanel rightSPanel;
        private GroupBox bottom;
        private List<Card> cards;
        private List<MediaUriElement> players;
        private List<CustomButton> closeButtons;
        private Button addCard;
        private Button playAll;
        private Button plusTenMS;
        private Button minusTenMS;
        private int cardCount;
        private Slider speedRatioSlider;

        public VideoDashboard()
        {
            cardCount = 0;
            InitializeComponent();
            mainWPanel = new WrapPanel();
            mainWPanel.Width = 1050;
            rightSPanel = new StackPanel();
            rightSPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            rightSPanel.Orientation = Orientation.Vertical;
            rightSPanel.Width = 180;
            bottom = new GroupBox();
            bottom.Header = "Controls";
            bottom.Content = rightSPanel;
            cards = new List<Card>();
            addCard = new Button { Content = "Add Card", Margin = new Thickness(3) };
            playAll = new Button { Content = "Play All", Margin = new Thickness(3) };
            plusTenMS = new Button { Content = "+10 milliseconds", Margin = new Thickness(10,3,10,3) };
            minusTenMS = new Button { Content = "-10 milliseconds", Margin = new Thickness(10,3,10,3) };
            speedRatioSlider = new Slider { Orientation = Orientation.Vertical, Height = 75, Margin = new Thickness(72, 5, 72, 5), Value = 0 };
            speedRatioSlider.Maximum = 1.2;
            Binding binding_sRatioTooltip = new Binding("Value");
            binding_sRatioTooltip.Source = speedRatioSlider;
            speedRatioSlider.SetBinding(Slider.ToolTipProperty, binding_sRatioTooltip);

            mainStack.CanVerticallyScroll = true;
            mainStack.Children.Add(mainWPanel);
            mainStack.Children.Add(bottom);
            
            addCard.Click += addCard_Click;
            playAll.Click += playAll_Click;
            plusTenMS.Click += (s, e) =>
                {
                    foreach (var p in players)
                    {
                        p.Pause();
                    }
                    playAll.Content = "Play All";
                    foreach (var c in cards)
                    {
                        c.advanceMediaPos(10);
                    }
                };

            minusTenMS.Click += (s, e) =>
                {
                    foreach (var p in players)
                    {
                        p.Pause();
                    }
                    playAll.Content = "Play All";
                    foreach (var c in cards)
                    {
                        c.advanceMediaPos(-10);
                    }
                };


            rightSPanel.Children.Add(addCard);
            rightSPanel.Children.Add(playAll);
            rightSPanel.Children.Add(new GroupBox { Header = "Speed Ratio", Content = speedRatioSlider });
            rightSPanel.Children.Add(plusTenMS);
            rightSPanel.Children.Add(minusTenMS);

            players = new List<MediaUriElement>();
            closeButtons = new List<CustomButton>();
        }

        private void playAll_Click(object sender, RoutedEventArgs e)
        {
            if (cardCount == 0)
                return;
            if (((Button)sender).Content.Equals("Play All"))
            {
                foreach (var c in cards)
                {
                    c.Play();
                }
                ((Button)sender).Content = "Pause All";
            }
            else
            {
                foreach (var c in cards)
                {
                    c.Pause();
                }
                ((Button)sender).Content = "Play All";
            }
        }

        private void addCard_Click(object sender, RoutedEventArgs e)
        {
            closeButtons.Add(new CustomButton(cardCount));
            closeButtons[cardCount].Click += VideoDashboard_CloseButtonClick;
            cards.Add(new Card(closeButtons[cardCount]));
            mainWPanel.Children.Add(cards[cardCount].Box);
            if (cardCount == 0)
            {
                Binding binding_speedRatio = new Binding("SpeedRatio");
                binding_speedRatio.Source = cards[cardCount].getPlayer();
                speedRatioSlider.SetBinding(Slider.ValueProperty, binding_speedRatio);
            }
            else
            {
                cards[cardCount].setSpeedRatio(speedRatioSlider.Value);
                Binding binding_speedRatio = new Binding("SpeedRatio");
                binding_speedRatio.Source = cards[0].getPlayer();
                cards[cardCount].getPlayer().SetBinding(MediaUriElement.SpeedRatioProperty, binding_speedRatio);
            }

            players.Insert(cardCount, cards[cardCount].getPlayer());
            players[cardCount].MediaEnded += VideoDashboard_MediaEnded;
            cardCount++;
        }

        private void VideoDashboard_CloseButtonClick(object sender, RoutedEventArgs e)
        {
            int index = ((CustomButton)sender).Index;
            mainWPanel.Children.Remove(cards[index].Box);
            cards.RemoveAt(index);
            BindingOperations.ClearAllBindings(players[index]);
            players.RemoveAt(index);
            BindingOperations.ClearAllBindings(closeButtons[index]);
            closeButtons.RemoveAt(index);
            for (int i = index; i < cards.Count; i++)
            {
                cards[i].UpdateIndex();
                closeButtons[i].Index = i;
            }
            cardCount--;
        }

        private void VideoDashboard_MediaEnded(object sender, RoutedEventArgs e)
        {
            foreach (var p in players)
            {
                p.Pause();
            }
            playAll.Content = "Play All";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWPanel.Width = this.ActualWidth - 200;
        }
    }
}
using DiscordRPC;
using MediaManager.Dialogs;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for MediaPanel.xaml
    /// </summary>

    public partial class MediaPanel : System.Windows.Controls.UserControl
    {
        public AudioHandler audioHandler;
        long currentTime;

        bool UILoaded = false;
        bool TimeSliderDragging = false;

        public MediaPanel()
        {
            InitializeComponent();

            //MainWin = ((MainWindow)System.Windows.Application.Current.MainWindow);
            //audioHandler = MainWin.audioHandler;

            CompositionTarget.Rendering += updateSliderInformation;

            //setupUIControls();
        }
        public void loadAudioHandler()
        {
            if (audioHandler == null)
            {
            }
        }
        public void updateSliderInformation(object? sender, EventArgs? e)
        {
            // Ensures it doesnt crash the wpf designer lol
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // Ensures it only runs once per second
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() != currentTime)
                {
                    if (audioHandler.audioFile == null) { return; }
                    float elapsed = audioHandler.TimeInSeconds();
                    TimeSpan totalTime = TimeSpan.FromSeconds(audioHandler.audioFile.Length / audioHandler.BytesPerSecond);

                    audioHandler.mainWindow.discordrpc.Timestamps = new Timestamps()
                    {
                        Start = DateTime.Now.AddSeconds(elapsed),
                        End = DateTime.Now.AddSeconds(totalTime.TotalSeconds)
                        /*
                        End = DateTime.Now.AddSeconds(totalTime.TotalSeconds),*/
                        /*Start = (ulong)(DateTimeOffset.Now.ToUnixTimeSeconds() - audioHandler.TimeInSeconds()),*/
                        /*End = (ulong)(DateTimeOffset.Now.ToUnixTimeSeconds() + ((audioHandler.audioFile.Length / audioHandler.BytesPerSecond) - audioHandler.TimeInSeconds())),*/

                        /*End=DateTime.Now + ((audioHandler.audioFile.Length / audioHandler.BytesPerSecond) - TimeSpan.FromSeconds(audioHandler.TimeInSeconds())),*/
                    };


                    audioHandler.mainWindow.client.SetPresence(audioHandler.mainWindow.discordrpc);
                }
                currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            if (audioHandler != null)
            {
                if (audioHandler.Playing)
                {
                    float elapsed = audioHandler.TimeInSeconds();
                    TimeSpan inTime = TimeSpan.FromSeconds(elapsed);
                    leftTimeText.Text = inTime.ToString("m':'ss");

                    timeSlider.Value = audioHandler.audioFile.Position;
                    timeSlider.Maximum = audioHandler.audioFile.Length;

                    if (audioHandler.audioFile.Position >= audioHandler.audioFile.Length)
                    {
                        audioHandler.playAutoNext();
                    }

                    TimeSpan totalTime = TimeSpan.FromSeconds(audioHandler.audioFile.Length / audioHandler.BytesPerSecond);
                    rightTimeText.Text = totalTime.ToString("m':'ss");
                }
            }
        }
        public void updateMetadata(JToken songItem) {
            // Album Art
            JToken albumToken = audioHandler.audioLibrary["albums"][(int)songItem["album"]];
            string ? albumArtSrc = (string?)albumToken["cover"];
            Uri albumArtUri;
            if (albumArtSrc == null)
            {
                albumArtUri = new Uri("pack://application:,,,/../Lib/albumicon.png");
            } else
            {
                if (albumArtSrc[1] != ':')
                {
                    albumArtUri = new Uri(albumArtSrc, UriKind.Relative);
                }
                else
                {
                    albumArtUri = new Uri(albumArtSrc);
                }
            }
            albumArt.ImageSource = new BitmapImage(albumArtUri);

            // Other Metadata
            songTitle.Text = songItem["title"].ToString();
            artistTitle.Text = songItem["artist"].ToString();
            albumTitle.Text = albumToken["name"].ToString();

            MainWindow main = System.Windows.Application.Current.MainWindow as MainWindow;
            string details = $"{artistTitle.Text} - {songTitle.Text}";
            if (details.Length > 127) { details = $"{details.Remove(125)}..."; }
            main.discordrpc.Details = details;
            main.discordrpc.State = $"{albumTitle.Text}";
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.ToggleShuffle();/*
            if (audioHandler.isShuffled)
            {
                shuffleButton.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255));
            } else
            {
                shuffleButton.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            }*/
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            //audioHandler = MainWin.audioHandler;

            BitmapImage icon = new BitmapImage();
            icon.BeginInit();
            if (audioHandler.Playing)
            {
                icon.UriSource = new Uri("pack://application:,,,/../Lib/play.png");
                audioHandler.Pause();
            }
            else
            {
                icon.UriSource = new Uri("pack://application:,,,/../Lib/pause.png");
                int result = audioHandler.PlayAudio();
                if (result != 0) { return; }
            }
            playButtonImage.Source = icon;
            icon.EndInit();
        }

        private void timeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //MessageBox.Show(e.NewValue.ToString());
            if (TimeSliderDragging) {
                audioHandler.audioFile.Position = (long)e.NewValue;
            }
        }

        private void timeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {TimeSliderDragging = false;}
        private void timeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {TimeSliderDragging = true;}

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.playNext();
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.playPrev();
        }

        private void queueButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.mainWindow.ToggleQueue();
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.mainWindow.ToggleVolume();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void loopButton_Click(object sender, RoutedEventArgs e)
        {
            audioHandler.ToggleLoop();
        }
    }
}

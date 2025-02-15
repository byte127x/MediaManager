using System.Windows;
using System.Media;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.InteropServices;
using MicaWPF.Controls;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using MediaManager.Widgets;
using System.Windows.Media.Imaging;
using MediaManager.Widgets.MainPages;
using Microsoft.Win32;
using MediaManager.Dialogs;
using System.Windows.Threading;
using MicaWPF.Core.Extensions;
using MicaWPF.Helpers;
using System.Windows.Interop;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Forms.VisualStyles;
using System.Windows.Shell;
using Dark.Net;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using TagLib.Mpeg;
using Sungaila.ImmersiveDarkMode.Wpf;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Scrobblers;
using IF.Lastfm.Core.Objects;
using System.Drawing;
using System.Windows.Shapes;
using DiscordRPC.Logging;
using DiscordRPC;
using NAudio.Midi;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using NAudio.Gui;
using NVorbis;
using NVorbis.Contracts;
using NAudio.CoreAudioApi;

namespace MediaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class AudioHandler
    {
        // Shuffling
        private Random rng = new Random();
        public void ShuffleList(List<int> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Media GUI Features
        public List<int> Queue;
        public List<int>? OriginalQueue;
        public int Index;
        public int OriginalIndex;
        public bool Playing;
        public MediaPanel mediaPanel;
        public MainWindow mainWindow;
        public int loopType;
        public IEnumerable<string> artistList;
        public bool isShuffled = false;

        // Audio Handling Features
        public WaveOutEvent outputDevice;
        public dynamic audioFile; // Either AudioFileReader or VorbisWaveReader (because NAudio by default does not support ogg/vorbis)
        public long BytesPerSecond;
        public JObject audioLibrary;

        public AudioHandler()
        {
            Queue = new List<int>();
            Index = 0;
            Playing = false;
            string audioLibrarySrc = System.IO.File.ReadAllText(@"media\library.json");
            audioLibrary = JObject.Parse(audioLibrarySrc);
            artistList = ((JObject)audioLibrary["artists"]).Properties().Select(p => p.Name);
            artistList = artistList.OrderBy(s => s);

            // LoopTypes (0: No Loop, 1: Loop Playlist, 2: Loop Track)
            loopType = 0;

            outputDevice = new WaveOutEvent();
        }
        public void ToggleShuffle()
        {
            if (isShuffled)
            {
                //if (OriginalQueue == null) { return; isShuffled = false; }
                Queue = OriginalQueue;
                Index = OriginalIndex;
                OriginalQueue = null;
                OriginalIndex = 0;
            } else
            {
                ShuffleQueue();
            }
            isShuffled = !isShuffled;
            UpdateQueueGui();
        }
        public void ToggleLoop()
        {
            if (loopType == 0) {
                loopType = 1;
            } else
            {
                loopType = 0;
            }
        }
        public void ShuffleQueue()
        {
            OriginalQueue = Queue;
            OriginalIndex = Index;
            List<int> beginning = Queue.Take(Index + 1).ToList(); // +1 preserves the position of the current song playing
            List<int> toshuffle = Queue.Skip(Index + 1).ToList();

            ShuffleList(toshuffle);
            Queue = beginning.Concat(toshuffle).ToList();
        }
        public void ShuffleFullQueue()
        {
            OriginalQueue = Queue.ToList();
            OriginalIndex = Index;
            int selectedsong = Queue[Index];

            List<int> toshuffle = Queue;
            toshuffle.Remove(selectedsong);
            ShuffleList(toshuffle);

            Index = 0;
            Queue = new List<int> { selectedsong };
            Queue = Queue.Concat(toshuffle).ToList();
        }
        public float TimeInSeconds()
        {
            if (audioFile == null) { return -1; }
            return (float)(audioFile.Position / BytesPerSecond);
        }
        public int PlayAudio()
        {
            if (audioFile == null)
            {
                try
                {
                    var currentMedia = audioLibrary["songs"][Queue[Index]];
                    try
                    {
                        audioFile = new AudioFileReader(currentMedia["path"].ToString());
                    }
                    catch (COMException)
                    {
                        audioFile = new NAudio.Vorbis.VorbisWaveReader(currentMedia["path"].ToString());
                    }

                    outputDevice.Init(audioFile);

                    mediaPanel.updateMetadata(currentMedia);

                    // Figure out bytes per second for seeking purposes
                    var lengthInBytes = audioFile.Length;
                    long totalSeconds = (long)audioFile.TotalTime.TotalSeconds;
                    if (totalSeconds < 0.1) {
                        totalSeconds = 1;
                    }

                    BytesPerSecond = (long)lengthInBytes / totalSeconds;
                } catch (System.ArgumentOutOfRangeException)
                {
                    return 1;
                }
            }
            outputDevice.Volume = (float)mainWindow.VolumeSlider.Value;
            outputDevice.Play();
            Playing = true;
            
            BitmapImage playicon = new BitmapImage();
            playicon.BeginInit();
            playicon.UriSource = new Uri("pack://application:,,,/Lib/pause.png");
            mediaPanel.playButtonImage.Source = playicon;
            playicon.EndInit();

            UpdateQueueGui();
            return 0;
        }
        public void Stop()
        {
            Playing = false;
            outputDevice.Stop();
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }
        }
        public void Pause()
        {
            outputDevice.Pause();
            Playing = false;
        }
        public void playPrev()
        {
            if (audioFile == null)
            {
                int result = PlayAudio();
                if (result != 0)
                {
                    return;
                }
            }
            Playing = false;
            Index--;
            outputDevice.Stop();
            if (Index < 0)
            {
                Index++;
                audioFile.Position = 0;
                Playing = true;

                // Set UI to a paused state
                mediaPanel.updateSliderInformation(this, new EventArgs());
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.UriSource = new Uri("pack://application:,,,/Lib/play.png");
                mediaPanel.playButtonImage.Source = icon;
                icon.EndInit();

                Playing = false;
                return;
            }

            audioFile.Dispose();
            audioFile = null;
            PlayAudio();
            UpdateSongViewerGui();
        }
        public void playNext()
        {
            if (audioFile == null) {
                int result = PlayAudio();
                if (result != 0) {
                    return;
                }
            }
            Playing = false;
            Index++;
            outputDevice.Stop();
            if (Index >= Queue.Count)
            {
                Index--;
                audioFile.Position = 0;
                Playing = true;

                // Set UI to a paused state
                mediaPanel.updateSliderInformation(this, new EventArgs());
                BitmapImage pauseicon = new BitmapImage();
                pauseicon.BeginInit();
                pauseicon.UriSource = new Uri("pack://application:,,,/Lib/play.png");
                mediaPanel.playButtonImage.Source = pauseicon;
                pauseicon.EndInit();

                Playing = false;
                return;
            }

            audioFile.Dispose();
            audioFile = null;
            PlayAudio();
            UpdateSongViewerGui();
        }
        public void playAutoNext()
        {
            if (loopType == 0) { playNext(); }
            else {
                if (Index >= Queue.Count - 1) {
                    Index = -1;
                }
                playNext();
            }
        }
        public void ForceUpdateQueueGui()
        {
            // Trunctate queue viewer if list is too large
            int currentSongIndex;
            if (Queue.Count > 50)
            {
                mainWindow.QueueListView.Tag = Queue.Skip(Index).Take(50).ToList();
                currentSongIndex = 0;
                if (((List<int>)mainWindow.QueueListView.Tag).Count < 50)
                {
                    mainWindow.QueueListView.Tag = Queue.Skip(Math.Max(0, Queue.Count - 50)).ToList();
                    currentSongIndex = 50 - (Queue.Count - Index);
                }
            }
            else
            {
                mainWindow.QueueListView.Tag = Queue;
                currentSongIndex = Index;
            }

            mainWindow.QueueListView.Items.Clear();

            foreach (int songid in (List<int>)mainWindow.QueueListView.Tag)
            {
                JToken song = audioLibrary["songs"][songid];
                string? albumCoverPath = (string)(audioLibrary["albums"][(Int32)song["album"]]["cover"]);

                // Create queue item
                ListViewItem item = new ListViewItem();
                Grid iteminternals = new Grid();
                iteminternals.Margin = new Thickness(0, 1, 0, 1);

                GridLength[] rowdefs = new GridLength[] { new GridLength(25), new GridLength(25) };
                GridLength[] coldefs = new GridLength[] { new GridLength(50), new GridLength(3), new GridLength(1, GridUnitType.Star) };
                foreach (GridLength g in rowdefs) { RowDefinition r = new RowDefinition(); r.Height = g; iteminternals.RowDefinitions.Add(r); }
                foreach (GridLength g in coldefs) { ColumnDefinition c = new ColumnDefinition(); c.Width = g; iteminternals.ColumnDefinitions.Add(c); }

                System.Windows.Shapes.Rectangle imagecontainer = new System.Windows.Shapes.Rectangle();
                imagecontainer.RadiusX = 10;
                imagecontainer.RadiusY = 10;
                imagecontainer.Margin = new Thickness(0);
                imagecontainer.SetValue(Grid.RowSpanProperty, 2);
                imagecontainer.Width = 50;
                imagecontainer.Height = 50;
                iteminternals.Children.Add(imagecontainer);

                ImageBrush brush = new ImageBrush();
                BitmapImage albumart = new BitmapImage();
                albumart.BeginInit();
                if (albumCoverPath != null)
                {
                    if (albumCoverPath[1] != ':')
                    {
                        albumart.UriSource = new Uri(albumCoverPath, UriKind.Relative);
                    }
                    else
                    {
                        albumart.UriSource = new Uri(albumCoverPath);
                    }
                }
                else
                {
                    albumart.UriSource = new Uri("pack://application:,,,/../Lib/albumicon.png");
                }
                brush.ImageSource = albumart;
                albumart.EndInit();

                imagecontainer.Fill = brush;

                TextBlock songname = new TextBlock { Text = (string)song["title"], FontSize = 15, Margin = new Thickness(2), VerticalAlignment = System.Windows.VerticalAlignment.Bottom };
                songname.SetValue(Grid.ColumnProperty, 2);
                TextBlock artistname = new TextBlock { Text = (string)song["artist"], FontSize = 13, Margin = new Thickness(2), VerticalAlignment = System.Windows.VerticalAlignment.Top };
                artistname.SetValue(Grid.ColumnProperty, 2);
                artistname.SetValue(Grid.RowProperty, 1);
                iteminternals.Children.Add(songname);
                iteminternals.Children.Add(artistname);

                item.Content = iteminternals;
                mainWindow.QueueListView.Items.Add(item);
            }
            mainWindow.QueueListView.SelectedIndex = currentSongIndex;

        }
        public void UpdateQueueGui()
        {
            if (mainWindow.QueueListView.Tag != Queue)
            {
                ForceUpdateQueueGui();
            } else
            {
                mainWindow.QueueListView.SelectedIndex = Index;
            }
        }
        public void UpdateSongViewerGui(int? desiredId = null)
        {
            if (desiredId == null) {
                if (Index >= Queue.Count) { return; }
                desiredId = Queue[Index];
            }

            //if (Queue.Count < 1) { return; }
            if (mainWindow.currentTab != null)
            {
                if (mainWindow.currentTab.Tag != null)
                {
                    if (mainWindow.currentTab.Tag.GetType() == 0.GetType())
                    {
                        AlbumViewingPage page = (AlbumViewingPage)mainWindow.currentTab;
                        int row = 0;
                        foreach (JArray numberAndId in page.innerTracklist)
                        {
                            int songid = (int)numberAndId[1];
                            if (songid == desiredId)
                            {
                                page.ColumnedView.SelectRowByIndex(row);
                            }
                            row++;
                        }
                    }
                    else if (mainWindow.currentTab.Tag == "SongPage")
                    {
                        SongsPage songPage = (SongsPage)mainWindow.currentTab;
                        for (int songid = 0; songid < ((JArray)audioLibrary["songs"]).Count; songid++)
                        {
                            if (songid == desiredId)
                            {
                                songPage.columnedView.SelectRowByIndex(songid);
                            }
                        }
                    }
                }
            }
        }
        public void SeekAudio(int seconds)
        {
            var pos = BytesPerSecond * seconds;
            audioFile.Position = pos;
        }
        // Library Adding stuff
        public void importFolder(string path)
        {
            if (Directory.Exists(path))
            {
                string[] allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (string file in allFiles)
                {
                    importSong(file);
                }
                flushLibrary();
            }
        }
        public void importSong(string path) { 
            // Get song metadata
            var tags = TagLib.File.Create(path);

            string title = tags.Tag.Title;
            string albumname = tags.Tag.Album;
            string songartist = tags.Tag.FirstPerformer;
            string albumartist = tags.Tag.FirstAlbumArtist;
            uint tracknum = tags.Tag.Track;
            string lyrics = tags.Tag.Lyrics;
            string genre = tags.Tag.FirstGenre;
            uint year = tags.Tag.Year;

            if (title == null)
            {
                title = System.IO.Path.GetFileName(path);
            }

            if (albumartist == null)
            {
                albumartist = songartist;
            }
            else if (songartist == null) {
                songartist = albumartist;
            }
            if ((albumartist == null) && (songartist == null)) {
                songartist = albumartist = "Unknown Artist";
            }
            if ((albumname == null) || (albumname == ""))
            {
                albumname = "Unknown Album";
            }

            string coverpath;


            int song_id = ((JArray)(audioLibrary["songs"])).Count();
            JObject? album_dict = null;
            int album_id = -1;

            // Check if album already exists
            for (int idx = 0; idx < ((JArray)(audioLibrary["albums"])).Count(); idx++) {
                JToken album = audioLibrary["albums"][idx];
                if (((string)(album["name"]) == albumname) && ((string)(album["artist"]) == albumartist))
                {
                    album_dict = (JObject)(audioLibrary["albums"][idx]);
                    album_id = idx;
                }
            }

            // Adds artist if missing (will automatically add new album)
            JToken artist_dict = audioLibrary["artists"][albumartist];
            if (artist_dict == null) { 
                artist_dict = new JObject();
                artist_dict["discography"] = new JArray();
                artist_dict["bio"] = "";
            }

            // Adds album if missing
            if (album_dict == null)
            {
                album_id = ((JArray)(audioLibrary["albums"])).Count();
                album_dict = new JObject();
                album_dict["name"] = albumname;
                album_dict["artist"] = albumartist;
                album_dict["tracklist"] = new JArray();
                album_dict["genre"] = genre;
                album_dict["year"] = year;

                JArray newdiscog = (JArray)(artist_dict["discography"]);
                newdiscog.Add(album_id);
                artist_dict["discography"] = newdiscog;
                JArray newalbums = (JArray)(audioLibrary["albums"]);
                newalbums.Add(album_dict);
                audioLibrary["albums"] = newalbums;

                album_dict = (JObject)(audioLibrary["albums"][album_id]);
            }

            // Add song info to object
            JArray tlist = (JArray)(album_dict["tracklist"]);
            JArray numberPlusId = new JArray();
            numberPlusId.Add(tracknum);
            numberPlusId.Add(song_id);
            tlist.Add(numberPlusId);


            album_dict["tracklist"] = tlist;
            JObject song_dict = new JObject();

            // Store song length ahead of time
            dynamic a; // Either AudioFileReader, or VorbisReader if ogg vorbis audio is imported
            try
            {
                a = new AudioFileReader(path);
            }
            catch (COMException e)
            {
                a = new VorbisReader(path);
            }

            song_dict["duration"] = Math.Round(a.TotalTime.TotalSeconds, 2);


            song_dict["title"] = title;
            song_dict["album"] = album_id;
            song_dict["artist"] = songartist;
            song_dict["lyrics"] = lyrics;
            song_dict["path"] = path;


            if (tags.Tag.Pictures.Count() > 0)
            {
                TagLib.IPicture albumArt = tags.Tag.Pictures[0];
                if ((albumArt != null) && (album_dict["cover"] == null))
                {
                    string extension;
                    if (albumArt.MimeType == "image/jpeg") { extension = "jpg"; }
                    else if (albumArt.MimeType == "image/png") { extension = "png"; }
                    else if (albumArt.MimeType == "image/webp") { extension = "webp"; }
                    else { extension = "bmp"; }

                    string filepath = string.Format(@"{0}.", Guid.NewGuid());
                    coverpath = $"media\\{filepath}{extension}";
                    byte[] bin = (byte[])(albumArt.Data.Data);
                    MemoryStream imgOut = new MemoryStream(bin);

                    using (StreamWriter outputFile = new StreamWriter(coverpath))
                    {
                        imgOut.WriteTo(outputFile.BaseStream);
                    }
                }
                else
                {
                    coverpath = null;
                }
            } else
            {
                coverpath = null;
            }

            if (album_dict["cover"] == null)
            {
                album_dict["cover"] = coverpath;
            }

            // Add modified artist and song to audioLibrary
            audioLibrary["artists"][albumartist] = artist_dict;
            JArray newSongArr = (JArray)(audioLibrary["songs"]);
            newSongArr.Add(song_dict);
            audioLibrary["songs"] = newSongArr;
        }
        public void flushLibrary()
        {
            using (StreamWriter outputFile = new StreamWriter(@"media\library.json"))
            {
                outputFile.WriteLine(
                    audioLibrary.ToString()
                );
            }
        }

        public int PlayAllSongs(int startid)
        {
            Stop();

            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            Queue = Enumerable.Range(0, ((JArray)audioLibrary["songs"]).Count).ToList();
            Index = startid;

            if (isShuffled) { ShuffleFullQueue(); }

            PlayAudio();

            return 0;
        }
        public int PlayAlbum(int songid, int albumid)
        {
            Stop();
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            List<int> AlbumSongIds = new List<int>();
            List<int> TrackNums = new List<int>();
            foreach (JArray songIdAndNumbering in (JArray)audioLibrary["albums"][albumid]["tracklist"])
            {
                int songtracknumber = (int)songIdAndNumbering[0];
                int listsongid = (int)songIdAndNumbering[1];

                AlbumSongIds.Add(listsongid);
                TrackNums.Add(songtracknumber);
            }
            IEnumerable<int> SortedAlbumSongIds = AlbumSongIds.OrderBy(x => TrackNums[AlbumSongIds.IndexOf(x)]).ToList();
            Queue = SortedAlbumSongIds.ToList();
            Index = Queue.IndexOf(songid);

            if (isShuffled) { ShuffleFullQueue(); }

            PlayAudio();
            return 0;
        }
        public int PlayPlaylist(int songid, int albumid)
        {
            Stop();
            if (audioFile != null)
            {
                audioFile.Dispose();
                audioFile = null;
            }

            List<int> AlbumSongIds = new List<int>();
            List<int> TrackNums = new List<int>();
            foreach (UInt32 listsongid in (JArray)audioLibrary["playlists"][albumid]["tracklist"])
            {

                AlbumSongIds.Add((int)listsongid);
            }
            Queue = AlbumSongIds.ToList();
            Index = Queue.IndexOf(songid);

            if (isShuffled) { ShuffleFullQueue(); }

            PlayAudio();
            return 0;
        }
    }
    public class Settings
    {
        public string? username;
        public float volume;
        public JArray? lastqueue;
        public int lastidx;
    }

    public partial class MainWindow : Window
    {
        public AudioHandler audioHandler = new AudioHandler();

        public AlbumPage albumPage = new AlbumPage();
        public HomePage homePage = new HomePage();
        public ArtistPage artistPage = new ArtistPage();
        public SongsPage songPage = new SongsPage();
        public YearsPage yearPage = new YearsPage();
        public PlaylistPage playlistPage = new PlaylistPage();
        public SearchPage searchPage = new SearchPage();
        public FrameworkElement? currentTab;
        public bool queueShowing = true;
        public List<Window> Dialogs = new List<Window>();

        public Settings settings = new Settings();

        public DiscordRpcClient client;
        public RichPresence discordrpc;

        public List<SearchableField> allSongs = new List<SearchableField>();

        string[] audiofiletypes = new[] { ".wav", ".mp3", ".m4a", ".aif", ".aiff", ".wma", ".aac", ".ogg" };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Allow Horizontal Scrolling
            IEnumerable<ScrollViewer> scrolls = homePage.InternalGrid.FindLogicalChildren<ScrollViewer>();
            foreach (ScrollViewer s in scrolls)
            {
                s.PreviewMouseWheel += HorizontalScroll;
            }
        }
        private void HorizontalScroll(object sender, MouseWheelEventArgs e)
        {
            //MessageBox.Show(e.Delta.ToString());
            ScrollViewer s = (ScrollViewer)sender;
            s.ScrollToHorizontalOffset(e.Delta+s.HorizontalOffset);
        }
        public void CloseAllDialogs(object sender, object e)
        {
            client.ClearPresence();
            foreach (Window d in Dialogs)
            {
                d.Close();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            //WindowExtensions.InitTitlebarTheme(this);

            this.PreviewMouseDown += CloseAllDialogs;
            this.Closed += CloseAllDialogs;
            this.Closed += SaveSettingsData;
            songPage.Tag = "SongPage";
            audioHandler.mainWindow = this;
            audioHandler.mediaPanel = mediaPanelControl;
            mediaPanelControl.audioHandler = audioHandler;

            songPage.columnedView.AddColumn("Song Title");
            songPage.columnedView.AddColumn("Artist");
            songPage.columnedView.AddColumn("Album");
            songPage.columnedView.AddColumn("Genre");
            songPage.columnedView.AddColumn("Year");

            // Load Settings
            string settingsSrc = System.IO.File.ReadAllText(@"media\settings.json");
            JObject settingsJson = JObject.Parse(settingsSrc);

            settings.username = (string)(settingsJson["username"]);
            settings.volume = (float)(settingsJson["volume"]);
            settings.lastqueue = (JArray)(settingsJson["lastqueue"]);
            settings.lastidx = (int)(settingsJson["lastidx"]);

            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            client = new DiscordRpcClient("1277446264920735865");

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                //MessageBox.Show($"Received Ready from user {e.User.Username}");
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                //MessageBox.Show($"Received Update! {e.Presence}");
                if (e.Type == DiscordRPC.Message.MessageType.Error)
                {
                    MessageBox.Show(e.Name);
                }
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            discordrpc = new RichPresence()
            {
                Details = "Example Project",
                State = "csharp example",
                Assets = new Assets()
                {
                },
                Timestamps = new Timestamps()
                {
                }
            };
            client.SetPresence(discordrpc);



            //if (System.Environment.OSVersion.Version.Major <= 6)
            //{
            //    this.SystemBackdropType = MicaWPF.Core.Enums.BackdropType.None;
            //    SolidColorBrush s = new SolidColorBrush();
            //    s.Color = Color.FromArgb(0, 255, 255, 255);
            //}
            //else
            //{
            //    this.SystemBackdropType = MicaWPF.Core.Enums.BackdropType.Acrylic;
            //}
            App.AddMicaEffect(this);
            Loaded += Window_Loaded;

            homePage.AddFileButton.setIcon(27);
            homePage.AddFileButton.albumText.Text = "Add File +";

            homePage.AddFolderButton.ExtraMouseUp += AddFolder;
            homePage.AddFileButton.ExtraMouseUp += AddFile;

            songPage.columnedView.songPlaybackFunction = audioHandler.PlayAllSongs;

            VolumeSlider.Value = settings.volume;

            CreateUI();
            changeFrame("homeBtn");
            if (audioHandler.Queue.Count > 0)
            {
                mediaPanelControl.updateMetadata(audioHandler.audioLibrary["songs"][audioHandler.Queue[audioHandler.Index]]);
            }
            SystemSounds.Asterisk.Play();
        }
        public void SaveSettingsData(object sender, dynamic? _)
        {
            // Compress settings down to a JSON
            JObject settingsJson = new JObject();
            settingsJson["username"] = settings.username;
            settingsJson["volume"] = settings.volume;
            settingsJson["lastidx"] = settings.lastidx;
            settingsJson["lastqueue"] = settings.lastqueue;

            using (StreamWriter outputFile = new StreamWriter(@"media\settings.json"))
            {
                outputFile.WriteLine(
                    settingsJson.ToString()
                );
            }
        }

        public void VolumeChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            settings.volume = (float)VolumeSlider.Value;
            if (audioHandler.outputDevice != null)
            {
                audioHandler.outputDevice.Volume = settings.volume;
            }
        }

        public void SettingsDialog()
        {
            SettingsDialog dlg = new SettingsDialog();
            dlg.ApplySettings(settings);
            dlg.ShowDialog();
        }
        public void ToggleVolume()
        {
            bool volumeShowing = (VolumeSection.Height.Value != 0);

            if (volumeShowing)
            {
                VolumeSection.Height = new GridLength(0);
            } else
            {
                VolumeSection.Height = new GridLength(60);
                if (audioHandler.audioFile != null)
                {
                    VolumeSlider.Value = (double)audioHandler.outputDevice.Volume;
                }
                if (!queueShowing) {
                    ToggleQueue();
                }
            }
        }
        public void ToggleQueue()
        {
            if (queueShowing)
            {
                // Hide Queue Tab
                QueuePlace.Width = 0;
                QueueSplitterStyle.Width = 0;
                QueueGridSplitter.Width = 0;
                TabSwitcherParent.Margin = new Thickness(0, 15, 15, 0);
                TabSwitcherParent.SetValue(Grid.ColumnSpanProperty, 3);
            } else
            {
                // Show Queue Tab
                QueuePlace.Width = Double.NaN;
                QueueSplitterStyle.Width = 1;
                QueueGridSplitter.Width = 4;
                TabSwitcherParent.Margin = new Thickness(0, 15, 0, 0);
                TabSwitcherParent.SetValue(Grid.ColumnSpanProperty, 1);
            }
            queueShowing = !queueShowing;
        }

        public int AddFile(object sender)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Supported Audio Files|*.mp3;*.m4a;*.wav;*.aac;*.wma;*.aif;*.aiff;*.ogg|MPEG-3 Audio|*.mp3|AAC/ALAC Audio|*.m4a;*.aac|WAV Audio|*.wav|OGG Vorbis Audio|*.ogg|Windows Media Audio|*.wma|AIFF Audio|*.aif;*.aiff|All files (*.*)|*.*";
            dialog.Multiselect = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.ShowDialog(this);

            FileProgressbarDialog prog = new FileProgressbarDialog();
            prog.ShowInTaskbar = false;
            prog.Topmost = true;
            prog.Show();
            foreach (string file in dialog.FileNames)
            {
                string displayName = System.IO.Path.GetFileName(file);
                prog.FileNameText.Text = $"'{displayName}'";

                audioHandler.importSong(file);

                AllowUIToUpdate();
            }
            audioHandler.flushLibrary();
            prog.Close();

            ClearUI();
            CreateUI();
            return 0;
        }
        public int AddFolder(object sender)
        {
            // Get dialog to select folder
            //OpenFolderDialog dialog = new OpenFolderDialog();
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            //dialog.Multiselect = false;
            dialog.ShowDialog();
            if (dialog.SelectedPath == "") { return 1; }

            FileProgressbarDialog prog = new FileProgressbarDialog();
            prog.ShowInTaskbar = false;
            prog.Topmost = true;
            prog.Show();

            string[] filepaths = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories);
            IEnumerable<string> audioFilepaths = filepaths.Where(s => audiofiletypes.Contains(System.IO.Path.GetExtension(s).ToLower())); // Ensure only audio files are selected

            foreach (string file in audioFilepaths)
            {
                string displayName = System.IO.Path.GetFileName(file);
                prog.FileNameText.Text = $"'{displayName}'";

                audioHandler.importSong(file);

                AllowUIToUpdate();
            }
            audioHandler.flushLibrary();
            prog.Close();

            ClearUI();
            CreateUI();
            return 0;
        }
        public int AddPlaylist(object sender)
        {
            PlaylistCreator dialog = new PlaylistCreator();
            App.AddMicaEffect(dialog);
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                MessageBox.Show(dialog.PlaylistBody.InnerView.Items.Count.ToString());

                JObject playlistInfo = new JObject();
                playlistInfo["title"] = dialog.TitleInput.InternalText.Text;
                playlistInfo["description"] = dialog.DescInput.InternalText.Text;
                playlistInfo["datecreated"] = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                JArray tracklist = new JArray();

                foreach (ListViewItem songlvi in dialog.PlaylistBody.InnerView.Items)
                {
                    int songid = (int)songlvi.Tag;
                    tracklist.Add(songid);
                }
                playlistInfo["tracklist"] = tracklist;
                ((JArray)audioHandler.audioLibrary["playlists"]).Add(playlistInfo);
            }

            audioHandler.flushLibrary();
            ClearUI();
            CreateUI();
            return 0;
        }
        void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
        public void ClearUI()
        {
            albumPage.AlbumContainer.InternalGrid.Children.Clear();

            artistPage.ArtistView.InnerView.Items.Clear();
            artistPage.AlbumContainer.InternalGrid.Children.Clear();
            artistPage.AlbumContainer.RefreshAlbumCards();

            homePage.RecentlyAddedGrid.Children.Clear();
            homePage.RecentlyAddedGrid.Children.Add(homePage.AddFolderButton);
            homePage.RecentlyAddedGrid.Children.Add(homePage.AddFileButton);

            ColumnDefinition c = new ColumnDefinition();
            c.Width = new GridLength(200, GridUnitType.Pixel);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(200, GridUnitType.Pixel);
            homePage.RecentlyAddedGrid.ColumnDefinitions.Clear();
            homePage.RecentlyAddedGrid.ColumnDefinitions.Add(c);
            homePage.RecentlyAddedGrid.ColumnDefinitions.Add(c2);

            songPage.columnedView.ClearItems();

            playlistPage.AlbumContainer.InternalGrid.Children.Clear();
            playlistPage.AlbumContainer.RefreshAlbumCards();

            yearPage.YearView.InnerView.Items.Clear();
            yearPage.AlbumContainer.InternalGrid.Children.Clear();
            yearPage.AlbumContainer.RefreshAlbumCards();
        }
        public void CreateUI()
        {
            searchPage.AlbumsToSearch = new List<SearchableField>();
            searchPage.ArtistsToSearch = new List<SearchableField>();
            searchPage.SongsToSearch = new List<SearchableField>();
            searchPage.PlaylistsToSearch = new List<SearchableField>();

            // Load Artists in Artist Tab
            ListView artistView = artistPage.ArtistView.InnerView;

            int artistid = 0;
            foreach (string artist in audioHandler.artistList)
            {
                ListViewItem item = new ListViewItem();
                item.Content = artist;
                item.FontSize = 13;
                artistView.Items.Add(item);
                searchPage.ArtistsToSearch.Add(new SearchableField(artist, artistid, 1));
                artistid++;
            }
            artistPage.ArtistView.selChangedFunc = changeArtistView;

            // Load Albums in Album Tab
            List<int> years = new List<int>();
            Grid AlbumInternalGrid = albumPage.AlbumContainer.InternalGrid;
            int albumid = 0;
            JToken[] jsonAlbums = ((JArray)(audioHandler.audioLibrary["albums"])).ToArray();
            foreach (JToken album in jsonAlbums) { jsonAlbums[albumid]["id"] = albumid; albumid++; }
            JToken[] sortedAlbums = jsonAlbums.OrderBy(x => $"{x["artist"]}+{x["name"]}").ToArray();

            foreach (JToken album in sortedAlbums)
            {
                int id = (int)album["id"];
                jsonAlbums[id]["id"] = id;
                AlbumCard card = new AlbumCard();
                card.albumId = id;
                card.ExtraMouseUp = albumCardClicked;

                string? cover = (string)album["cover"];
                if ((cover != null) && (!System.IO.File.Exists(cover))) {
                    cover = null;
                    album["cover"] = null;
                }
                card.setCardInfo((string)album["name"], (string)album["artist"], cover);
                AlbumInternalGrid.Children.Add(card);

                // Add years to queue
                bool yearInYearpage = years.Contains((int)(album["year"]));
                if (!yearInYearpage)
                {
                    years.Add((int)(album["year"]));
                }

                try
                {
                    searchPage.AlbumsToSearch.Add(new SearchableField((string)album["name"], id, 0));
                } catch (System.ArgumentException e)
                {
                    continue;
                }
            }
            albumPage.AlbumContainer.RefreshAlbumCards();

            // Add last 10 albums into recently added
            IEnumerable<JToken> recentlyAdded = jsonAlbums.Skip(Math.Max(0, jsonAlbums.Length - 10));
            recentlyAdded = recentlyAdded.OrderBy(x => recentlyAdded.Count() - (int)(x["id"])); // Reverse order from oldest-to-newest to newest-to-oldest
            int idx = jsonAlbums.Length-1;
            foreach (JToken album in recentlyAdded)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width = new GridLength(200, GridUnitType.Pixel);
                homePage.RecentlyAddedGrid.ColumnDefinitions.Add(c);

                AlbumCard recentCard = new AlbumCard();
                recentCard.albumId = idx;
                recentCard.setCardInfo((string)album["name"], (string)album["artist"], (string)album["cover"]);
                recentCard.ExtraMouseUp = albumCardClicked;

                recentCard.SetValue(Grid.ColumnProperty, homePage.RecentlyAddedGrid.ColumnDefinitions.Count - 1);

                homePage.RecentlyAddedGrid.Children.Add(recentCard);
                idx--;
            }

            // Load Years in Years Tab

            ListView yearView = yearPage.YearView.InnerView;
            years.Sort();
            foreach (int year in years)
            {
                ListViewItem item = new ListViewItem();
                if (year == 0) { item.Content = "Unknown Year"; }
                else { item.Content = year; }
                item.FontSize = 13;
                yearView.Items.Add(item);
            }
            yearPage.YearView.selChangedFunc += changeYearsView;

            // Load Songs in Songs Tab
            int songid = 0;
            foreach (JToken song in audioHandler.audioLibrary["songs"])
            {
                JToken album = audioHandler.audioLibrary["albums"][(Int32)(song["album"])];

                string? year = (string)(album["year"]);
                if (year == "0")
                {
                    year = "";
                }
                songPage.columnedView.AddItem(new string[] { (string)(song["title"]), (string)(song["artist"]), (string)(album["name"]), (string)(album["genre"]), year });

                allSongs.Add(new SearchableField(song["title"].ToString(), songid, 2));

                songid++;
            }
            searchPage.SongsToSearch = allSongs;

            // Load Playlists in Playlist Tab
            playlistPage.AddPlaylistBtn.ExtraMouseUp = AddPlaylist;
            playlistPage.InternalGrid.Children.Remove(playlistPage.AddPlaylistBtn);
            if (playlistPage.AddPlaylistBtn.Parent == null)
            {
                playlistPage.AlbumContainer.InternalGrid.Children.Add(playlistPage.AddPlaylistBtn);
            }
            int playlistid = 0;
            foreach (JToken playlist in audioHandler.audioLibrary["playlists"])
            {
                AlbumCard card = new AlbumCard();
                card.albumId = playlistid;
                card.ExtraMouseUp = playlistCardClicked;

                string? cover = (string)playlist["cover"];
                if ((cover != null) && (!System.IO.File.Exists(cover)))
                {
                    cover = null;
                    playlist["cover"] = null;
                }
                card.setCardInfo((string)playlist["title"], $"{(JToken)playlist["tracklist"].Count()} Songs", cover);
                playlistPage.AlbumContainer.InternalGrid.Children.Add(card);

                searchPage.PlaylistsToSearch.Add(new SearchableField((string)playlist["title"], playlistid, 3));
                playlistid++;
            }
            playlistPage.AlbumContainer.RefreshAlbumCards();

            // Load Searchable Items in Search Tab
            //archPage.AlbumsToSearch = 
            searchPage.mainWindow = this;

            sidePanelControl.returnFunction = changeFrame;
        }
        public int changeArtistView(string artistnew)
        {
            artistPage.AlbumContainer.InternalGrid.Children.Clear();
            JArray discography = (JArray)(audioHandler.audioLibrary["artists"][artistnew]["discography"]);
            foreach (int albumid in discography)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = albumid;
                card.ExtraMouseUp = albumCardClicked;

                JToken alb = audioHandler.audioLibrary["albums"][albumid];
                card.setCardInfo((string)alb["name"], artistnew, (string?)alb["cover"]);
                artistPage.AlbumContainer.InternalGrid.Children.Add(card);
            }
            artistPage.AlbumContainer.RefreshAlbumCards();
            return 0;
        }
        public int changeYearsView(string yearnew)
        {
            if (yearnew == "Unknown Year") { yearnew = "0"; }
            yearPage.AlbumContainer.InternalGrid.Children.Clear();
            int albumid = 0;
            foreach (JToken album in audioHandler.audioLibrary["albums"])
            {
                if ((string)(album["year"]) == yearnew)
                {
                    AlbumCard card = new AlbumCard();
                    card.albumId = albumid;
                    card.ExtraMouseUp = albumCardClicked;

                    card.setCardInfo((string)album["name"], (string)album["artist"], (string?)album["cover"]);
                    yearPage.AlbumContainer.InternalGrid.Children.Add(card);
                }
                albumid++;
            }
            yearPage.AlbumContainer.RefreshAlbumCards();
            return 0;
        }
        public int changeFrame(object frame)
        {
            if (currentTab != null) {
                TabSwitcher.Children.Remove(currentTab);
            }

            if (frame.GetType() == typeof(string))
            {
                string strframe = (string)frame;
                if (strframe == "homeBtn")
                {
                    currentTab = homePage;
                }
                else if (strframe == "artistBtn")
                {
                    currentTab = artistPage;
                }
                else if (strframe == "albumBtn")
                {
                    currentTab = albumPage;
                }
                else if (strframe == "songBtn")
                {
                    currentTab = songPage;
                    audioHandler.UpdateSongViewerGui();
                }
                else if (strframe == "yearBtn")
                {
                    currentTab = yearPage;
                }
                else if (strframe == "playlistBtn")
                {
                    currentTab = playlistPage;
                }
                else if (strframe == "searchBtn")
                {
                    currentTab = searchPage;
                }
            } else
            {
                currentTab = (FrameworkElement)frame;
            }

            TabSwitcher.Children.Add(currentTab);
            return 0;
        }
        public int artistCardClicked(AlbumCard card) {
            //MessageBox.Show("lolcatz");
            changeFrame("artistBtn");

            sidePanelControl.activeButton.IsChecked = false;
            sidePanelControl.artistBtn.IsChecked = true;
            sidePanelControl.activeButton = sidePanelControl.artistBtn;
            artistPage.ArtistView.InnerView.SelectedIndex = card.albumId;

            //sidePanelControl.toggleButton_Toggle(sidePanelControl.activeButton);
            //sidePanelControl.toggleButton_Toggle(sidePanelControl.artistBtn);
            //sidePanelControl.toggleButton_ClickStart(sidePanelControl.artistBtn, null);
            //sidePanelControl.toggleButton_ClickEnd(sidePanelControl.activeButton, null);

            return 0;
        }
        public int albumCardClicked(AlbumCard card) {
            int id = card.albumId;
            JToken albumInfo = audioHandler.audioLibrary["albums"][id];

            AlbumViewingPage page = new AlbumViewingPage();
            page.audioHandler = audioHandler;
            page.backPlace = sidePanelControl.activeButton.Name;
            page.backCommand = albumBack;

            List<JArray> tracklist = new List<JArray>();
            foreach (JArray track in (JArray)albumInfo["tracklist"])
            {
                JArray addedTrack = new JArray();
                addedTrack.Add(track[0]);
                addedTrack.Add(track[1]);

                // Get Audio Length
                addedTrack.Add((float)audioHandler.audioLibrary["songs"][(Int32)(track[1])]["duration"]);

                tracklist.Add(addedTrack);
            }

            page.Tag = card.albumId;
            page.UpdateInfo((string)albumInfo["name"], (string)albumInfo["artist"], (string)albumInfo["genre"], (string)albumInfo["year"], (string)albumInfo["cover"], tracklist);
            page.ColumnedView.songPlaybackFunction = (int songid) => { return audioHandler.PlayAlbum(songid, card.albumId); };

            sidePanelControl.activeButton.IsChecked = false;
            sidePanelControl.activeButton = null;
            changeFrame(page);

            audioHandler.UpdateSongViewerGui();
            return 0;
        }
        public int songCardClicked(AlbumCard card)
        {

            int songid = card.albumId;
            int albumid = (int)(audioHandler.audioLibrary["songs"][songid]["album"]);
            JToken albumInfo = audioHandler.audioLibrary["albums"][albumid];

            AlbumViewingPage page = new AlbumViewingPage();
            page.audioHandler = audioHandler;
            page.backPlace = sidePanelControl.activeButton.Name;
            page.backCommand = albumBack;

            List<JArray> tracklist = new List<JArray>();
            foreach (JArray track in (JArray)albumInfo["tracklist"])
            {
                JArray addedTrack = new JArray();
                addedTrack.Add(track[0]);
                addedTrack.Add(track[1]);

                // Get Audio Length

                //MessageBox.Show(System.IO.File.Exists((string)audioHandler.audioLibrary["songs"][(Int32)(track[1])]["path"]).ToString());
                AudioFileReader a = new AudioFileReader((string)audioHandler.audioLibrary["songs"][(Int32)(track[1])]["path"]);
                //addedTrack.Add("7:77");
                addedTrack.Add(a.TotalTime.TotalSeconds);

                tracklist.Add(addedTrack);
            }

            page.Tag = card.albumId;
            page.UpdateInfo((string)albumInfo["name"], (string)albumInfo["artist"], (string)albumInfo["genre"], (string)albumInfo["year"], (string)albumInfo["cover"], tracklist);
            page.ColumnedView.songPlaybackFunction = (int selectedsongid) => { return audioHandler.PlayAlbum(selectedsongid, albumid); };

            sidePanelControl.activeButton.IsChecked = false;
            sidePanelControl.activeButton = null;
            changeFrame(page);

            audioHandler.UpdateSongViewerGui(songid);
            return 0;
        }
        public int playlistCardClicked(AlbumCard card)
        {
            int id = card.albumId;
            JToken playlistInfo = audioHandler.audioLibrary["playlists"][id];

            AlbumViewingPage page = new AlbumViewingPage();
            page.audioHandler = audioHandler;
            page.backPlace = sidePanelControl.activeButton.Name;
            page.backCommand = albumBack;

            List<JArray> tracklist = new List<JArray>();
            int tracknum = 0;
            foreach (UInt32 track in (JArray)playlistInfo["tracklist"])
            {
                JArray addedTrack = new JArray();
                addedTrack.Add(tracknum+1);
                addedTrack.Add(track);

                // Get Audio Length

                AudioFileReader a = new AudioFileReader((string)audioHandler.audioLibrary["songs"][(Int32)(track)]["path"]);
                //addedTrack.Add("7:77");
                addedTrack.Add(a.TotalTime.TotalSeconds);

                tracklist.Add(addedTrack);
                tracknum++;
            }

            page.Tag = card.albumId;

            int unixTimestampCreated = (int)playlistInfo["datecreated"];
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestampCreated).ToLocalTime();

            page.UpdatePlaylistInfo((string)playlistInfo["title"], dateTime.ToString("d"), (string)playlistInfo["description"], (string)playlistInfo["cover"], tracklist);
            page.ColumnedView.songPlaybackFunction = (int songid) => { return audioHandler.PlayPlaylist(songid, card.albumId); };

            sidePanelControl.activeButton.IsChecked = false;
            sidePanelControl.activeButton = null;
            changeFrame(page);

            audioHandler.UpdateSongViewerGui();
            return 0;
        }
        public int albumBack(string whereto)
        {
            ToggleButton re = sidePanelControl.InternalGrid.FindName(whereto) as ToggleButton;
            re.IsChecked = true;

            changeFrame(whereto);

            return 0;
        }

        private void QueueListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QueueListView.SelectedIndex == -1)
            {
                return;
            }
            int BeforeSelectedSongId = ((List<int>)QueueListView.Tag)[QueueListView.SelectedIndex];
            audioHandler.Index = audioHandler.Queue.IndexOf(BeforeSelectedSongId);
            audioHandler.Stop();
            audioHandler.PlayAudio();
        }
    }
}
using MediaManager.Dialogs;
using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaManager.Widgets.MainPages
{
    /// <summary>
    /// Interaction logic for AlbumViewingPage.xaml
    /// </summary>
    public partial class AlbumViewingPage : UserControl
    {
        public string backPlace;
        public Func<string, int> backCommand;
        public AudioHandler audioHandler;
        public List<JArray> innerTracklist;
        public AlbumViewingPage()
        {
            InitializeComponent();

            ColumnedView.AddColumn("#");
            ColumnedView.AddColumn("Title");
            ColumnedView.AddColumn("Length");

            int idx = 0;
            foreach (ColumnDefinition c in ColumnedView.HeaderGrid.ColumnDefinitions)
            {
                if (idx == 0)
                {
                    c.Width = new GridLength(25);
                    c.MinWidth = 25;
                }
                else if (idx == 4) {
                    c.Width = new GridLength(50);
                }
                idx++;
            }
        }
        public void UpdateInfo(string title, string artist, string genre, string year, string albumart, List<JArray> tracklist)
        {
            if (genre == null) { genre = "Unknown Genre"; }
            if (year == "0") { year = "Unknown Year"; }
            tracklist = tracklist.OrderBy(x => x[0]).ToList();
            innerTracklist = tracklist;

            AlbumText.Text = title;
            ArtistText.Text = artist;
            GenreYearText.Text = $"{genre} • {year}";
            CountText.Text = $"{tracklist.Count} Songs";

            Uri albumArtUri;
            if (albumart == null)
            {
                albumArtUri = new Uri("pack://application:,,,/../Lib/albumicon.png");
            }
            else
            {
                if (albumart[1] != ':')
                {
                    albumArtUri = new Uri(albumart, UriKind.Relative);
                }
                else
                {
                    albumArtUri = new Uri(albumart);
                }
            }
            albumImg.ImageSource = new BitmapImage(albumArtUri);

            int idx = 0;

            foreach (JArray track in tracklist)
            {
                int tracknum = (int)track[0];
                int songid = (int)track[1];
                string length = TimeSpan.FromSeconds((double)track[2]).ToString("m':'ss");
                

                string songname = (string)audioHandler.audioLibrary["songs"][songid]["title"];
                ColumnedView.AddItem(new string[] { $"{tracknum}", songname, length });
                ColumnedView.allBorders[idx].Tag = songid;
                idx++;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            backCommand(backPlace);
        }

        private void PreviewAlbumArt(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                PreviewAlbumArtWindow dlg = new PreviewAlbumArtWindow();
                dlg.Title = $"Album Art: {AlbumText.Text}";
                dlg.albumImg.ImageSource = albumImg.ImageSource;
                dlg.ShowDialog();
            }
        }
    }
}

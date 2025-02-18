using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaManager.Widgets.MainPages
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : UserControl
    {
        public List<SearchableField> AlbumsToSearch = new List<SearchableField>();
        public List<SearchableField> PlaylistsToSearch = new List<SearchableField>();
        //public Dictionary<string, int> SongsToSearch = new Dictionary<string, int>();
        public List<SearchableField> SongsToSearch;
        public List<SearchableField> ArtistsToSearch;
        public MainWindow mainWindow { get; set; }

        public SearchPage()
        {
            InitializeComponent();
            //SearchBar.InternalText.KeyDown += Search;
            SearchBar.InternalText.PreviewKeyUp += Search;
        }
        public void Search(object sender, EventArgs e)
        {
            //string searchText = SearchBar.InternalText.Text;
            //MessageBox.Show(searchText);
            ComputeSearch(SearchBar.InternalText.Text);
        }
        public int CompareFieldsByScore(SearchableField field1, SearchableField field2)
        {
            return field2.score.CompareTo(field1.score);
        }
        public void ComputeSearch(string searchText)
        {
            if (searchText.Length < 2)
            {
                return;
            }

            AlbumContainer.InternalGrid.Children.Clear();
            if (searchText == "") {
                AlbumContainer.RefreshAlbumCards();
                return;
            }
            List<SearchableField> searchedAlbums = AlbumsToSearch.FindAll((SearchableField field) => { return QualifiesForSearch(field, searchText); });
            List<SearchableField> searchedSongs = SongsToSearch.FindAll((SearchableField field) => { return QualifiesForSearch(field, searchText); });
            List<SearchableField> searchedPlaylists = PlaylistsToSearch.FindAll((SearchableField field) => { return QualifiesForSearch(field, searchText); });
            List<SearchableField> searchedArtists = ArtistsToSearch.FindAll((SearchableField field) => { return QualifiesForSearch(field, searchText); });
            
            List<SearchableField> searchResults = searchedAlbums.Concat(searchedArtists.Concat(searchedSongs.Concat(searchedPlaylists))).ToList();
            searchResults.Sort(CompareFieldsByScore);
            // Cap to only 75 results
            searchResults = searchResults.Take(75).ToList();

            foreach (SearchableField field in searchResults)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;

                switch (field.fieldType)
                {
                    case 0: // Album
                        card.ExtraMouseUp = mainWindow.albumCardClicked;
                        card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["name"], $"{field.score}Album • {(string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["artist"]}", (string?)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["cover"]);
                        break;
                    case 1: // Artist
                        card.ExtraMouseUp = mainWindow.artistCardClicked;
                        card.setCardInfo(field.fieldKey, $"{field.score}Artist • {((JArray)mainWindow.audioHandler.audioLibrary["artists"][field.fieldKey]["discography"]).Count()} Songs", @"C:\Users\iONSZ\source\repos\WPFTutorial\WPFTutorial\bin\Debug\net6.0-windows7.0\media\a231f2e0-2998-4e00-bdc6-2c70f58dee5a.jpg");

                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = new Uri("pack://application:,,,/../Lib/artisticon.png");
                        card.albumImg.ImageSource = img;
                        img.EndInit();

                        break;
                    case 2: // Song
                        card.ExtraMouseUp = mainWindow.songCardClicked;
                        card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["title"], $"{field.score}Song • {(string)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["artist"]}", (string?)mainWindow.audioHandler.audioLibrary["albums"][(Int32)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["album"]]["cover"]);
                        break;
                    case 3: // Playlist
                        card.ExtraMouseUp = mainWindow.playlistCardClicked;
                        card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["playlists"][field.fieldId]["title"], $"Playlist • {((JArray)(mainWindow.audioHandler.audioLibrary["playlists"][field.fieldId]["tracklist"])).Count()} Songs", (string?)mainWindow.audioHandler.audioLibrary["playlists"][field.fieldId]["cover"]);
                        break;
                    default:
                        break;
                }

                AlbumContainer.InternalGrid.Children.Add(card);
            }

            /*
            foreach (SearchableField field in searchedAlbums)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;
                card.ExtraMouseUp = mainWindow.albumCardClicked;


                card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["name"], $"Album • {(string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["artist"]}", (string?)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["cover"]);
                AlbumContainer.InternalGrid.Children.Add(card);
            }
            foreach (SearchableField field in searchedArtists)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;
                card.ExtraMouseUp = mainWindow.artistCardClicked;
                card.setCardInfo(field.fieldKey, $"Artist • {((JArray)mainWindow.audioHandler.audioLibrary["artists"][field.fieldKey]["discography"]).Count()} Songs", @"C:\Users\iONSZ\source\repos\WPFTutorial\WPFTutorial\bin\Debug\net6.0-windows7.0\media\a231f2e0-2998-4e00-bdc6-2c70f58dee5a.jpg");

                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri("pack://application:,,,/../Lib/artisticon.png");
                card.albumImg.ImageSource = img;
                img.EndInit();

                AlbumContainer.InternalGrid.Children.Add(card);
            }
            foreach (SearchableField field in searchedSongs)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;
                card.ExtraMouseUp = mainWindow.songCardClicked;


                card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["title"], $"Song • {(string)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["artist"]}", (string?)mainWindow.audioHandler.audioLibrary["albums"][(Int32)mainWindow.audioHandler.audioLibrary["songs"][field.fieldId]["album"]]["cover"]);
                AlbumContainer.InternalGrid.Children.Add(card);
            }
            */
            AlbumContainer.RefreshAlbumCards();
        }
        public bool QualifiesForSearch(SearchableField field, string searchQuery)
        {
            field.score = 0;
            string key = field.fieldKey;
            searchQuery = searchQuery.ToLower();

            // Checks if field contains characters, if it doesnt then stop entirely
            bool qualifies = key.ToLower().Contains(searchQuery);
            if (qualifies) { field.score++; }
            else { return false; }

            // Check if field starts with characters in any of its words
            string[] words = key.ToLower().Split(" ");
            int wordidx = 0;
            foreach (string word in words)
            {
                string beginningSection;

                // Skip if word is too short
                try
                {
                    beginningSection = word.Substring(0, searchQuery.Length);
                } catch (ArgumentOutOfRangeException e)
                {
                    continue;
                }

                if (beginningSection == searchQuery)
                {
                    field.score++;

                    // Check if the FIRST word is the query
                    if (wordidx == 0) { field.score++; }
                }

                // If word exactly matches query
                if (word == searchQuery) { field.score++; }

                wordidx++;
            }
            // If key and query EXACTLY match
            if (key.ToLower() == searchQuery)
            {
                field.score++;
            }

            return true;
        }
    }
    public class SearchableField
    {
        public string fieldKey;
        public int fieldId;
        public int score;
        public int fieldType; // 0: Album, 1: Artist, 2: Song, 3: Playlist
        public SearchableField(string key, int id, int type)
        {
            fieldKey = key;
            fieldId = id;
            fieldType = type;
        }
    }
}

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
        public void ComputeSearch(string searchText)
        {
            AlbumContainer.InternalGrid.Children.Clear();
            if (searchText == "") {
                AlbumContainer.RefreshAlbumCards();
                return;
            }
            List<SearchableField> searchedAlbums = AlbumsToSearch.FindAll((SearchableField field) => { return field.fieldKey.Contains(searchText); });
            List<SearchableField> searchedSongs = SongsToSearch.FindAll((SearchableField field) => { return field.fieldKey.Contains(searchText); });
            List<SearchableField> searchedPlaylists = PlaylistsToSearch.FindAll((SearchableField field) => { return field.fieldKey.Contains(searchText); });
            List<SearchableField> searchedArtists = ArtistsToSearch.FindAll((SearchableField field) => { return field.fieldKey.Contains(searchText); });

            foreach (SearchableField field in searchedAlbums)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;
                card.ExtraMouseUp = mainWindow.albumCardClicked;


                card.setCardInfo((string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["name"], (string)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["artist"], (string?)mainWindow.audioHandler.audioLibrary["albums"][field.fieldId]["cover"]);
                AlbumContainer.InternalGrid.Children.Add(card);
            }
            foreach (SearchableField field in searchedArtists)
            {
                AlbumCard card = new AlbumCard();
                card.albumId = field.fieldId;
                card.ExtraMouseUp = mainWindow.artistCardClicked;
                card.setCardInfo(field.fieldKey, $"{((JArray)mainWindow.audioHandler.audioLibrary["artists"][field.fieldKey]["discography"]).Count()} Songs", @"C:\Users\iONSZ\source\repos\WPFTutorial\WPFTutorial\bin\Debug\net6.0-windows7.0\media\a231f2e0-2998-4e00-bdc6-2c70f58dee5a.jpg");

                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri("pack://application:,,,/../Lib/artisticon.png");
                card.albumImg.ImageSource = img;
                img.EndInit();

                AlbumContainer.InternalGrid.Children.Add(card);
            }
            AlbumContainer.RefreshAlbumCards();
        }
    }
    public class SearchableField
    {
        public string fieldKey;
        public int fieldId;
        public SearchableField(string key, int id)
        {
            fieldKey = key;
            fieldId = id;
        }
    }
}

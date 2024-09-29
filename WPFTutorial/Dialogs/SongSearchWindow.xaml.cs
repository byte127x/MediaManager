using MediaManager.Widgets.MainPages;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
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

namespace MediaManager.Dialogs
{
    /// <summary>
    /// Interaction logic for SongSearchWindow.xaml
    /// </summary>
    public partial class SongSearchWindow : Window
    {
        List<SearchableField> SongsToSearch;
        SearchPage sp;
        AudioHandler audioHandler;
        public int selectedSongId;
        public string selectedSongFullName;
        public SongSearchWindow()
        {
            InitializeComponent();
            MainWindow mw = (MainWindow)Application.Current.MainWindow;
            audioHandler = mw.audioHandler;
            sp = mw.searchPage;

            SongsToSearch = mw.allSongs;
            SearchBar.InternalText.PreviewKeyUp += Search;

            ListView.InnerView.PreviewMouseDoubleClick += InnerView_PreviewMouseDoubleClick;
        }

        private void InnerView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            selectedSongId = (int)(((ListViewItem)(ListView.InnerView.Items[ListView.InnerView.SelectedIndex])).Tag);
            selectedSongFullName = ((ListViewItem)(ListView.InnerView.Items[ListView.InnerView.SelectedIndex])).Content.ToString();
            this.Close();
        }

        public void Search(object sender, EventArgs e)
        {
            ListView.InnerView.Items.Clear();
            string searchText = SearchBar.InternalText.Text;
            List<SearchableField> searchedSongs = SongsToSearch.FindAll((SearchableField field) => { return sp.QualifiesForSearch(field, searchText); });
            searchedSongs.Sort(sp.CompareFieldsByScore);
            foreach (SearchableField field in searchedSongs)
            {
                ListViewItem item = new ListViewItem();
                string artistName = (string)(audioHandler.audioLibrary["songs"][field.fieldId]["artist"]);
                item.Content = $"{artistName} - {field.fieldKey}";
                item.Tag = field.fieldId;
                ListView.InnerView.Items.Add(item);
            }
        }
    }
}

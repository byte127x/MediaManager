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

namespace MediaManager.Dialogs
{
    /// <summary>
    /// Interaction logic for PlaylistCreator.xaml
    /// </summary>
    public partial class PlaylistCreator : Window
    {
        public PlaylistCreator()
        {
            InitializeComponent();
            this.Loaded += PlaylistCreator_Loaded;

            TitleInput.InternalText.Text = "New Playlist";
            DescInput.InternalText.Text = "";
        }

        private void addNewItem(string name)
        {
            ListViewItem item = new ListViewItem();
            item.Content = name;
            PlaylistBody.InnerView.Items.Add(item);
        }

        private void PlaylistCreator_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            int index = PlaylistBody.InnerView.SelectedIndex;
            if (index >= PlaylistBody.InnerView.Items.Count-1) { return; }

            ListViewItem activeitem = (ListViewItem)(PlaylistBody.InnerView.Items[index]);
            PlaylistBody.InnerView.Items.RemoveAt(index);


            //MessageBox.Show(activeitem.Parent.GetType().ToString());
            PlaylistBody.InnerView.Items.Insert(index+1, activeitem);
            PlaylistBody.InnerView.SelectedIndex = index+1;

            PlaylistBody.InnerView.Focus();
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            int index = PlaylistBody.InnerView.SelectedIndex;
            if (index <= 0) { return; }

            ListViewItem activeitem = (ListViewItem)(PlaylistBody.InnerView.Items[index]);
            PlaylistBody.InnerView.Items.RemoveAt(index);


            //MessageBox.Show(activeitem.Parent.GetType().ToString());
            PlaylistBody.InnerView.Items.Insert(index - 1, activeitem);
            PlaylistBody.InnerView.SelectedIndex = index - 1;

            PlaylistBody.InnerView.Focus();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            SongSearchWindow ssw = new SongSearchWindow();
            App.AddMicaEffect(ssw);
            ssw.ShowDialog();
            if (ssw.selectedSongFullName == null) { return; }

            ListViewItem item = new ListViewItem();
            item.Content = ssw.selectedSongFullName;
            item.Tag = ssw.selectedSongId;
            PlaylistBody.InnerView.Items.Add(item);
            
        }
    }
}

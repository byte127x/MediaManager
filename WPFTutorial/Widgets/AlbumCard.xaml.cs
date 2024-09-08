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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for AlbumCard.xaml
    /// </summary>
    public partial class AlbumCard : UserControl
    {
        public static string AlbumTitleName;
        Storyboard albumHoverStoryboard;
        Storyboard albumUnhoverStoryboard;
        Storyboard albumPressedStoryboard;
        public int albumId;
        public bool haventMovedOut = false;
        public AlbumCard()
        {
            InitializeComponent();

            albumHoverStoryboard = (Storyboard)FindResource("albumHoverStoryboard");
            albumUnhoverStoryboard = (Storyboard)FindResource("albumUnhoverStoryboard");
            albumPressedStoryboard = (Storyboard)FindResource("albumPressedStoryboard");

            this.MouseEnter += onHover;
            this.MouseLeave += onUnhover;
            this.PreviewMouseDown += onMouseDown;
            this.PreviewMouseUp += onMouseUp;
        }
        public void setCardInfo(string albumTitle, string artistTitle, string? albumCoverPath)
        {
            albumText.Text = albumTitle;
            artistText.Text = artistTitle;

            BitmapImage img = new BitmapImage();
            img.BeginInit();
            if (albumCoverPath != null)
            {
                if (albumCoverPath[1] != ':')
                {
                    img.UriSource = new Uri(albumCoverPath, UriKind.Relative);
                }
                else
                {
                    img.UriSource = new Uri(albumCoverPath);
                }
            } else {
                img.UriSource = new Uri("pack://application:,,,/../Lib/albumicon.png");
            }
            albumImg.ImageSource = img;
            img.EndInit();
        }
        public void onHover(object sender, RoutedEventArgs e)
        {
            albumHoverStoryboard.Begin(((AlbumCard)sender).MainBorder);
            albumUnhoverStoryboard.Stop(((AlbumCard)sender).MainBorder);
        }
        public void onUnhover(object sender, RoutedEventArgs e)
        {
            haventMovedOut = false;
            albumUnhoverStoryboard.Begin(((AlbumCard)sender).MainBorder);
            albumHoverStoryboard.Stop(((AlbumCard)sender).MainBorder);
        }
        public void onMouseDown(object sender, MouseEventArgs e)
        {
            haventMovedOut = true;
            albumPressedStoryboard.Begin(((AlbumCard)sender).MainBorder);
        }
        public void onMouseUp(object sender, MouseEventArgs e)
        {
            //isDown = false;
            albumPressedStoryboard.Stop(((AlbumCard)sender).MainBorder);
            albumHoverStoryboard.Begin(((AlbumCard)sender).MainBorder);
        }
    }
}

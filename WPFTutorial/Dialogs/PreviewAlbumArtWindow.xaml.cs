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
    /// Interaction logic for PreviewAlbumArtWindow.xaml
    /// </summary>
    public partial class PreviewAlbumArtWindow : Window
    {
        double size = 400;
        Point startdragpos;
        double lastPercentMouseAcrossX = 0;
        double lastPercentMouseAcrossY = 0;
        public PreviewAlbumArtWindow()
        {
            InitializeComponent();
            App.AddMicaEffect(this);
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            double originalsize = size;
                if (e.Delta > 0)
                {
                    size *= 1.2;
                }
                else
                {
                    size /= 1.2;
                }
                if (size < 10)
                {
                    size = 10;
                }
                ArtViewer.Width = size;
                ArtViewer.Height = size;
                ScrollerThingy.UpdateLayout();

            //ScrollerThingy.ScrollToVerticalOffset(0);
            //ScrollerThingy.ScrollToHorizontalOffset(0);
            if (e.Delta > 0)
            {

                double percentMouseIsAcrossX = (Mouse.GetPosition(this).X + ScrollerThingy.HorizontalOffset) / originalsize;
                double percentMouseIsAcrossY = (Mouse.GetPosition(this).Y + ScrollerThingy.VerticalOffset) / originalsize;

                ScrollerThingy.ScrollToVerticalOffset(percentMouseIsAcrossY * ScrollerThingy.ScrollableHeight);
                ScrollerThingy.ScrollToHorizontalOffset(percentMouseIsAcrossX * ScrollerThingy.ScrollableWidth);

                lastPercentMouseAcrossX = percentMouseIsAcrossX;
                lastPercentMouseAcrossY = percentMouseIsAcrossY;
            } else
            {
                ScrollerThingy.ScrollToVerticalOffset(lastPercentMouseAcrossY * ScrollerThingy.ScrollableHeight);
                ScrollerThingy.ScrollToHorizontalOffset(lastPercentMouseAcrossX * ScrollerThingy.ScrollableWidth);

            }
        }
        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                //MessageBox.Show((startdragpos.X - lastdragpos.X).ToString());
                double xoffset = (startdragpos.X - Mouse.GetPosition(this).X) + ScrollerThingy.HorizontalOffset;
                double yoffset = (startdragpos.Y - Mouse.GetPosition(this).Y) + ScrollerThingy.VerticalOffset;


                double percentMouseIsAcrossX = ((this.ActualWidth / 2) + ScrollerThingy.HorizontalOffset) / size;
                double percentMouseIsAcrossY = ((this.ActualHeight / 2) + ScrollerThingy.VerticalOffset) / size;
                lastPercentMouseAcrossX = percentMouseIsAcrossX;
                lastPercentMouseAcrossY = percentMouseIsAcrossY;

                ScrollerThingy.ScrollToVerticalOffset(yoffset);
                ScrollerThingy.ScrollToHorizontalOffset(xoffset);

            }
            startdragpos = Mouse.GetPosition(this);

        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startdragpos = Mouse.GetPosition(this);
        }

        private void Grid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

            double percentMouseIsAcrossX = ((this.ActualWidth / 2) + ScrollerThingy.HorizontalOffset) / size;
            double percentMouseIsAcrossY = ((this.ActualHeight / 2) + ScrollerThingy.VerticalOffset) / size;
            lastPercentMouseAcrossX = percentMouseIsAcrossX;
            lastPercentMouseAcrossY = percentMouseIsAcrossY;

        }
    }
}

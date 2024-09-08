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
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Interop;
using TsudaKageyu;
using TagLib;

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for AddFolderButton.xaml
    /// </summary>
    public partial class AddFolderButton : UserControl
    {
        Storyboard albumHoverStoryboard;
        Storyboard albumUnhoverStoryboard;
        Storyboard albumPressedStoryboard;
        public int albumId;

        IconExtractor IconEx = new IconExtractor("imageres.dll");
        public AddFolderButton()
        {
            InitializeComponent();

            albumHoverStoryboard = (Storyboard)FindResource("albumHoverStoryboard");
            albumUnhoverStoryboard = (Storyboard)FindResource("albumUnhoverStoryboard");
            albumPressedStoryboard = (Storyboard)FindResource("albumPressedStoryboard");

            this.MouseEnter += onHover;
            this.MouseLeave += onUnhover;
            this.PreviewMouseDown += onMouseDown;
            this.PreviewMouseUp += onMouseUp;

            //Icon img = System.Drawing.SystemIcons.WinLogo;

            setIcon(4);
        }
        public void setIcon(int id)
        {
            Icon img = IconEx.GetIcon(id);
            Bitmap bitmap = ExtractVistaIcon(img);
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            MainIcon.Source = wpfBitmap;
        }
        public void onHover(object sender, RoutedEventArgs e)
        {
            albumHoverStoryboard.Begin(((AddFolderButton)sender).MainBorder);
            albumUnhoverStoryboard.Stop(((AddFolderButton)sender).MainBorder);
        }
        public void onUnhover(object sender, RoutedEventArgs e)
        {
            albumUnhoverStoryboard.Begin(((AddFolderButton)sender).MainBorder);
            albumHoverStoryboard.Stop(((AddFolderButton)sender).MainBorder);
        }
        public void onMouseDown(object sender, MouseEventArgs e)
        {
            albumPressedStoryboard.Begin(((AddFolderButton)sender).MainBorder);
        }
        public void onMouseUp(object sender, MouseEventArgs e)
        {
            albumPressedStoryboard.Stop(((AddFolderButton)sender).MainBorder);
            albumHoverStoryboard.Begin(((AddFolderButton)sender).MainBorder);
        }
        // from stack overflow also
        Bitmap ExtractVistaIcon(Icon icoIcon)
        {
            Bitmap bmpPngExtracted = null;
            try
            {
                byte[] srcBuf = null;
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                { icoIcon.Save(stream); srcBuf = stream.ToArray(); }
                const int SizeICONDIR = 6;
                const int SizeICONDIRENTRY = 16;
                int iCount = BitConverter.ToInt16(srcBuf, 4);
                for (int iIndex = 0; iIndex < iCount; iIndex++)
                {
                    int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                    int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                    int iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);
                    if (iWidth == 0 && iHeight == 0 && iBitCount == 32)
                    {
                        int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                        int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                        System.IO.MemoryStream destStream = new System.IO.MemoryStream();
                        System.IO.BinaryWriter writer = new System.IO.BinaryWriter(destStream);
                        writer.Write(srcBuf, iImageOffset, iImageSize);
                        destStream.Seek(0, System.IO.SeekOrigin.Begin);
                        bmpPngExtracted = new Bitmap(destStream); // This is PNG! :)
                        break;
                    }
                }
            }
            catch { return null; }
            return bmpPngExtracted;
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}

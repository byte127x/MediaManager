using MediaManager.Dialogs;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace MediaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow MainWin;
        AudioHandler audioHandler;
        public void loadAudioHandler()
        {
            if (audioHandler == null)
            {
                MainWin = ((MainWindow)System.Windows.Application.Current.MainWindow);
                audioHandler = MainWin.audioHandler;
            }
        }
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.loadAudioHandler();
        }

        // Mica Theme
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,
            DWMWA_MICA_EFFECT = 1029,
            DWMWA_SYSTEMBACKDROP_TYPE = 38,

            DWMSBT_AUTO = 0,
            DWMSBT_DISABLE = 1, // None
            DWMSBT_MAINWINDOW = 2, // Mica
            DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
            DWMSBT_TABBEDWINDOW = 4 // Tabbed
        }
        public static int SetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, int parameter)
            => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
        public static int ExtendFrame(IntPtr hwnd, MARGINS margins)
            => DwmExtendFrameIntoClientArea(hwnd, ref margins);

        public static void UpdateStyleAttributes(Window win, HwndSource source)
        {
            var darkThemeEnabled = true;

            int trueValue = 0x01;
            int falseValue = 0x00;

            source.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            MARGINS margins = new MARGINS();
            margins.cxLeftWidth = -1;
            margins.cxRightWidth = -1;
            margins.cyTopHeight = -1;
            margins.cyBottomHeight = -1;
            ExtendFrame(source.Handle, margins);

            //if (darkThemeEnabled)
            //    DwmSetWindowAttribute(source.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            //else
            //    DwmSetWindowAttribute(source.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));


            //SetWindowAttribute(new WindowInteropHelper(this).Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, 4);

            SetWindowAttribute(new WindowInteropHelper(win).Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, 4);
            SetWindowAttribute(new WindowInteropHelper(win).Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, 0x01);

            // Hack to make scrollbars appear in dark mode
            FileProgressbarDialog prog = new FileProgressbarDialog();
            prog.Close();
        }

        // Apply Mica
        public static void Window_Loaded(Window win, object sender, RoutedEventArgs e)
        {

        }
        public static void Window_ContentRendered(Window win, object sender, System.EventArgs e)
        {
            if (System.Environment.OSVersion.Version.Major <= 6)
            {

                win.Background = System.Windows.Media.Brushes.Transparent;
            }
            else
            {
                // Apply Mica brush
                UpdateStyleAttributes(win, HwndSource.FromHwnd((new WindowInteropHelper((Window)sender)).Handle));
            }
        }

        public static void AddMicaEffect(Window win)
        {
            win.Loaded += (object sender, RoutedEventArgs e) => { Window_Loaded(win, sender, e); };
            win.ContentRendered += (object sender, System.EventArgs e) => { Window_ContentRendered(win, sender, e); };

        }
    }

}

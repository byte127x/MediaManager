using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {
        List<ScrollViewer> scrollers;
        public HomePage()
        {
            InitializeComponent();
            this.SizeChanged += onResize;
        }
        public void onResize(object sender, EventArgs e)
        {
            scrollers = this.InternalGrid.Children.OfType<ScrollViewer>().ToList();
            foreach (ScrollViewer s in scrollers)
            {
                double defaultHeight = 260;
                if (s.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
                {
                    defaultHeight += SystemParameters.HorizontalScrollBarHeight;
                }
                s.Height = defaultHeight;
            }
        }
    }
}

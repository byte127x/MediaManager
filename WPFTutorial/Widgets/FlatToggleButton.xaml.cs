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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for FlatToggleButton.xaml
    /// </summary>
    public partial class FlatToggleButton : ToggleButton
    {
        Storyboard mouseEnterBeginStoryboard;
        Storyboard mouseDownBeginStoryboard;
        Storyboard mouseLeaveBeginStoryboard;
        public FlatToggleButton()
        {
            InitializeComponent();
            mouseEnterBeginStoryboard = (Storyboard)FindResource("mouseEnterBeginStoryboard");
            mouseDownBeginStoryboard = (Storyboard)FindResource("mouseDownBeginStoryboard");
            mouseLeaveBeginStoryboard = (Storyboard)FindResource("mouseLeaveBeginStoryboard");

            this.MouseEnter += FlatMouseEnter;
            this.MouseLeave += FlatMouseLeave;
            this.PreviewMouseUp += FlatMouseUp;
            this.PreviewMouseDown += FlatMouseDown;
        }

        public void FlatMouseDown(object sender, MouseEventArgs e)
        {
            mouseEnterBeginStoryboard.Stop((ToggleButton)sender);
            mouseLeaveBeginStoryboard.Stop((ToggleButton)sender);
            mouseDownBeginStoryboard.Begin((ToggleButton)sender);
        }
        public void FlatMouseUp(object sender, MouseEventArgs e)
        {
            mouseDownBeginStoryboard.Stop((ToggleButton)sender);
            mouseEnterBeginStoryboard.Begin((ToggleButton)sender);
        }
        public void FlatMouseEnter(object sender, MouseEventArgs e) {
            mouseLeaveBeginStoryboard.Stop((ToggleButton)sender);
            mouseEnterBeginStoryboard.Begin((ToggleButton)sender);
        }
        public void FlatMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsChecked == true) { return; }
            mouseDownBeginStoryboard.Stop((ToggleButton)sender);
            mouseEnterBeginStoryboard.Stop((ToggleButton)sender);
            mouseLeaveBeginStoryboard.Begin((ToggleButton)(sender));
        }
    }
}

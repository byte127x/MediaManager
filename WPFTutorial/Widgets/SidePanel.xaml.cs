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
using System.Runtime.InteropServices;

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for SidePanel.xaml
    /// </summary>
    public partial class SidePanel : UserControl
    {
        public Storyboard toggleStartHover;
        public Storyboard toggleEndHover;
        public Storyboard toggleStartClick;
        List<ToggleButton> toggleButtons;
        public Func<string, int> returnFunction;
        public ToggleButton activeButton;
        public SidePanel()
        {
            InitializeComponent();

            // Configure Toggle Animations
            toggleStartHover = (Storyboard)FindResource("toggleStartHover");
            toggleEndHover = (Storyboard)FindResource("toggleEndHover");
            toggleStartClick = (Storyboard)FindResource("toggleStartClick");

            toggleButtons = this.InternalGrid.Children.OfType<ToggleButton>().ToList();
            foreach (ToggleButton toggleButton in toggleButtons)
            {
                toggleButton.MouseEnter += toggleButton_HoverStart;
                toggleButton.MouseLeave += toggleButton_HoverEnd;
                toggleButton.PreviewMouseDown += toggleButton_ClickStart;
                toggleButton.PreviewMouseUp += toggleButton_ClickEnd;
                toggleButton.Checked += (object sender, RoutedEventArgs e) => { toggleButton_Toggle(sender, null); };
                toggleButton.Unchecked += (object sender, RoutedEventArgs e) => { toggleButton_Toggle(sender, null); };
            }
            homeBtn.IsChecked = true;
            homeBtn.Background = new SolidColorBrush(Color.FromArgb(8, 255, 255, 255));
        }
        // Toggle Functionality
        private void toggleButton_HoverStart(object sender, RoutedEventArgs e) {
            toggleEndHover.Stop((ToggleButton)sender);
            toggleStartHover.Begin((ToggleButton)sender);
        }
        private void toggleButton_HoverEnd(object sender, RoutedEventArgs e)
        {
            toggleStartHover.Stop((ToggleButton)sender);
            toggleEndHover.Begin((ToggleButton)sender);
        }
        public void toggleButton_ClickStart(object sender, RoutedEventArgs e) {
            toggleStartHover.Stop((ToggleButton)sender);
            toggleStartClick.Begin((ToggleButton)sender);
        }
        public void toggleButton_ClickEnd(object sender, RoutedEventArgs e) {
            toggleStartClick.Stop((ToggleButton)sender);
            toggleStartHover.Begin((ToggleButton)sender);
        }
        public void toggleButton_Toggle(object sender, [Optional] bool? isAutomatic) {
            ToggleButton tgl = (ToggleButton)sender;
            bool isChecked = false;
            if (tgl.IsChecked == true)
            {
                isChecked = true;
            }

            if (isChecked)
            {
                foreach (ToggleButton toggleButton in toggleButtons)
                {
                    if (toggleButton.Name != tgl.Name)
                    {
                        toggleButton.IsChecked = false;
                    }
                }
                toggleStartHover.Stop(tgl);
                toggleStartClick.Begin(tgl);
                activeButton = tgl;

                tgl.MouseEnter -= toggleButton_HoverStart;
                tgl.MouseLeave -= toggleButton_HoverEnd;
                tgl.PreviewMouseDown -= toggleButton_ClickStart;
                tgl.PreviewMouseUp -= toggleButton_ClickEnd;

                if (returnFunction != null)
                {
                    returnFunction(tgl.Name);
                }
            } else
            {
                if (tgl.IsMouseOver)
                {
                    tgl.IsChecked = true;
                    return;
                }
                toggleStartClick.Stop(tgl);
                toggleEndHover.Begin(tgl);

                tgl.MouseEnter += toggleButton_HoverStart;
                tgl.MouseLeave += toggleButton_HoverEnd;
                tgl.PreviewMouseDown += toggleButton_ClickStart;
                tgl.PreviewMouseUp += toggleButton_ClickEnd;
            }

        }
    }
}

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
    /// Interaction logic for CustomListView.xaml
    /// </summary>
    public partial class CustomListView : UserControl
    {
        Storyboard lvSelectedStoryboard;
        Storyboard lvUnselectedStoryboard;
        public Func<string, int> selChangedFunc;
        public CustomListView()
        {
            InitializeComponent();
            lvSelectedStoryboard = (Storyboard)FindResource("lvSelectedStoryboard");
            lvUnselectedStoryboard = (Storyboard)FindResource("lvUnselectedStoryboard");
        }

        private void InnerView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null) { return; }
            ListViewItem selecteditem = (ListViewItem)(InnerView.SelectedItem);
            foreach (ListViewItem item in InnerView.Items)
            {
                if (item == selecteditem)
                {
                    lvSelectedStoryboard.Begin(item);
                } else
                {
                    lvUnselectedStoryboard.Begin(item);
                }
            }
            if (selChangedFunc != null)
            {
                if (selecteditem == null) { return; }
                selChangedFunc(selecteditem.Content.ToString());
            }
        }
    }
}

using MediaManager.Dialogs;
using MicaWPF.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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

namespace MediaManager.Widgets
{
    /// <summary>
    /// Interaction logic for SongsPageColumnView.xaml
    /// </summary>
    public class InternalColumn : FrameworkElement
    {

        private Pen mainPen = new Pen(Brushes.Red, 10.0);
        
        public IEnumerable<FormattedText> texts = [];
        double dip;
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            double dip = VisualTreeHelper.GetDpi(new TextBlock()).PixelsPerDip;
            //drawingContext.DrawLine(mainPen, new Point(0, 0), new Point(this.ActualWidth, 640));

            int rowidx = 0;
            foreach (FormattedText font in texts)
            {
                if (font == null)
                {
                    rowidx++; continue;
                }
                if (this.ActualWidth < 11) { font.MaxTextWidth = this.ActualWidth; }
                else { font.MaxTextWidth = this.ActualWidth - 10; }
                
                drawingContext.DrawText(font, new Point(10, (2 + (20 * rowidx))));
                rowidx++;
            }
        }
        public void clearData()
        {
            texts = [];
        }
        public void addData(string data)
        {
            //rowInfo.Add(data);
            if (data == null) { data = ""; }
            FormattedText font = new FormattedText(data, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.White, dip);
            font.MaxTextHeight = 20;
            font.Trimming = TextTrimming.CharacterEllipsis;
            texts = texts.Append(font);
            //MessageBox.Show(texts.Count().ToString());
        }
    }
    public partial class SongsPageColumnView : UserControl
    {
        int columns = 0;
        int rows = 0;
        Border selectedBorder;
        public List<Border> allBorders = new List<Border>();
        List<InternalColumn> allColumnViews = new List<InternalColumn>();
        public Func<int, int> songPlaybackFunction;

        public SongsPageColumnView()
        {
            InitializeComponent();
            this.SizeChanged += FrameSizeChanged;
            //MessageBox.Show((songMenu == null).ToString());

            //AddColumn("hello");
            //AddItem(new string[] { "helloText", "worldText" });
        }
        public void ColumnSizeChanged(object sender, EventArgs e)
        {
            List<double> ColumnWidths = new List<double>();

            MainGrid.ColumnDefinitions.Clear();

            int idx = 0;
            foreach (ColumnDefinition col in HeaderGrid.ColumnDefinitions)
            {
                idx++;
                if (col.ActualWidth <= 1) { continue; }

                ColumnDefinition c = new ColumnDefinition();
                if ((idx == HeaderGrid.ColumnDefinitions.Count) && (MainScroller.ComputedVerticalScrollBarVisibility == Visibility.Visible))
                {
                    c.Width = new GridLength(col.ActualWidth - SystemParameters.VerticalScrollBarWidth, GridUnitType.Pixel);
                }
                else
                {
                    c.Width = new GridLength(col.ActualWidth, GridUnitType.Pixel);
                }
                MainGrid.ColumnDefinitions.Add(c);
            }
        }
        public void FrameSizeChanged(object sender, EventArgs e)
        {
            ColumnSizeChanged(sender, new DragDeltaEventArgs(1, 1));
        }
        public void ClearItems()
        {
            foreach(InternalColumn i in MainGrid.FindLogicalChildren<InternalColumn>()) { i.clearData(); }
            MainGrid.RowDefinitions.Clear();
            
            foreach (Border b in allBorders)
            {
                MainGrid.Children.Remove(b);
            }
            allBorders = new List<Border>();
            //selectedBorder.Background = new SolidColorBrush(Color.FromArgb(12, 255, 255, 255));
            
            selectedBorder = null;
            rows = 0;
        }
        public int AddItem(IEnumerable<string> itemInfo)
        {
            // Add Selection Area
            Border selectionArea = new Border();
            selectionArea.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            selectionArea.Tag = rows;
            selectionArea.CornerRadius = new CornerRadius(5);
            selectionArea.SetValue(Grid.RowProperty, rows);
            selectionArea.SetValue(Grid.ColumnSpanProperty, 9999);

            selectionArea.MouseDown += SelectRow;
            selectionArea.MouseEnter += HoverRow;
            selectionArea.MouseLeave += UnhoverRow;
            allBorders.Add(selectionArea);

            int itemIdx = 0;
            foreach (var item in itemInfo)
            {
                allColumnViews[itemIdx].addData(item);
                itemIdx++;
            }

            // Add row measurement
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(20, GridUnitType.Pixel);
            rowDefinition.MinHeight = 20;
            rowDefinition.MaxHeight = 20;
            MainGrid.RowDefinitions.Add(rowDefinition);
            MainGrid.Children.Add(selectionArea);

            rows++;
            return rows - 1;
        }
        public int AddColumn(string name)
        {
            if (columns != 0)
            {
                GridSplitter split = new GridSplitter();
                split.SetValue(Grid.ColumnProperty, HeaderGrid.ColumnDefinitions.Count);
                split.Background = new SolidColorBrush(Color.FromArgb(26, 255, 255, 255));
                split.HorizontalAlignment = HorizontalAlignment.Stretch;
                split.VerticalAlignment = VerticalAlignment.Stretch;
                split.DragDelta += ColumnSizeChanged;
                split.DragCompleted += ColumnSizeChanged;

                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(1, GridUnitType.Pixel);
                HeaderGrid.Children.Add(split);
                HeaderGrid.ColumnDefinitions.Add(column);
            }

            TextBlock title = new TextBlock();
            title.Text = name;
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.VerticalAlignment = VerticalAlignment.Center;
            title.SetValue(Grid.ColumnProperty, HeaderGrid.ColumnDefinitions.Count);
            HeaderGrid.Children.Add(title);

            ColumnDefinition c = new ColumnDefinition();
            c.Width = new GridLength(1, GridUnitType.Star);
            c.MinWidth = 20;
            HeaderGrid.ColumnDefinitions.Add(c);
            ColumnDefinition cmain = new ColumnDefinition();
            cmain.Width = new GridLength(1, GridUnitType.Star);
            MainGrid.ColumnDefinitions.Add(cmain);

            // Add Text Element to Main Grid
            InternalColumn i = new InternalColumn();
            i.SetValue(Grid.ColumnProperty, columns);
            i.SetValue(Grid.RowSpanProperty, 9999);
            MainGrid.Children.Add(i);
            allColumnViews.Add(i);

            columns++;
            return columns - 1;
        }
        public void SelectRowByIndex(int index)
        {
            SelectRow(allBorders[index], null);
        }
        public void DoubleClicked(Border selected)
        {
            songPlaybackFunction((int)selected.Tag);
        }
        public void HoverRow(object sender, EventArgs e)
        {
            Border selectionArea = (Border)sender;
            if (Mouse.LeftButton == MouseButtonState.Pressed) { SelectRow(sender, null); return; } // SelectRow
            if (selectedBorder != null)
            {
                if (selectionArea.Tag == selectedBorder.Tag)
                {
                    return;
                }
            }
            selectionArea.Background = new SolidColorBrush(Color.FromArgb(12, 255, 255, 255));
        }
        public void UnhoverRow(object sender, EventArgs? e)
        {
            Border selectionArea = (Border)sender;
            if (selectedBorder != null)
            {
                if (selectionArea.Tag == selectedBorder.Tag)
                {
                    return;
                }
            }
            selectionArea.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
        }
        public void SelectRow(object sender, MouseButtonEventArgs e)
        {
            Border selectionArea = (Border)sender;
            if (e != null) { 
                if (e.ClickCount == 2) { DoubleClicked(selectionArea); }
                else if (e.RightButton == MouseButtonState.Pressed) { RightClickMenu(selectionArea); } 
            }
            
            selectionArea.Background = new SolidColorBrush(Color.FromArgb(22, 255, 255, 255));
            selectedBorder = selectionArea;
            foreach (Border b in allBorders)
            {
                if (b.Tag != selectedBorder.Tag)
                {
                    UnhoverRow(b, null);
                }
            }
        }

        public void RightClickMenu(Border selected)
        {
            SongContextMenu menu = new SongContextMenu();
            menu.songid = (int)selected.Tag;
            menu.WindowStartupLocation = WindowStartupLocation.Manual;
            menu.Left = System.Windows.Forms.Control.MousePosition.X;
            menu.Top = System.Windows.Forms.Control.MousePosition.Y;
            (Application.Current.MainWindow as MainWindow).Dialogs.Add(menu);
            menu.Show();
            Application.Current.MainWindow.Focus();
        }
    }
}

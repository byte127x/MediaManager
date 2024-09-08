using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for DynamicAlbumContainer.xaml
    /// </summary>
    public partial class DynamicAlbumContainer : UserControl
    {
        List<AlbumCard> albumCards;
        int MinWidth;
        int MaxWidth;
        TextBlock? block;
        public DynamicAlbumContainer()
        {
            InitializeComponent();
            MinWidth = 200;
            MaxWidth = 300;

            RefreshAlbumCards();
            this.SizeChanged += DynamicAlbumContainer_SizeChanged;
        }
        public void RefreshAlbumCards()
        {
            albumCards = this.InternalGrid.Children.OfType<AlbumCard>().ToList();

            if (albumCards.Count < 1)
            {
                block = new TextBlock();
                block.Text = "No Albums ...";
                block.Margin = new Thickness(15);
                block.FontSize = 32;
                this.InternalGrid.Children.Add(block);
                return;
            }
            //SizeChangedEventArgs s = new SizeChangedEventArgs();
            //s.NewSize.Width = this.Width;

            if (this.ActualWidth > 0)
            {
                DynamicAlbumContainer_SizeChanged(this, this.ActualWidth);
            }

        }

        private void DynamicAlbumContainer_SizeChanged(object sender, object inputsize)
        {
            if (albumCards.Count < 1)
            {
                return;
            }
            if (block != null)
            {
                this.InternalGrid.Children.Remove(block);
            }

            double width;
            if (inputsize.GetType() == typeof(SizeChangedEventArgs))
            {
                width = ((SizeChangedEventArgs)(inputsize)).NewSize.Width;
            } else
            {
                width = (double)(inputsize);
            }

            // 9 cards, 2 columns

            double newChildSize;
            int MaxColumns;
            if (ScrollView.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                MaxColumns = (int)Math.Floor((this.ActualWidth - SystemParameters.VerticalScrollBarWidth) / (double)MinWidth);
                if (MaxColumns == 0) { MaxColumns = 1; }
                else if (MaxColumns >= albumCards.Count) { MaxColumns = albumCards.Count; }

                newChildSize = (width - SystemParameters.VerticalScrollBarWidth) / MaxColumns;
            } else
            {
                MaxColumns = (int)Math.Floor((this.ActualWidth) / (double)MinWidth);
                if (MaxColumns == 0) { MaxColumns = 1; }
                else if (MaxColumns >= albumCards.Count) { MaxColumns = albumCards.Count; }

                newChildSize = width / MaxColumns; 
            }

            if (newChildSize > MaxWidth)
            {
                newChildSize = MaxWidth;
            }

            int MaxRows = (int)Math.Ceiling((double)(albumCards.Count) / (double)(MaxColumns))-1;

            //InternalGrid.MaxWidth = MaxColumns * (MinWidth*2);
            InternalGrid.ColumnDefinitions.Clear();
            InternalGrid.RowDefinitions.Clear();

            int idx = 0;
            int col = 0;
            int row = 0;
            Console.WriteLine(MaxColumns.ToString());

            var rowD = new RowDefinition();
            rowD.Height = GridLength.Auto;
            InternalGrid.RowDefinitions.Add(rowD);

            var extraHeight = (int)(38 + 20);
            foreach (AlbumCard card in albumCards)
            {
                idx++;
                if (row == 0)
                {
                    var colDef = new ColumnDefinition();
                    colDef.Width = new GridLength(newChildSize, GridUnitType.Pixel);
                    InternalGrid.ColumnDefinitions.Add(colDef);
                }
                Grid.SetColumn(card, col);
                Grid.SetRow(card, row);
                col++;
                
                //card.InnerText.Text = $"C:{col},R:{row},MAX:{MaxColumns}\nID:{idx}";

                if (col >= MaxColumns)
                {
                    col = 0;
                    row++;
                }
                card.Height = newChildSize + extraHeight;
            }
            for (int i = 0; i < MaxRows; i++)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = new GridLength(newChildSize+extraHeight, GridUnitType.Pixel);
                InternalGrid.RowDefinitions.Add(rowDef);
            }

            //Trace.WriteLine($"{InternalGrid.ActualWidth} VS {this.ActualWidth}");
        }
    }
}

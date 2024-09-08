using MicaWPF.Controls;
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
    /// Interaction logic for SongContextMenu.xaml
    /// </summary>
    public partial class SongContextMenu : Window
    {
        public int songid;
        public SongContextMenu()
        {
            InitializeComponent();
            MenuListView.SelectionChanged += SelectMenuItem;
            //App.AddMicaEffect(this);
        }
        public void SelectMenuItem(object sender, SelectionChangedEventArgs e)
        {
            int menuindex = MenuListView.SelectedIndex;
            MainWindow main = Application.Current.MainWindow as MainWindow;

            switch (menuindex)
            {
                default:
                    break;

                case 0:
                    try
                    {
                        main.audioHandler.Queue.Insert(main.audioHandler.Index + 1, songid);
                    }
                    catch (Exception ex) {
                        main.audioHandler.Queue.Add(songid);
                    }
                    break;
                case 1:
                    main.audioHandler.Queue.Add(songid);
                    break;
                case 2:
                    main.audioHandler.Queue = new List<int> { songid };
                    main.audioHandler.Index = 0;
                    main.audioHandler.Stop();
                    main.audioHandler.PlayAudio();
                    break;
            }
            main.audioHandler.ForceUpdateQueueGui();

            main.Dialogs.Remove(this);
            this.Close();
        }
    }
}

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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public Settings settings;
        public SettingsDialog()
        {
            InitializeComponent();
            App.AddMicaEffect(this);
        }

        public void ApplySettings(Settings? s)
        {
            if (s != null)
            {
                settings = s;
            }

            if (settings.username == null)
            {
                usernameInput.InternalText.Text = "No Username";
            } else
            {
                usernameInput.InternalText.Text = settings.username;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.username = usernameInput.InternalText.Text;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LibraryDataDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear your music library data? (No files will be deleted)", "Delete Library Data", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes) {
                settings.ClearLibraryData();
                MessageBox.Show("Library data cleared.", "Delete Library Data", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
    }
}

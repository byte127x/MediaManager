using Newtonsoft.Json.Linq;
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
        public Settings originalSettings;

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
                originalSettings = (Settings)s.Clone();
            }

            if (settings.username == null)
            {
                usernameInput.InternalText.Text = "No Username";
            } else
            {
                usernameInput.InternalText.Text = settings.username;
            }

            EqualizerSlider0.Value = settings.equalizer[0] + 10;
            EqualizerSlider1.Value = settings.equalizer[1] + 10;
            EqualizerSlider2.Value = settings.equalizer[2] + 10;
            EqualizerSlider3.Value = settings.equalizer[3] + 10;
            EqualizerSlider4.Value = settings.equalizer[4] + 10;
            EqualizerSlider5.Value = settings.equalizer[5] + 10;
            EqualizerSlider6.Value = settings.equalizer[6] + 10;
            EqualizerSlider7.Value = settings.equalizer[7] + 10;
            EqualizerSlider8.Value = settings.equalizer[8] + 10;
            EqualizerSlider9.Value = settings.equalizer[9] + 10;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            settings.username = usernameInput.InternalText.Text;
            settings.equalizer = new float[] {
                (float)EqualizerSlider0.Value - 10,
                (float)EqualizerSlider1.Value - 10,
                (float)EqualizerSlider2.Value - 10,
                (float)EqualizerSlider3.Value - 10,
                (float)EqualizerSlider4.Value - 10,
                (float)EqualizerSlider5.Value - 10,
                (float)EqualizerSlider6.Value - 10,
                (float)EqualizerSlider7.Value - 10,
                (float)EqualizerSlider8.Value - 10,
                (float)EqualizerSlider9.Value - 10,
            };

            if (settings.equalizer != originalSettings.equalizer) {
                settings.UpdateEqualizerData();
            }

            originalSettings = settings;
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

        private void ResetEqualizerButton_Click(object sender, RoutedEventArgs e)
        {
            EqualizerSlider0.Value = 10;
            EqualizerSlider1.Value = 10;
            EqualizerSlider2.Value = 10;
            EqualizerSlider3.Value = 10;
            EqualizerSlider4.Value = 10;
            EqualizerSlider5.Value = 10;
            EqualizerSlider6.Value = 10;
            EqualizerSlider7.Value = 10;
            EqualizerSlider8.Value = 10;
            EqualizerSlider9.Value = 10;

            settings.equalizer = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            settings.UpdateEqualizerData();
        }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Ayane.Annotations;

namespace Ayane.Themes
{
    class ThemeManager : DependencyObject, INotifyPropertyChanged
    {
        public ThemeManager()
        {
            AccentColor = (Color)Application.Current.Resources["SystemAccentColor"];
        }

        private Color _accentColor;
        public Color AccentColor
        {
            get { return _accentColor; }
            set
            {
                _accentColor = value;
                OnPropertyChanged();
                Application.Current.Resources["AccentColor"] = value;
                LightAccentColor = Color.FromArgb(0x99, value.R, value.G, value.B);
            }
        }

        private Color _lightAccentColor;
        public Color LightAccentColor
        {
            get { return _lightAccentColor; }
            set
            {
                _lightAccentColor = value;
                Application.Current.Resources[nameof(LightAccentColor)] = value;
                OnPropertyChanged();
            }
        }

        private Color _subtleColor = Colors.DarkGray;
        public Color SubtleColor { get { return _subtleColor; } set { _subtleColor = value; OnPropertyChanged(); } }

        private Color _lightSubtleColor = Colors.LightGray;
        public Color LightSubtleColor { get { return _lightSubtleColor; } set { _lightSubtleColor = value; OnPropertyChanged(); } }

        public Color ContrastInLightBackgroundColor => Colors.Black;
        public Color ContrastInDarkBackgroundColor => Colors.White;

        private SolidColorBrush _lightAccentBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x98, 0xd7, 0xf1));
        public SolidColorBrush LightAccentBrush { get { return _lightAccentBrush; } set { _lightAccentBrush = value; OnPropertyChanged(); } }

        private Color _backgroundColor = Colors.White;
        public Color BackgroundColor { get { return _backgroundColor; } set { _backgroundColor = value; OnPropertyChanged(); BackgroundBrush = new SolidColorBrush(value); } }

        private SolidColorBrush _backgroundBrush = new SolidColorBrush(Colors.White);
        public SolidColorBrush BackgroundBrush { get { return _backgroundBrush; } set { _backgroundBrush = value; OnPropertyChanged(); } }

        private SolidColorBrush _darkBaseBackroundBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x41, 0x41, 0x41));
        public SolidColorBrush DarkBaseBackgroundBrush { get { return _darkBaseBackroundBrush; } set { _darkBaseBackroundBrush = value; OnPropertyChanged(); } }

        private SolidColorBrush _topTitleBrush = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush TopTitleBrush { get { return _topTitleBrush; } set { _topTitleBrush = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

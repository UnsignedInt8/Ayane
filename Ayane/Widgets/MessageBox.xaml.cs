using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Ayane.Annotations;
using Ayane.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Ayane.Widgets
{
    public sealed partial class MessageBox : UserControl, INotifyPropertyChanged
    {
        private string _title;
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

        private string _message;
        public string Message { get { return _message; } set { _message = value; OnPropertyChanged(); } }

        public string PositiveButtonTitle { get; set; } = "YES";
        public string NegativeButtonTitle { get; set; } = "NO";
        public Visibility ButtonsVisibility { get; set; } = Visibility.Visible;

        public MessageBox()
        {
            InitializeComponent();
            if (DesignMode.DesignModeEnabled) return;
            PopupRoot.Closed += PopupRoot_Closed;
        }

        private void PopupRoot_Closed(object sender, object e)
        {
            var frameX = Window.Current.Content as FrameX;
            frameX?.Deattach(this);
        }

        private void PositiveButton_OnClick(object sender, RoutedEventArgs e)
        {
            CloseAnimation.Begin();
            OnPositiveClick();
        }

        private void NegativeButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnNegativeClick();
            CloseAnimation.Begin();
        }

        public event EventHandler PositiveButtonClick;
        public event EventHandler NegativeButtonClick;

        private void OnPositiveClick()
        {
            PositiveButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void OnNegativeClick()
        {
            NegativeButtonClick?.Invoke(this, EventArgs.Empty);
        }

        public void Show()
        {
            if (FrameX == null) return;
            FrameX.Attach(this);
            PopupRoot.Open();
            ShowAnimation.Begin();
        }

        public void Close()
        {
            CloseAnimation.Begin();
        }

        private void CloseAnimation_OnCompleted(object sender, object e)
        {
            PopupRoot.Close();
            FrameX?.Deattach(this);
        }

        private FrameX FrameX => Window.Current.Content as FrameX;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

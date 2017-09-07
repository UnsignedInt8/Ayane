using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ayane.Common;
using Ayane.FrameworkEx;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace Ayane.ViewModels
{
    class SleepingModeViewModel : ViewModelBase
    {
        private string _userDefinedNumber;
        private uint _songsRemainingNumber;
        private bool _isMinutesMode;
        private bool _isSongsCountMode;
        private bool _isSleepingModeStarted;
        private Timer _shutdownTimer;

        public SleepingModeViewModel()
        {
            _userDefinedNumber = LocalSettingsHelper.LoadValue(CommonKeys.SleepingModeNumber, (uint)30).ToString();
            _isMinutesMode = LocalSettingsHelper.LoadValue(CommonKeys.SleepingMinutesMode, true);
            _isSongsCountMode = LocalSettingsHelper.LoadValue(CommonKeys.SleepingSongsCountMode, false);

            StartCommand = new ActionCommand<object>(ExecuteStartCommand);

            ViewModelLocator.Instance.PlayerViewModel.ActiveSongChanged += PlayerViewModelOnActiveSongChanged;
        }

        public string UserDefinedNumber
        {
            get { return _userDefinedNumber; }
            set
            {
                if (_userDefinedNumber == value) return;
                _userDefinedNumber = value;

                uint number;
                if (uint.TryParse(value, out number) && number > 0)
                {
                    _songsRemainingNumber = number;
                    LocalSettingsHelper.SaveValue(CommonKeys.SleepingModeNumber, number);
                    return;
                }

                _userDefinedNumber = "30";
                RaisePropertyChanged();
            }
        }

        public bool IsMinutesMode
        {
            get { return _isMinutesMode; }
            set
            {
                if (_isMinutesMode == value) return;
                _isMinutesMode = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.SleepingMinutesMode, value);

                IsSongsCountMode = !value;
            }
        }

        public bool IsSongsCountMode
        {
            get { return _isSongsCountMode; }
            set
            {
                if (_isSongsCountMode == value) return;
                _isSongsCountMode = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.SleepingSongsCountMode, value);

                IsMinutesMode = !value;
            }
        }

        public bool IsSleepingModeStarted { get { return _isSleepingModeStarted; } set { _isSleepingModeStarted = value; RaisePropertyChanged(); } }

        public ICommand StartCommand { get; set; }

        private void ExecuteStartCommand(object o)
        {
            _shutdownTimer?.Dispose();
            _shutdownTimer = null;

            IsSleepingModeStarted = true;
            if (IsSongsCountMode)
            {
                _songsRemainingNumber = uint.Parse(UserDefinedNumber);
                if (_songsRemainingNumber > 0) _songsRemainingNumber--;
                return;
            }

            int minutes;
            if (!int.TryParse(UserDefinedNumber, out minutes))
            {
                minutes = 30;
            }

            _shutdownTimer = new Timer(state =>
            {
                if (!IsSleepingModeStarted) return;
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    IsSleepingModeStarted = false;
                    ViewModelLocator.Instance.PlayerViewModel.Pause();
                });
            }, null, TimeSpan.FromMinutes(minutes), TimeSpan.FromMilliseconds(-1));
        }

        public void OnCancelClick()
        {
            _shutdownTimer?.Dispose();
            _shutdownTimer = null;

            IsSleepingModeStarted = false;
            _songsRemainingNumber = 0;
        }

        private void PlayerViewModelOnActiveSongChanged(object sender, EventArgs eventArgs)
        {
            if (!IsSleepingModeStarted) return;
            if (!IsSongsCountMode) return;

            if (_songsRemainingNumber <= 0)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    IsSleepingModeStarted = false;
                    ViewModelLocator.Instance.PlayerViewModel.AutoPlay = false;
                    ViewModelLocator.Instance.PlayerViewModel.Pause();
                });
                return;
            }

            _songsRemainingNumber--;
        }
    }
}

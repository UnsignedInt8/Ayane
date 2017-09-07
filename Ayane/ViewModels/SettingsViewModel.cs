using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Display;
using Ayane.Common;
using Ayane.FrameworkEx;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        private bool _autoPlay;
        private bool _keepScreenOn;

        public SettingsViewModel()
        {
            _autoPlay = LocalSettingsHelper.LoadValue(CommonKeys.AutoPlay, false);
            _keepScreenOn = LocalSettingsHelper.LoadValue(CommonKeys.KeepScreenOn, false);
        }

        public bool AutoPlay
        {
            get { return _autoPlay; }
            set
            {
                if (_autoPlay == value) return;
                _autoPlay = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.AutoPlay, value);
            }
        }

        public bool KeepScreenOn
        {
            get { return _keepScreenOn; }
            set
            {
                if (_keepScreenOn == value) return;
                _keepScreenOn = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.KeepScreenOn, value);

                if (value) DisplayRequestHelper.RequestActive();
                else DisplayRequestHelper.RequestRelease();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ayane.Common;
using Ayane.FrameworkEx;
using GalaSoft.MvvmLight;

namespace Ayane.ViewModels
{
    class AudioEffectsViewModel : ViewModelBase
    {
        private bool _isEqualizerEnabled;
        private bool _isEchoEnabled;
        private bool _isReverbEnabled;
        private bool _isLimiterEnabled;

        private double _echoDelay;
        private double _reverbDecayTime;
        private double _limiterLoudness;
        private double _eq100HzGain;
        private double _eq900HzGain;
        private double _eq5kHzGain;
        private double _eq12kHzGain;

        private PlayerViewModel PlayerViewModel => ViewModelLocator.Instance.PlayerViewModel;

        public AudioEffectsViewModel()
        {
            _isEqualizerEnabled = LocalSettingsHelper.LoadValue(CommonKeys.EQEffectEnabled, false);
            _isEchoEnabled = LocalSettingsHelper.LoadValue(CommonKeys.EchoEffectEnabled, false);
            _isLimiterEnabled = LocalSettingsHelper.LoadValue(CommonKeys.LimiterEffectEnabled, false);
            _isReverbEnabled = LocalSettingsHelper.LoadValue(CommonKeys.ReverbEffectEnabled, false);

            _echoDelay = PlayerViewModel.XAudioPlayer?.EchoDelay ?? 50;
            _limiterLoudness = PlayerViewModel.XAudioPlayer?.LimiterLoudness ?? 20;
            _reverbDecayTime = PlayerViewModel.XAudioPlayer?.ReverbDecayTime ?? 2;
            _eq100HzGain = PlayerViewModel.XAudioPlayer?.EQBassGain ?? 50;
            _eq900HzGain = PlayerViewModel.XAudioPlayer?.EQLowMidGain ?? 20;
            _eq5kHzGain = PlayerViewModel.XAudioPlayer?.EQHighMidGain ?? 70;
            _eq12kHzGain = PlayerViewModel.XAudioPlayer?.EQHighPitchGain ?? 30;
        }

        public double EQ12kHzGain
        {
            get { return _eq12kHzGain; }
            set
            {
                _eq12kHzGain = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EQHighPitchGain, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.EQHighPitchGain = value;
            }
        }

        public double EQ5kHzGain
        {
            get { return _eq5kHzGain; }
            set
            {
                _eq5kHzGain = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EQHighMidGain, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.EQHighMidGain = value;
            }
        }

        public double EQ900HzGain
        {
            get { return _eq900HzGain; }
            set
            {
                _eq900HzGain = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EQLowMidGain, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.EQLowMidGain = value;
            }
        }

        public double EQ100HzGain
        {
            get { return _eq100HzGain; }
            set
            {
                if (Math.Abs(_eq100HzGain - value) < 0.01) return;
                _eq100HzGain = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EQBassGain, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.EQBassGain = value;
            }
        }

        public double LimiterLoudness
        {
            get { return _limiterLoudness; }
            set
            {
                _limiterLoudness = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.LimiterLoudness, (uint)value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.LimiterLoudness = (uint)value;
            }
        }

        public double ReverbDecayTime
        {
            get { return _reverbDecayTime; }
            set
            {
                _reverbDecayTime = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.ReverbDecay, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.ReverbDecayTime = value;
            }
        }

        public double EchoDelay
        {
            get { return _echoDelay; }
            set
            {
                _echoDelay = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EchoDelay, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.EchoDelay = value;
            }
        }

        public bool IsEqualizerEnabled
        {
            get { return _isEqualizerEnabled; }
            set
            {
                _isEqualizerEnabled = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EQEffectEnabled, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.IsEQEffectEnabled = value;
            }
        }

        public bool IsEchoEnabled
        {
            get { return _isEchoEnabled; }
            set
            {
                if (_isEchoEnabled == value) return;
                _isEchoEnabled = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.EchoEffectEnabled, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.IsEchoEffectEnabled = value;
            }
        }

        public bool IsReverbEnabled
        {
            get { return _isReverbEnabled; }
            set
            {
                if (_isReverbEnabled == value) return;
                _isReverbEnabled = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.ReverbEffectEnabled, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.IsReverbEffectEnabled = value;
            }
        }

        public bool IsLimiterEnabled
        {
            get { return _isLimiterEnabled; }
            set
            {
                if (_isLimiterEnabled == value) return;
                _isLimiterEnabled = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.LimiterEffectEnabled, value);

                if (PlayerViewModel.XAudioPlayer == null) return;
                PlayerViewModel.XAudioPlayer.IsLimiterEffectEnabled = value;
            }
        }
    }
}

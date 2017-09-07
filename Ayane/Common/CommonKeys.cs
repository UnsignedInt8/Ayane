using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Ayane.Common
{
    class CommonKeys
    {
        public const string IsFirstLaunch = nameof(IsFirstLaunch);
        public const string IsShuffleOn = nameof(IsShuffleOn);
        public const string IsLoopOn = nameof(IsLoopOn);
        public const string IsSingleRepeatOn = nameof(IsSingleRepeatOn);
        public const string LastUsedPlaylist = nameof(LastUsedPlaylist);
        public const string LastPlayedSong = nameof(LastPlayedSong);

        public const string ClearBackStack = nameof(ClearBackStack);
        public const string TokensTipUsed = nameof(TokensTipUsed);

        public const string SleepingModeNumber = nameof(SleepingModeNumber);
        public const string SleepingMinutesMode = nameof(SleepingMinutesMode);
        public const string SleepingSongsCountMode = nameof(SleepingSongsCountMode);

        public const string AutoPlay = nameof(AutoPlay);
        public const string KeepScreenOn = nameof(KeepScreenOn);
        
        /// <summary>
        /// Audio Effect Keys
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xapofx.fxecho_parameters(v=vs.85).aspx
        /// </summary>
        public const string EchoEffectEnabled = nameof(EchoEffectEnabled);
        public const string EchoWetDryMix = nameof(EchoWetDryMix); // Ratio of wet (processed) signal to dry (original) signal. 0.7f
        public const string EchoFeedback = nameof(EchoFeedback); // Amount of output to feed back into input. 0.5f
        public const string EchoDelay = nameof(EchoDelay); // Millseconds. 500ms

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xaudio2.xaudio2fx_reverb_parameters(v=vs.85).aspx
        /// </summary>
        public const string ReverbEffectEnabled = nameof(ReverbEffectEnabled);
        public const string ReverbWetDryMix = nameof(ReverbWetDryMix); // Percentage of the output that will be reverb. Allowable values are from 0 to 100. 50f
        public const string ReverbReflectionsDelay = nameof(ReverbReflectionsDelay);  // The delay time of the first reflection relative to the direct path. Permitted range is from 0 to 300 milliseconds. 
        public const string ReverbReverbDelay = nameof(ReverbReverbDelay); // Delay of reverb relative to the first reflection. Permitted range is from 0 to 85 milliseconds.
        public const string ReverbRearDelay = nameof(ReverbRearDelay); // Delay for the left rear output and right rear output. Permitted range is from 0 to 5 milliseconds.
        //public const string ReverbSideDelay = nameof(ReverbSideDelay); // Delay for the left side output and right side output. Permitted range is from 0 to 5 milliseconds.
        public const string ReverbDecay = nameof(ReverbDecay); // Reverberation decay time at 1 kHz. This is the time that a full scale input signal decays by 60 dB. Permitted range is from 0.1 to infinity seconds.

        public const string LimiterEffectEnabled = nameof(LimiterEffectEnabled);
        public const string LimiterLoudness = nameof(LimiterLoudness); // 1000; 1 ~ 1800
        public const string LimiterRelease = nameof(LimiterRelease); // 10

        // https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xapofx.fxeq_parameters(v=vs.85).aspx
        public const string EQEffectEnabled = nameof(EQEffectEnabled);
        public const string EQBassGain = nameof(EQBassGain); // 100Hz - 4.033 - 1.5
        public const string EQLowMidGain = nameof(EQLowMidGain); // 900Hz - 1.6888 - 1.5
        public const string EQHighMidGain = nameof(EQHighMidGain); // 5KHz - 2.4702 - 1.5
        public const string EQHighPitchGain = nameof(EQHighPitchGain); // 12KHz - 5.5958 - 2.0
    }
}

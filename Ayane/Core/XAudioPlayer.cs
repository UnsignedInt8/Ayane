using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Audio;
using Windows.Media.Effects;
using Windows.Media.Playback;
using Windows.Media.Render;
using Windows.Storage;
using Ayane.Common;

namespace Ayane.Core
{
    partial class XAudioPlayer : IDisposable
    {
        private Timer _timer;
        private AudioGraph _audioGraph;
        private AudioDeviceOutputNode _outputNode;
        private AudioFileInputNode _inputNode;
        private EchoEffectDefinition _echoEffect;
        private ReverbEffectDefinition _reverbEffect;
        private LimiterEffectDefinition _limiterEffect;
        private EqualizerEffectDefinition _equalizerEffect;
        private MediaPlaybackState _state = MediaPlaybackState.None;

        public async Task<bool> InitializeAudioGraphAsync()
        {
            var audioGraphSettings = new AudioGraphSettings(AudioRenderCategory.Media);
            var result = await AudioGraph.CreateAsync(audioGraphSettings);

            LastStatus = result.Status.ToString();
            if (result.Status != AudioGraphCreationStatus.Success) return false;

            _audioGraph = result.Graph;
            _audioGraph.UnrecoverableErrorOccurred += (sender, args) => OnErrorOccurred(args);

            var outputResult = await _audioGraph.CreateDeviceOutputNodeAsync();
            LastStatus = outputResult.Status.ToString();

            if (outputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                _audioGraph.Dispose();
                return false;
            }

            _outputNode = outputResult.DeviceOutputNode;

            CreateEchoEffect();
            CreateLimiterEffect();
            CreateReverbEffect();
            CreateEqualizerEffect();

            return true;
        }

        public async Task<bool> SetSourceAsync(StorageFile file)
        {
            Stop();

            _inputNode?.Dispose();
            _inputNode = null;

            var result = await _audioGraph.CreateFileInputNodeAsync(file);

            LastStatus = result.Status.ToString();
            if (result.Status != AudioFileNodeCreationStatus.Success) return false;

            lock (this)
            {
                try
                {
                    _inputNode = result.FileInputNode;

                    var invalidDuration = TimeSpan.FromMilliseconds(50);
                    if (_inputNode.Duration < invalidDuration) return false;

                    _inputNode.AddOutgoingConnection(_outputNode);
                    ThresoldDuration = _inputNode.Duration - invalidDuration;

                    _inputNode.EffectDefinitions.Add(CreateEchoEffect());
                    _inputNode.EffectDefinitions.Add(CreateLimiterEffect());
                    _inputNode.EffectDefinitions.Add(CreateReverbEffect());
                    _inputNode.EffectDefinitions.Add(CreateEqualizerEffect());

                    IsEchoEffectEnabled = IsEchoEffectEnabled;
                    IsReverbEffectEnabled = IsReverbEffectEnabled;
                    IsLimiterEffectEnabled = IsLimiterEffectEnabled;
                    IsEQEffectEnabled = IsEQEffectEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (AutoPlay) Play();

            return true;
        }

        public void Play()
        {
            if (_inputNode == null) return;

            _audioGraph?.Start();
            State = MediaPlaybackState.Playing;

            _timer?.Dispose();
            _timer = null;

            _timer = new Timer(state =>
            {
                if (!IsEnd)
                {
                    OnPositionChanged();
                    return;
                }

                _timer?.Dispose();
                _timer = null;

                OnMediaEnd();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void Pause()
        {
            if (_inputNode == null) return;

            _timer?.Dispose();
            _timer = null;

            _audioGraph.Stop();
            State = MediaPlaybackState.Paused;
        }

        public void Stop()
        {
            lock (this)
            {
                _inputNode?.Dispose();
                _timer?.Dispose();
                _timer = null;
                _inputNode = null;
            }

            try
            {
                _audioGraph?.Stop();
                _audioGraph?.ResetAllNodes();
            }
            catch (Exception)
            {

            }

            State = MediaPlaybackState.None;
        }

        public event EventHandler StateChanged;
        public event EventHandler MediaEnd;
        public event EventHandler<AudioGraphUnrecoverableErrorOccurredEventArgs> ErrorOccurred;
        public event EventHandler PositionChanged;

        public bool AutoPlay { get; set; }

        public MediaPlaybackState State
        {
            get { return _state; }
            private set
            {
                if (_state == value) return;
                _state = value;
                OnStateChanged();
            }
        }

        public string LastStatus { get; private set; }

        public TimeSpan Duration
        {
            get
            {
                try
                {
                    return _inputNode?.Duration ?? TimeSpan.Zero;
                }
                catch (Exception)
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan Position
        {
            get
            {
                try
                {
                    return _inputNode?.Position ?? TimeSpan.Zero;
                }
                catch (Exception)
                {
                    return TimeSpan.Zero;
                }
            }
            set
            {
                if (_inputNode == null) return;
                try
                {
                    if (value < _inputNode?.StartTime || value > _inputNode?.EndTime) return;
                    _inputNode?.Seek(value);
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public bool IsEnd
        {
            get
            {
                try
                {
                    return _inputNode?.Position > ThresoldDuration;
                }
                catch
                {
                    return true;
                }
            }
        }

        private TimeSpan ThresoldDuration { get; set; } = TimeSpan.Zero;

        public void Dispose()
        {
            _audioGraph?.Dispose();
            _outputNode?.Dispose();
            _inputNode?.Dispose();
            _timer?.Dispose();

            _timer = null;
            _audioGraph = null;
            _outputNode = null;
            _inputNode = null;
        }

        protected virtual void OnMediaEnd()
        {
            MediaEnd?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnErrorOccurred(AudioGraphUnrecoverableErrorOccurredEventArgs e)
        {
            State = MediaPlaybackState.None;
            LastStatus = e.Error.ToString();
            ErrorOccurred?.Invoke(this, e);
            OnMediaEnd();
        }

        protected virtual void OnPositionChanged()
        {
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Audio Effects Area
    /// </summary>
    partial class XAudioPlayer
    {
        private bool _isEchoEffectEnabled;
        public bool IsEchoEffectEnabled
        {
            get { return _isEchoEffectEnabled; }
            set
            {
                _isEchoEffectEnabled = value;
                _inputNode?.SwitchEffect(value, _echoEffect);
            }
        }

        public double EchoWetDryMix { get { return _echoEffect?.WetDryMix ?? 0.7d; } set { if (_echoEffect != null) _echoEffect.WetDryMix = value; } }
        public double EchoFeedback { get { return _echoEffect?.Feedback ?? 0.5d; } set { if (_echoEffect != null) _echoEffect.Feedback = value; } }
        public double EchoDelay { get { return _echoEffect?.Delay ?? 500d; } set { if (_echoEffect != null) _echoEffect.Delay = value; } }

        private EchoEffectDefinition CreateEchoEffect()
        {
            if (_echoEffect != null) return _echoEffect;

            _echoEffect = new EchoEffectDefinition(_audioGraph)
            {
                WetDryMix = EchoWetDryMix,
                Feedback = EchoFeedback,
                Delay = EchoDelay,
            };

            return _echoEffect;
        }

        private bool _isReverbEffectEnabled;
        public bool IsReverbEffectEnabled
        {
            get { return _isReverbEffectEnabled; }
            set
            {
                _isReverbEffectEnabled = value;
                _inputNode?.SwitchEffect(value, _reverbEffect);
            }
        }

        public double ReverbWetDryMix { get { return _reverbEffect?.WetDryMix ?? 50d; } set { if (_reverbEffect != null) _reverbEffect.WetDryMix = value; } }
        public uint ReverbReflectionsDelay { get { return _reverbEffect?.ReflectionsDelay ?? 120u; } set { if (_reverbEffect != null) _reverbEffect.ReflectionsDelay = value; } }
        public byte ReverbDelay { get { return _reverbEffect?.ReverbDelay ?? 30; } set { if (_reverbEffect != null) _reverbEffect.ReverbDelay = value; } }
        public byte ReverbRearDelay { get { return _reverbEffect?.RearDelay ?? 3; } set { if (_reverbEffect != null) _reverbEffect.RearDelay = value; } }
        public double ReverbDecayTime { get { return _reverbEffect?.DecayTime ?? 2d; } set { if (_reverbEffect != null) _reverbEffect.DecayTime = value; } }

        private ReverbEffectDefinition CreateReverbEffect()
        {
            if (_reverbEffect != null) return _reverbEffect;

            _reverbEffect = new ReverbEffectDefinition(_audioGraph)
            {
                WetDryMix = ReverbWetDryMix,
                ReflectionsDelay = ReverbReflectionsDelay,
                ReverbDelay = ReverbDelay,
                RearDelay = ReverbRearDelay,
                DecayTime = ReverbDecayTime,
            };

            return _reverbEffect;
        }

        private bool _isLimiterEffectEnabled;
        public bool IsLimiterEffectEnabled
        {
            get { return _isLimiterEffectEnabled; }
            set
            {
                _isLimiterEffectEnabled = value;
                _inputNode?.SwitchEffect(value, _limiterEffect);
            }
        }

        public uint LimiterLoudness { get { return _limiterEffect?.Loudness ?? 1000; } set { if (_limiterEffect != null) _limiterEffect.Loudness = value; } }
        public uint LimiterRelease { get { return _limiterEffect?.Release ?? 10; } set { if (_limiterEffect != null) _limiterEffect.Release = value; } }

        private LimiterEffectDefinition CreateLimiterEffect()
        {
            if (_limiterEffect != null) return _limiterEffect;

            _limiterEffect = new LimiterEffectDefinition(_audioGraph)
            {
                Loudness = LimiterLoudness,
                Release = LimiterRelease,
            };

            return _limiterEffect;
        }

        private bool _isEqEffectEnabled;
        public bool IsEQEffectEnabled
        {
            get { return _isEqEffectEnabled; }
            set
            {
                _isEqEffectEnabled = value;
                _inputNode?.SwitchEffect(value, _equalizerEffect);
            }
        }

        public double EQBassGain { get { return ConvertBackRange(_equalizerEffect?.Bands[0].Gain ?? 4.033); } set { if (_equalizerEffect != null) _equalizerEffect.Bands[0].Gain = ConvertRange(value); } }
        public double EQLowMidGain { get { return ConvertBackRange(_equalizerEffect?.Bands[1].Gain ?? 1.6888); } set { if (_equalizerEffect != null) _equalizerEffect.Bands[1].Gain = ConvertRange(value); } }
        public double EQHighMidGain { get { return ConvertBackRange(_equalizerEffect?.Bands[2].Gain ?? 2.4702); } set { if (_equalizerEffect != null) _equalizerEffect.Bands[2].Gain = ConvertRange(value); } }
        public double EQHighPitchGain { get { return ConvertBackRange(_equalizerEffect?.Bands[3].Gain ?? 5.5958); } set { if (_equalizerEffect != null) _equalizerEffect.Bands[3].Gain = ConvertRange(value); } }

        // Mapping the 0-100 scale of the slider to a value between the min and max gain
        private double ConvertRange(double value)
        {
            // These are the same values as the ones in xapofx.h
            const double fxeq_min_gain = 0.126;
            const double fxeq_max_gain = 7.94;

            double scale = (fxeq_max_gain - fxeq_min_gain) / 100;
            return (fxeq_min_gain + ((value) * scale));
        }

        private double ConvertBackRange(double result)
        {
            return 100d * (result - 0.126) / (7.94 - 0.126);
        }

        private EqualizerEffectDefinition CreateEqualizerEffect()
        {
            if (_equalizerEffect != null) return _equalizerEffect;

            _equalizerEffect = new EqualizerEffectDefinition(_audioGraph);
            _equalizerEffect.Bands[0].FrequencyCenter = 100;
            _equalizerEffect.Bands[0].Gain = 4.0333;
            _equalizerEffect.Bands[0].Bandwidth = 1.5;

            _equalizerEffect.Bands[1].FrequencyCenter = 900;
            _equalizerEffect.Bands[1].Gain = 1.6888;
            _equalizerEffect.Bands[1].Bandwidth = 1.5;

            _equalizerEffect.Bands[2].FrequencyCenter = 5000;
            _equalizerEffect.Bands[2].Gain = 2.4702;
            _equalizerEffect.Bands[2].Bandwidth = 1.5;

            _equalizerEffect.Bands[3].FrequencyCenter = 12000;
            _equalizerEffect.Bands[3].Gain = 5.5958;
            _equalizerEffect.Bands[3].Bandwidth = 2;

            return _equalizerEffect;
        }

    }

    partial class XAudioPlayer
    {
        public static readonly IReadOnlyList<string> SupportedEffectFormats = new[] { ".mp3", ".wma", ".m4a", ".wav" };
    }

    static class AudioNodeEx
    {
        public static void SwitchEffect(this IAudioNode node, bool on, IAudioEffectDefinition effect)
        {
            if (effect == null) return;

            if (on)
            {
                node.EnableEffectsByDefinition(effect);
            }
            else
            {
                node.DisableEffectsByDefinition(effect);
            }
        }
    }
}

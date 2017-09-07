using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Playback;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Streams;
using Ayane.Common;
using Ayane.Core;
using Ayane.FrameworkEx;
using Ayane.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace Ayane.ViewModels
{
    partial class PlayerViewModel : ViewModelBase
    {
        private bool _initialized;
        private XAudioPlayer _audioPlayer;
        private readonly MediaPlayer _mediaPlayer;
        private readonly SystemMediaTransportControls _systemMTC;

        public PlayerViewModel()
        {
            _mediaPlayer = new MediaPlayer { AudioCategory = MediaPlayerAudioCategory.Media, AutoPlay = true };
            _mediaPlayer.CommandManager.IsEnabled = false;
            _mediaPlayer.PlaybackSession.PlaybackStateChanged += (sender, args) => DispatcherHelper.CheckBeginInvokeOnUI(() => State = sender.PlaybackState);
            _mediaPlayer.PlaybackSession.PositionChanged += (sender, args) => DispatcherHelper.CheckBeginInvokeOnUI(() => NotifyPositionInternal(_mediaPlayer.PlaybackSession.Position));
            _mediaPlayer.MediaEnded += (sender, args) => OnCurrentTrackEnd();

            _systemMTC = SystemMediaTransportControls.GetForCurrentView();
            _systemMTC.IsEnabled = true;
            _systemMTC.IsPlayEnabled = true;
            _systemMTC.IsPauseEnabled = true;
            _systemMTC.IsNextEnabled = true;
            _systemMTC.IsPreviousEnabled = true;
            _systemMTC.IsStopEnabled = true;
            _systemMTC.ButtonPressed += SystemMTC_ButtonPressed;

            PlayCommand = new ActionCommand<object>(o => { AutoPlay = true; Resume(); });
            PauseCommand = new ActionCommand<object>(o => { AutoPlay = false; Pause(); });
            PlayNextCommand = new ActionCommand<object>(o => PlayNext());
            PlayPreviousCommand = new ActionCommand<object>(o => PlayPrevious());
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            _isLoopOn = LocalSettingsHelper.LoadValue(CommonKeys.IsLoopOn, false);
            _isShuffleOn = LocalSettingsHelper.LoadValue(CommonKeys.IsShuffleOn, false);
            _isSingleRepeatOn = LocalSettingsHelper.LoadValue(CommonKeys.IsSingleRepeatOn, false);

            await InitializeAudioGraphAsync();
        }

        private async Task InitializeAudioGraphAsync()
        {
            _audioPlayer = new XAudioPlayer();

            var successful = await _audioPlayer.InitializeAudioGraphAsync();
            if (!successful)
            {
                IsAudioEffectsSupport = false;
                return;
            }

            IsAudioEffectsSupport = true;
            _audioPlayer.MediaEnd += (sender, args) => OnCurrentTrackEnd();
            _audioPlayer.StateChanged += (sender, args) => DispatcherHelper.CheckBeginInvokeOnUI(() => State = _audioPlayer.State);
            _audioPlayer.ErrorOccurred += (sender, args) => OnCurrentTrackEnd();
            _audioPlayer.PositionChanged += (sender, args) => DispatcherHelper.CheckBeginInvokeOnUI(() => NotifyPositionInternal(_audioPlayer.Position));

            _audioPlayer.IsEQEffectEnabled = LocalSettingsHelper.LoadValue(CommonKeys.EQEffectEnabled, false);
            _audioPlayer.EQBassGain = LocalSettingsHelper.LoadValue(CommonKeys.EQBassGain, 50d);
            _audioPlayer.EQLowMidGain = LocalSettingsHelper.LoadValue(CommonKeys.EQLowMidGain, 20d);
            _audioPlayer.EQHighMidGain = LocalSettingsHelper.LoadValue(CommonKeys.EQHighMidGain, 70d);
            _audioPlayer.EQHighPitchGain = LocalSettingsHelper.LoadValue(CommonKeys.EQHighPitchGain, 30d);

            _audioPlayer.IsEchoEffectEnabled = LocalSettingsHelper.LoadValue(CommonKeys.EchoEffectEnabled, false);
            _audioPlayer.EchoDelay = LocalSettingsHelper.LoadValue(CommonKeys.EchoDelay, 50d);

            _audioPlayer.IsLimiterEffectEnabled = LocalSettingsHelper.LoadValue(CommonKeys.LimiterEffectEnabled, false);
            _audioPlayer.LimiterLoudness = LocalSettingsHelper.LoadValue(CommonKeys.LimiterLoudness, 20u);

            _audioPlayer.IsReverbEffectEnabled = LocalSettingsHelper.LoadValue(CommonKeys.ReverbEffectEnabled, false);
            _audioPlayer.ReverbDecayTime = LocalSettingsHelper.LoadValue(CommonKeys.ReverbDecay, 2d);
        }

        public event EventHandler ActiveSongChanged;
        public event EventHandler SkipToNext;
        public event EventHandler SkipToPrevious;
        public event EventHandler ShuffleChanged;

        public bool IsAudioEffectsSupport { get; private set; }
        public string PlaylistTitle { get; set; }

        public XAudioPlayer XAudioPlayer => _audioPlayer;

        public TimeSpan Duration => ActiveSong?.Duration ?? TimeSpan.Zero;

        private readonly TimeSpan _threshold = TimeSpan.FromSeconds(2);
        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                if (_position == value || (value > _position - _threshold && value < _position + _threshold)) return;
                _position = value;
                if (_audioPlayer != null) _audioPlayer.Position = value;
                _mediaPlayer.PlaybackSession.Position = value;
                RaisePropertyChanged();
            }
        }

        private MediaPlaybackState _state;
        public MediaPlaybackState State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                _state = value;
                UpdateSystemMTC();
                RaisePropertyChanged();
                Task.Run(async () => { await Task.Delay(TimeSpan.FromMilliseconds(150)); DispatcherHelper.CheckBeginInvokeOnUI(() => RaisePropertyChanged(nameof(IsPlaying))); });
            }
        }

        public bool IsPlaying => State == MediaPlaybackState.Playing;

        private int _activeSongIndex = -1;
        public int ActiveSongIndex
        {
            get { return _activeSongIndex; }
            set
            {
                if (_activeSongIndex == value) return;
                _activeSongIndex = value;
                PlaySongAtIndex(value);
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Position = TimeSpan.Zero;
                    RaisePropertyChanged(nameof(ActiveSong));
                    RaisePropertyChanged(nameof(PreviousSong));
                    RaisePropertyChanged(nameof(NextSong));
                    RaisePropertyChanged(nameof(Duration));
                    OnActiveSongChanged();
                    UpdateSystemMediaInfo();
                });
            }
        }

        public bool AutoPlay
        {
            get { return _audioPlayer?.AutoPlay ?? _mediaPlayer.AutoPlay; }
            set
            {
                _mediaPlayer.AutoPlay = value;
                if (_audioPlayer != null) _audioPlayer.AutoPlay = value;
            }
        }

        public Song ActiveSong => ActiveSongIndex >= 0 && ActiveSongIndex < Queue?.Count ? Queue?[ActiveSongIndex] : null;
        public Song PreviousSong => ActiveSongIndex >= 0 && ActiveSongIndex < Queue?.Count ? (ActiveSongIndex == 0 ? Queue?[Queue.Count - 1] : Queue?[ActiveSongIndex - 1]) : null;
        public Song NextSong => ActiveSongIndex >= 0 && ActiveSongIndex < Queue?.Count ? (ActiveSongIndex == Queue?.Count - 1 ? Queue?[0] : Queue?[ActiveSongIndex + 1]) : null;

        private ObservableCollection<Song> _queue;
        public ObservableCollection<Song> Queue
        {
            get { return _queue; }
            set
            {
                _queue = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSingleRepeatOn;
        public bool IsSingleRepeatOn
        {
            get { return _isSingleRepeatOn; }
            set
            {
                if (_isSingleRepeatOn == value) return;
                _isSingleRepeatOn = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.IsSingleRepeatOn, value);
            }
        }

        private bool _isLoopOn;
        public bool IsLoopOn
        {
            get { return _isLoopOn; }
            set
            {
                if (_isLoopOn == value) return;
                _isLoopOn = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.IsLoopOn, value);
            }
        }

        private bool _isShuffleOn;
        public bool IsShuffleOn
        {
            get { return _isShuffleOn; }
            set
            {
                _isShuffleOn = value;
                RaisePropertyChanged();
                LocalSettingsHelper.SaveValue(CommonKeys.IsShuffleOn, value);


                if (!(Queue?.Any() ?? false)) return;

                var activeSong = ActiveSong;

                if (value)
                {
                    var random = new Random((int)DateTime.Now.Ticks);
                    var length = Queue.Count - 1;
                    for (int i = 0; i <= length; i++)
                    {
                        var index = random.Next(i, length);
                        var tmp = Queue[i];
                        Queue[i] = Queue[index];
                        Queue[index] = tmp;
                    }
                }
                else
                {
                    if (_songslistCache == null) return;
                    Queue = new ObservableCollection<Song>(_songslistCache);
                }

                if (activeSong == null) return;
                _activeSongIndex = Queue?.IndexOf(activeSong) ?? -1;
                RaisePropertyChanged(nameof(PreviousSong));
                RaisePropertyChanged(nameof(ActiveSong));
                RaisePropertyChanged(nameof(NextSong));
                OnShuffleChanged();
            }
        }

        public ICommand PlayCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand PlayNextCommand { get; private set; }
        public ICommand PlayPreviousCommand { get; private set; }

        private int _lastSongsListHashcode;
        private IList<Song> _songslistCache;

        public void Play(Song song)
        {
            Play(new[] { song });
        }

        public void Play(IList<Song> songs, int startIndex = 0, bool force = false)
        {
            if (songs == null || songs.Count == 0) return;

            startIndex = startIndex >= 0 && startIndex < songs.Count ? startIndex : 0;

            var hash = songs.GetHashCode();
            if (hash != _lastSongsListHashcode) Queue = new ObservableCollection<Song>(songs);
            _lastSongsListHashcode = hash;
            _songslistCache = songs;

            if (force)
            {
                _activeSongIndex = -1;
            }

            ActiveSongIndex = startIndex;
            if (IsShuffleOn)
            {
                IsShuffleOn = true;
                _lastSongsListHashcode = -1;
            }

            OnShuffleChanged();
        }

        public void RemoveQueueSong(Song song)
        {
            if (ActiveSong?.Equals(song) ?? false) PlayNext();
            var index = Queue.IndexOf(song);
            if (index == -1) return;
            _activeSongIndex = index <= ActiveSongIndex ? _activeSongIndex - 1 : _activeSongIndex;
            Queue.Remove(song);

            if (Queue.Any()) return;
            Stop();
        }

        public void Resume()
        {
            _audioPlayer?.Play();
            _mediaPlayer.Play();
        }

        public void Pause()
        {
            _audioPlayer?.Pause();
            if (_mediaPlayer.Source != null) _mediaPlayer.Pause();
        }

        public void PlayNext()
        {
            if (Queue == null || Queue.Count == 0) return;
            ActiveSongIndex = ActiveSongIndex + 1 >= Queue.Count ? 0 : ActiveSongIndex + 1;
            OnSkipToNext();
        }

        public void PlayPrevious()
        {
            if (Queue == null || Queue.Count == 0) return;
            ActiveSongIndex = ActiveSongIndex - 1 < 0 ? Queue.Count - 1 : ActiveSongIndex - 1;
            OnSkipToPrevious();
        }

        public void Stop()
        {
            _audioPlayer?.Stop();
            _mediaPlayer.Pause();
            _mediaPlayer.Source = null;
        }

        public void ClearQueue()
        {
            Queue?.Clear();
        }

        public void ClearActiveSong()
        {
            ActiveSongIndex = -1;
        }

        protected virtual void OnSkipToNext()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => SkipToNext?.Invoke(this, EventArgs.Empty));
        }

        protected virtual void OnSkipToPrevious()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => SkipToPrevious?.Invoke(this, EventArgs.Empty));
        }

        protected virtual void OnShuffleChanged()
        {
            ShuffleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    partial class PlayerViewModel
    {
        private void OnCurrentTrackEnd()
        {
            if (IsSingleRepeatOn)
            {
                PlaySongAtIndex(ActiveSongIndex);
                return;
            }

            if (ActiveSongIndex == Queue.Count - 1)
            {
                ActiveSongIndex = -1;
                if (IsLoopOn) PlayNext();
                return;
            }

            PlayNext();
        }

        private async void PlaySongAtIndex(int index)
        {
            Stop();

            if (Queue == null || Queue.Count == 0 || index < 0 || index >= Queue.Count) return;
            var song = Queue[index];

            if (_audioPlayer != null && XAudioPlayer.SupportedEffectFormats.Any(t => t.Equals(song.FormatType, StringComparison.OrdinalIgnoreCase)))
            {
                var file = await song.ToStorageFileAsync();
                if (file == null)
                {
                    OnCurrentTrackEnd();
                    return;
                }

                if (await _audioPlayer.SetSourceAsync(file)) return;
            }

            var mediaItem = await song.ToMediaPlaybackItem();
            _mediaPlayer.AutoPlay = AutoPlay;
            _mediaPlayer.Source = mediaItem;
            //if (!AutoPlay) _mediaPlayer.Pause();

            if (mediaItem == null)
            {
                OnCurrentTrackEnd();
            }
        }

        protected virtual void OnActiveSongChanged()
        {
            ActiveSongChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NotifyPositionInternal(TimeSpan position)
        {
            _position = position;
            RaisePropertyChanged(nameof(Position));
        }
    }

    partial class PlayerViewModel
    {
        private void SystemMTC_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Next:
                    PlayNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    PlayPrevious();
                    break;
                case SystemMediaTransportControlsButton.Play:
                    AutoPlay = true;
                    Resume();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    AutoPlay = false;
                    Pause();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Stop();
                    break;
                case SystemMediaTransportControlsButton.Record:
                    break;
                case SystemMediaTransportControlsButton.FastForward:
                    break;
                case SystemMediaTransportControlsButton.Rewind:
                    break;
                case SystemMediaTransportControlsButton.ChannelUp:
                    break;
                case SystemMediaTransportControlsButton.ChannelDown:
                    break;
                default:
                    return;
            }
        }

        private void UpdateSystemMTC()
        {
            switch (State)
            {
                case MediaPlaybackState.Playing:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlaybackState.None:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaPlaybackState.Opening:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlaybackState.Buffering:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                default:
                    _systemMTC.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        private async void UpdateSystemMediaInfo()
        {
            if (_systemMTC == null) return;
            var song = ActiveSong;
            if (song == null) return;

            var updater = _systemMTC.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;

            try
            {
                updater.MusicProperties.Artist = song.ArtistName;
                updater.MusicProperties.Title = song.Title;
                updater.MusicProperties.AlbumTitle = song.AlbumTitle;

                if (string.IsNullOrEmpty(song.CoverFilePath))
                {
                    updater.Thumbnail = null;
                    return;
                }

                var file = await StorageFile.GetFileFromPathAsync(song.CoverFilePath);
                updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                updater.Update();
            }
        }
    }
}

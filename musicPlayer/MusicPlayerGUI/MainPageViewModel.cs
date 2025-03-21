using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using MusicPlayerLib;

namespace MusicPlayerGUI.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly MusicPlayer _musicPlayer;
        private System.Timers.Timer _timer;
        private CancellationTokenSource _notificationCts;
        private bool _isSeeking = false;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TrackItemViewModel> _tracks = new();

        [ObservableProperty]
        private ObservableCollection<string> _modes = new() { "Queue", "Buffer" };

        [ObservableProperty]
        private string _selectedMode = "Buffer";

        [ObservableProperty]
        private string _currentTrackName;

        [ObservableProperty]
        private string _trackTime;

        [ObservableProperty]
        private double _trackProgress;

        [ObservableProperty]
        private string _playPauseButtonText = "▶";

        [ObservableProperty]
        private string _notificationMessage = string.Empty;

        public MainPageViewModel()
        {
            _musicPlayer = new MusicPlayer();
            _musicPlayer.TrackFinished += OnTrackFinished;

            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += (s, e) => UpdateTrackProgress();
            _timer.Start();
        }

        partial void OnSelectedModeChanged(string value)
        {
            UpdateTracksBasedOnMode();
        }

        private void OnTrackFinished(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateTracksBasedOnMode();
                UpdateCurrentTrackInfo();
            });
        }

        [RelayCommand]
        private void LoadMusic()
        {
            try
            {
                _musicPlayer.LoadSongs(_path);
                UpdateTracksBasedOnMode();
                SetNotification("Music loaded successfully.");
            }
            catch (Exception ex)
            {
                SetNotification($"Music loading error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ShuffleQueue()
        {
            try
            {
                _musicPlayer.ShuffleQueue();
                if (_selectedMode == "Queue")
                {
                    UpdateTracksBasedOnMode();
                }
                SetNotification("Queue shuffled successfully.");
            }
            catch (Exception ex)
            {
                SetNotification($"Queue shuffle error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void DeleteTrack(TrackItemViewModel trackItem)
        {
            try
            {
                if (_selectedMode != "Queue")
                {
                    SetNotification("Delete action is only available in Queue mode.");
                    return;
                }

                int index = _tracks.IndexOf(trackItem);
                if (index >= 0)
                {
                    if (trackItem.IsCurrent)
                    {
                        int currentIndex = _musicPlayer.GetCurrentTrackIndex();
                        _musicPlayer.RemoveTracksFromQueueByIndices(new int[] { index });

                        if (_tracks.Count > currentIndex)
                        {
                            _musicPlayer.PlayTrack();
                            SetNotification("Current track removed. Playing track at same index.");
                        }
                        else if (_tracks.Count > 0)
                        {
                            _musicPlayer.PrevTrack();
                            _musicPlayer.PlayTrack();
                            SetNotification("Current track removed. Playing previous track.");
                        }
                        else
                        {
                            SetNotification("Current track removed. Queue is now empty.");
                        }
                        UpdateTracksBasedOnMode();
                        UpdateCurrentTrackInfo();
                    }
                    else
                    {
                        _musicPlayer.RemoveTracksFromQueueByIndices(new int[] { index });
                        UpdateTracksBasedOnMode();
                        UpdateCurrentTrackInfo();
                        SetNotification("Track removed from queue.");
                    }
                }
                else
                {
                    SetNotification("Track not found in queue.");
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Delete track error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AddTrack(TrackItemViewModel trackItem)
        {
            try
            {
                if (_selectedMode == "Buffer")
                {
                    var bufferTracks = _musicPlayer.GetBuffer();
                    int index = bufferTracks.FindIndex(t => t.FullPath == trackItem.FullPath);
                    if (index >= 0)
                    {
                        _musicPlayer.AddTracksToQueueByIndices(new int[] { index });
                        if (_selectedMode == "Queue")
                        {
                            UpdateTracksBasedOnMode();
                        }
                        SetNotification("Track added to queue.");
                    }
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Track addition error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void PreviousTrack()
        {
            try
            {
                _musicPlayer.PrevTrack();
                _musicPlayer.PlayTrack();
                PlayPauseButtonText = "||";
                UpdateCurrentTrackInfo();
                SetNotification("Playing previous track.");
            }
            catch (Exception ex)
            {
                SetNotification($"Previous track error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void PlayPause()
        {
            try
            {
                if (PlayPauseButtonText == "▶")
                {
                    _musicPlayer.PlayTrack();
                    PlayPauseButtonText = "||";
                    UpdateCurrentTrackInfo();
                    SetNotification("Playing track.");
                }
                else
                {
                    _musicPlayer.PauseTrack();
                    PlayPauseButtonText = "▶";
                    SetNotification("Track paused.");
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Play/Pause error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void NextTrack()
        {
            try
            {
                _musicPlayer.NextTrack();
                _musicPlayer.PlayTrack();
                PlayPauseButtonText = "||";
                UpdateCurrentTrackInfo();
                SetNotification("Playing next track.");
            }
            catch (Exception ex)
            {
                PlayPause();
                SetNotification($"Next track error: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SliderDragCompleted(double sliderValue)
        {
            _isSeeking = true;

            try
            {
                var totalTime = _musicPlayer.GetCurrentTrackTotalDuration();
                if (totalTime.TotalSeconds > 0)
                {
                    double desiredSeconds = (sliderValue / 100.0) * totalTime.TotalSeconds;
                    _musicPlayer.SeekTrack(desiredSeconds.ToString());
                    UpdateCurrentTrackInfo();
                    SetNotification("Track position updated.");
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Seek error: {ex.Message}");
            }
            finally
            {
                _isSeeking = false;
            }
        }

        [RelayCommand]
        private void SliderValueChanged(double sliderValue)
        {
            if (_isSeeking || _isUpdatingProgress) return;

            try
            {
                var totalTime = _musicPlayer.GetCurrentTrackTotalDuration();
                if (totalTime.TotalSeconds > 0)
                {
                    _isSeeking = true;
                    double desiredSeconds = (sliderValue / 100.0) * totalTime.TotalSeconds;
                    _musicPlayer.SeekTrack(desiredSeconds.ToString());
                    UpdateCurrentTrackInfo();
                    SetNotification("Track position updated.");
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Seek error: {ex.Message}");
            }
            finally
            {
                _isSeeking = false;
            }
        }

        private void UpdateTracksBasedOnMode()
        {
            try
            {
                _tracks.Clear();
                var list = _selectedMode == "Queue" ? _musicPlayer.GetQueue() : _musicPlayer.GetBuffer();
                for (int i = 0; i < list.Count; i++)
                {
                    var tivm = new TrackItemViewModel(list[i]);
                    tivm.IsCurrent = (_selectedMode == "Queue" && i == 0);
                    _tracks.Add(tivm);
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Error updating track list: {ex.Message}");
            }
        }

        private void UpdateCurrentTrackInfo()
        {
            try
            {
                var currentQueue = _musicPlayer.GetQueue();
                if (currentQueue.Count > 0)
                {
                    int currentIndex = _musicPlayer.GetCurrentTrackIndex();
                    var currentTrack = currentQueue[currentIndex];
                    CurrentTrackName = currentTrack.Title;
                    var currentTime = _musicPlayer.GetCurrentTrackTime();
                    var totalTime = _musicPlayer.GetCurrentTrackTotalDuration();
                    TrackTime = $"{currentTime:mm\\:ss} / {totalTime:mm\\:ss}";

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        for (int i = 0; i < Tracks.Count; i++)
                        {
                            Tracks[i].IsCurrent = (i == currentIndex);
                        }
                    });
                }
                else
                {
                    CurrentTrackName = string.Empty;
                    TrackTime = "00:00 / 00:00";
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Error updating track info: {ex.Message}");
            }
        }

        private bool _isUpdatingProgress = false;

        private void UpdateTrackProgress()
        {
            if (_isSeeking) return;

            try
            {
                var currentTime = _musicPlayer.GetCurrentTrackTime();
                var totalTime = _musicPlayer.GetCurrentTrackTotalDuration();

                if (totalTime.TotalSeconds > 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (_isSeeking) return;

                        _isUpdatingProgress = true;
                        TrackProgress = currentTime.TotalSeconds / totalTime.TotalSeconds * 100;
                        TrackTime = $"{currentTime:mm\\:ss} / {totalTime:mm\\:ss}";
                        _isUpdatingProgress = false;
                    });
                }
            }
            catch (Exception ex)
            {
                SetNotification($"Error updating progress: {ex.Message}");
            }
        }

        private async void SetNotification(string message)
        {
            if (_notificationCts != null)
            {
                _notificationCts.Cancel();
                _notificationCts.Dispose();
            }
            _notificationCts = new CancellationTokenSource();

            MainThread.BeginInvokeOnMainThread(() => { NotificationMessage = message; });

            try
            {
                await Task.Delay(3000, _notificationCts.Token);
                MainThread.BeginInvokeOnMainThread(() => { NotificationMessage = string.Empty; });
            }
            catch (TaskCanceledException) { }
        }
    }

    public partial class TrackItemViewModel : ObservableObject
    {
        private readonly Track _track;

        [ObservableProperty]
        private bool _isCurrent;

        public TrackItemViewModel(Track track)
        {
            _track = track;
        }

        public string FullPath => _track.FullPath;
        public string TrackName => _track.Title;
        public string TrackArtist => _track.Artist;
        public string Duration => _track.Duration.ToString(@"mm\:ss");
    }

    public class SliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is Slider slider)
            {
                return slider.Value;
            }
            if (value is int intValue)
            {
                return (double)intValue;
            }
            if (value is double doubleValue)
            {
                return doubleValue;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueChangedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ValueChangedEventArgs args)
            {
                return args.NewValue;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

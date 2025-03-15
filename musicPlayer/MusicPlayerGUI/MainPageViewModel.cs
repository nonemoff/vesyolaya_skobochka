using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MusicPlayerLib;
using System.Collections.Generic;

namespace MusicPlayerGUI
{
    public enum NotificationType
    {
        Error,
        Notification
    }

    public class NotificationStyle
    {
        public Color BackgroundColor { get; }
        public Color TextColor { get; }
        public Color ButtonColor { get; }

        public NotificationStyle(Color backgroundColor, Color textColor, Color buttonColor)
        {
            BackgroundColor = backgroundColor;
            TextColor = textColor;
            ButtonColor = buttonColor;
        }
    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private MusicPlayer _musicPlayer;
        private bool _isBufferMode = true;
        private string _modeLabel;
        private string _toggleModeButtonText;
        private string _songsPath;
        private bool _notificationVisible;
        private string _notificationMessage;
        private string _notificationButtonText;

        public ObservableCollection<TrackViewModel> LoadedTracks { get; set; } = new ObservableCollection<TrackViewModel>();
        public ObservableCollection<TrackViewModel> QueueTracks { get; set; } = new ObservableCollection<TrackViewModel>();

        public string ModeLabel
        {
            get => _modeLabel;
            set { _modeLabel = value; OnPropertyChanged(nameof(ModeLabel)); }
        }

        public string ToggleModeButtonText
        {
            get => _toggleModeButtonText;
            set { _toggleModeButtonText = value; OnPropertyChanged(nameof(ToggleModeButtonText)); }
        }

        public string SongsPath
        {
            get => _songsPath;
            set { _songsPath = value; OnPropertyChanged(nameof(SongsPath)); }
        }

        public bool NotificationVisible
        {
            get => _notificationVisible;
            set { _notificationVisible = value; OnPropertyChanged(nameof(NotificationVisible)); }
        }

        public string NotificationMessage
        {
            get => _notificationMessage;
            set { _notificationMessage = value; OnPropertyChanged(nameof(NotificationMessage)); }
        }

        public string NotificationButtonText
        {
            get => _notificationButtonText;
            set { _notificationButtonText = value; OnPropertyChanged(nameof(NotificationButtonText)); }
        }

        // Свойство для привязки коллекции треков, зависящее от режима
        public ObservableCollection<TrackViewModel> DisplayedTracks => _isBufferMode ? LoadedTracks : QueueTracks;

        public ICommand LoadSongsCommand { get; }
        public ICommand ToggleModeCommand { get; }
        public ICommand NotificationCommand { get; }

        // Стили уведомлений
        private readonly Dictionary<NotificationType, NotificationStyle> _notificationStyles = new()
        {
            { NotificationType.Error, new NotificationStyle(Colors.DarkRed, Colors.White, Colors.Black) },
            { NotificationType.Notification, new NotificationStyle(Colors.Blue, Colors.White, Colors.Gray) }
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageViewModel()
        {
            _musicPlayer = new MusicPlayer();
            ModeLabel = "Buffer";
            ToggleModeButtonText = "Show Queue";

            LoadSongsCommand = new AsyncCommand(LoadSongsAsync);
            ToggleModeCommand = new Command(ToggleMode);
            NotificationCommand = new Command(HideNotification);
        }

        private async Task LoadSongsAsync()
        {
            try
            {
                // Асинхронная загрузка песен (оборачиваем синхронный метод в Task.Run)
                await Task.Run(() => _musicPlayer.LoadSongs(SongsPath));
                var bufferTracks = _musicPlayer.GetBuffer();

                LoadedTracks.Clear();
                for (int i = 0; i < bufferTracks.Count; i++)
                {
                    var track = bufferTracks[i];
                    LoadedTracks.Add(new TrackViewModel(
                        i,
                        track.Title,
                        track.Artist,
                        FormatDuration(track.Duration),
                        AddTrackToQueue));
                }
                ShowNotification($"Loaded {bufferTracks.Count} track(s).", "OK", NotificationType.Notification);
                OnPropertyChanged(nameof(DisplayedTracks));
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, "Close", NotificationType.Error);
            }
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.Hours > 0)
                return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            else
                return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        private void ToggleMode()
        {
            _isBufferMode = !_isBufferMode;
            if (_isBufferMode)
            {
                ModeLabel = "Buffer";
                ToggleModeButtonText = "Show Queue";
            }
            else
            {
                ModeLabel = "Queue";
                ToggleModeButtonText = "Show Buffer";
                UpdateQueueTracks();
            }
            OnPropertyChanged(nameof(DisplayedTracks));
        }

        private void HideNotification()
        {
            NotificationVisible = false;
        }

        private void ShowNotification(string message, string buttonText, NotificationType type)
        {
            if (!_notificationStyles.TryGetValue(type, out var style))
                return;

            NotificationMessage = message;
            NotificationButtonText = buttonText;
            NotificationVisible = true;
        }

        private void AddTrackToQueue(int trackIndex)
        {
            try
            {
                _musicPlayer.AddTracksToQueueByIndices(new int[] { trackIndex });
                ShowNotification("Track added to queue", "OK", NotificationType.Notification);
                if (!_isBufferMode)
                {
                    UpdateQueueTracks();
                }
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, "Close", NotificationType.Error);
            }
        }

        private void UpdateQueueTracks()
        {
            var queue = _musicPlayer.GetQueue();
            QueueTracks.Clear();
            for (int i = 0; i < queue.Count; i++)
            {
                var track = queue[i];
                QueueTracks.Add(new TrackViewModel(
                    i,
                    track.Title,
                    track.Artist,
                    FormatDuration(track.Duration),
                    index => { } // Действие не требуется для треков в очереди
                ));
            }
            OnPropertyChanged(nameof(DisplayedTracks));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Реализация команды для асинхронных операций
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            await _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // ViewModel для отдельного трека
    public class TrackViewModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public int Index { get; set; }
        public ICommand AddToQueueCommand { get; set; }

        public TrackViewModel(int index, string title, string artist, string duration, Action<int> addToQueueAction)
        {
            Index = index;
            Title = title;
            Artist = artist;
            Duration = duration;
            AddToQueueCommand = new Command(() => addToQueueAction(Index));
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MusicPlayerLib;
using System.Collections.Generic;

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

namespace MusicPlayerGUI
{
    public partial class MainPage : ContentPage
    {
        private MusicPlayer _musicPlayer = new MusicPlayer();

        public ObservableCollection<TrackViewModel> Tracks { get; set; } = new();

        private readonly Dictionary<NotificationType, NotificationStyle> _notificationStyles = new()
        {
            { NotificationType.Error, new NotificationStyle(Colors.DarkRed, Colors.White, Colors.Black) },
            { NotificationType.Notification, new NotificationStyle(Colors.Blue, Colors.White, Colors.Gray) }
        };

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.Window.MinimumHeight = 300;
            this.Window.MinimumWidth = 600;
        }

        private void OnNotificationButtonClicked(object sender, EventArgs e)
        {
            NotificationBlock.IsVisible = false;
        }

        private void OnLoadSongsClicked(object sender, EventArgs e)
        {
            string path = PathEntry.Text;

            try
            {
                _musicPlayer.LoadSongs(path);
                var tracks = _musicPlayer.GetBuffer();
                Tracks.Clear();

                for (int i = 0; i < tracks.Count; i++)
                {
                    Tracks.Add(new TrackViewModel(
                        i,
                        tracks[i].Title,
                        tracks[i].Artist,
                        FormatDuration(tracks[i].Duration),
                        AddTrackToQueue));
                }

                ShowNotification($"Loaded {tracks.Count} track(s).", "OK", NotificationType.Notification);
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

        private void AddTrackToQueue(int trackIndex)
        {
            try
            {
                _musicPlayer.AddTracksToQueueByIndices(new int[] { trackIndex });
                ShowNotification("Track added to queue", "OK", NotificationType.Notification);
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, "Close", NotificationType.Error);
            }
        }

        private void ShowNotification(string message, string buttonText, NotificationType type)
        {
            if (!_notificationStyles.TryGetValue(type, out var style))
                return;

            NotificationLabel.Text = message;
            NotificationButton.Text = buttonText;
            NotificationBlock.IsVisible = true;

            NotificationBlock.BackgroundColor = style.BackgroundColor;
            NotificationLabel.TextColor = style.TextColor;
            NotificationButton.BackgroundColor = style.ButtonColor;
        }
    }
}

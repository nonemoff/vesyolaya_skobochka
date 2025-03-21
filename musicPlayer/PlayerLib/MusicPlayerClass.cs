using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NAudio.Wave;

namespace MusicPlayerLib
{
    public enum PlayerState
    {
        Stopped,
        Playing,
        Paused
    }

    public class MusicPlayer
    {
        private Buffer _buffer = new Buffer();
        private Queue _queue = new Queue();
        private string _defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private IWavePlayer _waveOut;
        private AudioFileReader _audioFileReader;
        private PlayerState _playerState = PlayerState.Stopped;
        public event EventHandler? TrackFinished;

        public void LoadSongs(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _defaultPath;
            }

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"LoadSongs: The directory '{path}' does not exist.");
            }

            var files = Directory.GetFiles(path, "*.mp3");
            if (files.Length == 0)
            {
                throw new InvalidOperationException($"LoadSongs: The directory '{path}' contains no mp3 files.");
            }

            foreach (var file in files)
            {
                _buffer.Add(new Track(file));
            }
        }

        public List<Track> GetBuffer()
        {
            return _buffer.GetTracks();
        }

        public List<Track> GetQueue()
        {
            return _queue.GetTracks();
        }

        public void AddTracksToQueueByIndices(int[] indices)
        {
            List<Track> tracksToAdd = _buffer.GetTracksByIndices(indices);
            foreach (Track track in tracksToAdd)
            {
                _queue.AddTrack(track);
            }
        }

        public void RemoveTracksFromQueueByIndices(int[] indices)
        {
            _queue.RemoveTracksByIndices(indices);
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }

        public void ClearQueue()
        {
            _queue.Clear();
        }

        public void ShuffleQueue()
        {
            _queue.Shuffle();
        }

        private void StopPlayback()
        {
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
            _playerState = PlayerState.Stopped;
        }

        public void PlayTrack()
        {
            if (_playerState == PlayerState.Paused && _waveOut != null)
            {
                _waveOut.Play();
                _playerState = PlayerState.Playing;
                return;
            }

            StopPlayback();
            Track currentTrack = _queue.GetCurrentTrack();

            try
            {
                _audioFileReader = new AudioFileReader(currentTrack.FullPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось загрузить трек '{currentTrack.FullPath}'.", ex);
            }

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioFileReader);
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            _waveOut.Play();
            _playerState = PlayerState.Playing;
        }

        public void PauseTrack()
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
                _playerState = PlayerState.Paused;
            }
        }

        public void NextTrack()
        {
            StopPlayback();
            _queue.NextTrack();
            PlayTrack();
        }

        public void PrevTrack()
        {
            StopPlayback();
            _queue.PrevTrack();
            PlayTrack();
        }

        public void SeekTrack(string timeCommand)
        {
            if (_audioFileReader == null)
            {
                throw new InvalidOperationException("Нет загруженного трека для перемотки.");
            }

            double seconds;
            TimeSpan newTime;
            if (timeCommand.StartsWith("+"))
            {
                if (!double.TryParse(timeCommand.Substring(1), out seconds))
                {
                    throw new ArgumentException("Неверный формат времени для перемотки.", nameof(timeCommand));
                }
                newTime = _audioFileReader.CurrentTime.Add(TimeSpan.FromSeconds(seconds));
            }
            else if (timeCommand.StartsWith("-"))
            {
                if (!double.TryParse(timeCommand.Substring(1), out seconds))
                {
                    throw new ArgumentException("Неверный формат времени для перемотки.", nameof(timeCommand));
                }
                newTime = _audioFileReader.CurrentTime.Subtract(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                if (!double.TryParse(timeCommand, out seconds))
                {
                    throw new ArgumentException("Неверный формат времени для перемотки.", nameof(timeCommand));
                }
                newTime = TimeSpan.FromSeconds(seconds);
            }

            if (newTime < TimeSpan.Zero)
            {
                newTime = TimeSpan.Zero;
            }
            if (newTime > _audioFileReader.TotalTime)
            {
                newTime = _audioFileReader.TotalTime;
            }
            _audioFileReader.CurrentTime = newTime;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_playerState != PlayerState.Paused && _audioFileReader != null && _audioFileReader.Position >= _audioFileReader.Length)
            {
                try
                {
                    NextTrack();
                    TrackFinished?.Invoke(this, EventArgs.Empty);
                }
                catch (ArgumentOutOfRangeException)
                {
                    StopPlayback();
                    TrackFinished?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public TimeSpan GetCurrentTrackTime()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.CurrentTime;
            }
            return TimeSpan.Zero;
        }

        public TimeSpan GetCurrentTrackTotalDuration()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime;
            }
            return TimeSpan.Zero;
        }

        public int GetCurrentTrackIndex()
        {
            return _queue.GetCurrentTrackIndex();
        }
    }
}

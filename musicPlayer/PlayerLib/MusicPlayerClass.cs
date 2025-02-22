using System;
using System.IO;
using NAudio.Wave;
using System.Linq;
using System.Collections.Generic;

namespace MusicPlayerLib
{
    public struct Track
    {
        public string FullPath { get; }
        public string FileName { get; }
        public TimeSpan Duration { get; }

        public Track(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            using var reader = new AudioFileReader(fullPath);
            Duration = TimeSpan.FromSeconds((int)reader.TotalTime.TotalSeconds);
        }
    }

    public class PlaybackQueue
    {
        private List<Track> _queue = new List<Track>();
        private int _currentIndex = -1;
        private WaveOutEvent? _player;
        private AudioFileReader? _audioFile;
        private bool _isPlaying;

        public bool AddTrack(Track track, out string errorMessage)
        {
            _queue.Add(track);
            errorMessage = string.Empty;
            return true;
        }

        public bool RemoveTrack(int index, out string errorMessage)
        {
            if (index < 0 || index >= _queue.Count)
            {
                errorMessage = "Error: Wrong index";
                return false;
            }
            if (index <= _currentIndex && _currentIndex > 0)
            {
                _currentIndex--;
            }
            _queue.RemoveAt(index);
            errorMessage = string.Empty;
            return true;
        }

        public bool Play(out string errorMessage)
        {
            if (_player == null)
            {
                if (_queue.Count == 0)
                {
                    errorMessage = "Error: Queue is empty";
                    return false;
                }
                if (_currentIndex < 0 || _currentIndex >= _queue.Count)
                {
                    _currentIndex = 0;
                }
                PlayTrack(_queue[_currentIndex]);
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                _player.Play();
                _isPlaying = true;
                errorMessage = string.Empty;
                return true;
            }
        }

        public bool Pause(out string errorMessage)
        {
            if (_player == null)
            {
                errorMessage = "Error: Track is not playing";
                return false;
            }
            _player.Pause();
            _isPlaying = false;
            errorMessage = string.Empty;
            return true;
        }

        public bool NextTrack(out string errorMessage)
        {
            if (_queue.Count == 0)
            {
                errorMessage = "Error: Queue is empty";
                return false;
            }
            if (_currentIndex + 1 >= _queue.Count)
            {
                errorMessage = "Error: No next track";
                return false;
            }
            if (_player != null)
            {
                _player.PlaybackStopped -= OnPlaybackStopped;
                _player.Stop();
                _player.Dispose();
                _audioFile?.Dispose();
                _player = null;
            }
            _currentIndex++;
            PlayTrack(_queue[_currentIndex]);
            errorMessage = string.Empty;
            return true;
        }

        public bool PreviousTrack(out string errorMessage)
        {
            if (_queue.Count == 0)
            {
                errorMessage = "Error: Queue is empty";
                return false;
            }
            if (_currentIndex - 1 < 0)
            {
                errorMessage = "Error: No previous track";
                return false;
            }
            if (_player != null)
            {
                _player.PlaybackStopped -= OnPlaybackStopped;
                _player.Stop();
                _player.Dispose();
                _audioFile?.Dispose();
                _player = null;
            }
            _currentIndex--;
            PlayTrack(_queue[_currentIndex]);
            errorMessage = string.Empty;
            return true;
        }

        private void PlayTrack(Track track)
        {
            _audioFile?.Dispose();
            _player?.Dispose();
            _audioFile = new AudioFileReader(track.FullPath);
            _player = new WaveOutEvent();
            _player.Init(_audioFile);
            _player.PlaybackStopped += OnPlaybackStopped;
            _player.Play();
            _isPlaying = true;
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            _audioFile?.Dispose();
            _audioFile = null;
            _player?.Dispose();
            _player = null;
            if (e.Exception != null)
            {
                _isPlaying = false;
                return;
            }
            _currentIndex++;
            if (_currentIndex < _queue.Count)
            {
                PlayTrack(_queue[_currentIndex]);
            }
            else
            {
                _isPlaying = false;
                _currentIndex = -1;
            }
        }

        public List<Track> GetQueue()
        {
            return new List<Track>(_queue);
        }

        public bool GetCurrentTrackIndex(out int currentIndex, out string errorMessage)
        {
            if (_currentIndex >= 0 && _currentIndex < _queue.Count)
            {
                currentIndex = _currentIndex;
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                currentIndex = -1;
                errorMessage = "Error: No current track";
                return false;
            }
        }

        public bool Shuffle(out string errorMessage)
        {
            if (_queue.Count == 0)
            {
                errorMessage = "Error: Queue is empty";
                return false;
            }
            Random rnd = new Random();
            if (_currentIndex >= 0 && _currentIndex < _queue.Count)
            {
                Track current = _queue[_currentIndex];
                _queue.RemoveAt(_currentIndex);
                _queue = _queue.OrderBy(x => rnd.Next()).ToList();
                _queue.Insert(0, current);
                _currentIndex = 0;
            }
            else
            {
                _queue = _queue.OrderBy(x => rnd.Next()).ToList();
            }
            errorMessage = string.Empty;
            return true;
        }

        public bool ClearQueue(out string errorMessage)
        {
            if (_player != null)
            {
                _player.PlaybackStopped -= OnPlaybackStopped;
                _player.Stop();
                _player.Dispose();
                _audioFile?.Dispose();
                _player = null;
            }
            _queue.Clear();
            _currentIndex = -1;
            errorMessage = string.Empty;
            return true;
        }
    }

    public class MusicPlayer
    {
        private List<Track> _buffer { get; } = new List<Track>();
        private PlaybackQueue _queue { get; } = new PlaybackQueue();

        public bool LoadSongs(string? path, out string[] songs, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine("C:", "Users", "Matvey", "Desktop");
            }
            if (!Directory.Exists(path))
            {
                _buffer.Clear();
                errorMessage = $"Error: Directory '{path}' does not exist";
                songs = Array.Empty<string>();
                return false;
            }
            var files = Directory.GetFiles(path, "*.mp3");
            foreach (var file in files)
            {
                if (!_buffer.Any(t => t.FullPath == file))
                {
                    _buffer.Add(new Track(file));
                }
            }
            if (_buffer.Count == 0)
            {
                errorMessage = "Error: Directory contains no mp3 files";
                songs = Array.Empty<string>();
                return false;
            }
            songs = _buffer.Select(t => t.FullPath).ToArray();
            errorMessage = string.Empty;
            return true;
        }

        public bool AddTrackToQueue(int[] indices, out string errorMessage)
        {
            foreach (var index in indices)
            {
                if (index < 0 || index >= _buffer.Count)
                {
                    errorMessage = "Error: Wrong index";
                    return false;
                }
            }
            foreach (var index in indices)
            {
                _queue.AddTrack(_buffer[index], out string _);
            }
            errorMessage = string.Empty;
            return true;
        }

        public bool RemoveTrackFromQueue(int[] indices, out string errorMessage)
        {
            var sortedIndices = indices.OrderByDescending(i => i).ToArray();
            foreach (var index in sortedIndices)
            {
                if (!_queue.RemoveTrack(index, out string err))
                {
                    errorMessage = err;
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }

        public bool Play(out string errorMessage)
        {
            return _queue.Play(out errorMessage);
        }

        public bool Pause(out string errorMessage)
        {
            return _queue.Pause(out errorMessage);
        }

        public bool NextTrack(out string errorMessage)
        {
            return _queue.NextTrack(out errorMessage);
        }

        public bool PreviousTrack(out string errorMessage)
        {
            return _queue.PreviousTrack(out errorMessage);
        }

        public List<Track> GetBuffer()
        {
            return new List<Track>(_buffer);
        }

        public List<Track> GetQueue()
        {
            return _queue.GetQueue();
        }

        public bool GetCurrentTrackIndex(out int currentIndex, out string errorMessage)
        {
            return _queue.GetCurrentTrackIndex(out currentIndex, out errorMessage);
        }

        public bool ClearBuffer(out string errorMessage)
        {
            _buffer.Clear();
            errorMessage = string.Empty;
            return true;
        }

        public bool ShuffleQueue(out string errorMessage)
        {
            return _queue.Shuffle(out errorMessage);
        }

        public bool ClearQueue(out string errorMessage)
        {
            return _queue.ClearQueue(out errorMessage);
        }
    }
}

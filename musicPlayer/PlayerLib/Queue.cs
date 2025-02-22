using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerLib
{
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

        public bool GetCurrentTrack(out Track currentTrack, out string errorMessage)
        {
            if (_currentIndex >= 0 && _currentIndex < _queue.Count)
            {
                currentTrack = _queue[_currentIndex];
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                currentTrack = default;
                errorMessage = "Error: No current track";
                return false;
            }
        }

        public bool Seek(TimeSpan newPosition, out string errorMessage)
        {
            if (_audioFile == null)
            {
                errorMessage = "Error: No track is playing";
                return false;
            }
            if (newPosition < TimeSpan.Zero || newPosition > _audioFile.TotalTime)
            {
                errorMessage = "Error: Position out of range";
                return false;
            }
            _audioFile.CurrentTime = newPosition;
            errorMessage = string.Empty;
            return true;
        }

        public bool FastForward(TimeSpan offset, out string errorMessage)
        {
            if (_audioFile == null)
            {
                errorMessage = "Error: No track is playing";
                return false;
            }
            TimeSpan newTime = _audioFile.CurrentTime + offset;
            if (newTime > _audioFile.TotalTime)
            {
                newTime = _audioFile.TotalTime;
            }
            _audioFile.CurrentTime = newTime;
            errorMessage = string.Empty;
            return true;
        }

        public bool Rewind(TimeSpan offset, out string errorMessage)
        {
            if (_audioFile == null)
            {
                errorMessage = "Error: No track is playing";
                return false;
            }
            TimeSpan newTime = _audioFile.CurrentTime - offset;
            if (newTime < TimeSpan.Zero)
            {
                newTime = TimeSpan.Zero;
            }
            _audioFile.CurrentTime = newTime;
            errorMessage = string.Empty;
            return true;
        }
        public bool GetCurrentTime(out TimeSpan currentTime, out string errorMessage)
        {
            if (_audioFile != null)
            {
                currentTime = _audioFile.CurrentTime;
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                currentTime = TimeSpan.Zero;
                errorMessage = "Error: No track is playing";
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
}

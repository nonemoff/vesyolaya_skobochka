using System;
using System.IO;
using NAudio.Wave;
using System.Linq;
using System.Collections.Generic;

namespace MusicPlayerLib
{

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

        public bool SeekTrack(TimeSpan newPosition, out string errorMessage)
        {
            return _queue.Seek(newPosition, out errorMessage);
        }

        public bool FastForwardTrack(TimeSpan offset, out string errorMessage)
        {
            return _queue.FastForward(offset, out errorMessage);
        }

        public bool RewindTrack(TimeSpan offset, out string errorMessage)
        {
            return _queue.Rewind(offset, out errorMessage);
        }

        public bool GetCurrentTrack(out Track currentTrack, out string errorMessage)
        {
            return _queue.GetCurrentTrack(out currentTrack, out errorMessage);
        }
        public bool GetCurrentTime(out TimeSpan currentTime, out string errorMessage)
        {
            return _queue.GetCurrentTime(out currentTime, out errorMessage);
        }
    }
}

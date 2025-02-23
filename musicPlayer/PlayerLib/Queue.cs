using System;
using System.Collections.Generic;
using System.Reflection;

namespace MusicPlayerLib
{
    public class Queue
    {
        private List<Track> _queue = new List<Track>();
        private int _currentTrackIndex = -1;

        private bool ValidateIndex(int index)
        {
            return index >= 0 && index < _queue.Count;
        }

        public List<Track> GetTracks()
        {
            return new List<Track>(_queue);
        }

        public Track GetCurrentTrack()
        {
            return _queue[_currentTrackIndex];
        }

        public int GetCurrentTrackIndex()
        {
            return _currentTrackIndex;
        }

        public void NextTrack()
        {
            if (!ValidateIndex(_currentTrackIndex + 1))
            {
                throw new ArgumentOutOfRangeException(nameof(_currentTrackIndex), "There is no next track.");
            }
            _currentTrackIndex++;
        }

        public void PrevTrack()
        {
            if (!ValidateIndex(_currentTrackIndex - 1))
            {
                throw new ArgumentOutOfRangeException(nameof(_currentTrackIndex), "There is no previous track.");
            }
            _currentTrackIndex--;
        }

        public void AddTrack(Track newTrack)
        {
            _queue.Add(newTrack);
        }

        public void RemoveTrack(int index)
        {
            if (!ValidateIndex(index))
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of the valid range.");
            }
            _queue.RemoveAt(index);
        }

        public void Clear()
        {
            _queue.Clear();
            _currentTrackIndex = -1;
        }

        public void Shuffle()
        {
            if (_queue.Count < 2)
            {
                return;
            }

            var originalOrder = new List<Track>(_queue);
            var currentTrack = _queue[_currentTrackIndex];
            var rnd = new Random();

            List<Track> newOrder;

            do
            {
                var remainingTracks = new List<Track>(_queue);
                remainingTracks.Remove(currentTrack);

                for (int i = remainingTracks.Count - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    var temp = remainingTracks[i];
                    remainingTracks[i] = remainingTracks[j];
                    remainingTracks[j] = temp;
                }

                newOrder = new List<Track> { currentTrack };
                newOrder.AddRange(remainingTracks);
            } while (originalOrder.SequenceEqual(newOrder));

            _queue = newOrder;
            _currentTrackIndex = 0;
        }
    }
}

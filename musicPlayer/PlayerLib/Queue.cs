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
            if (_queue.Count == 1)
            {
                _currentTrackIndex = 0;
            }
        }

        public void RemoveTracksByIndices(int[] indices)
        {
            var invalidIndices = indices.Where(i => !ValidateIndex(i)).ToArray();
            if (invalidIndices.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(indices), $"Invalid indices: {string.Join(", ", invalidIndices)}. Valid range is 0 - {_queue.Count - 1}.");
            }


            var sortedIndices = indices.OrderByDescending(i => i).ToArray();
            foreach (int index in sortedIndices)
            {
                _queue.RemoveAt(index);

                if (_queue.Count == 0)
                {
                    _currentTrackIndex = -1;
                    return;
                }
                else if (index < _currentTrackIndex)
                {
                    _currentTrackIndex--;
                }
                else if (index == _currentTrackIndex && _currentTrackIndex >= _queue.Count)
                {
                    _currentTrackIndex = 0;
                }
            }
        }


        public void Clear()
        {
            _queue.Clear();
            _currentTrackIndex = -1;
        }

        public void Shuffle()
        {
            if (_queue.Count < 2)
                return;

            var originalOrder = _queue.ToList();
            var rnd = new Random();

            if (!ValidateIndex(_currentTrackIndex))
            {
                _queue = originalOrder.OrderBy(x => rnd.Next()).ToList();
                _currentTrackIndex = 0;
                return;
            }

            Track currentTrack = _queue[_currentTrackIndex];
            List<Track> newOrder;

            do
            {
                var remainingTracks = _queue.Where(t => t != currentTrack)
                                            .OrderBy(x => rnd.Next())
                                            .ToList();
                newOrder = new List<Track> { currentTrack };
                newOrder.AddRange(remainingTracks);
            }
            while (originalOrder.SequenceEqual(newOrder));

            _queue = newOrder;
            _currentTrackIndex = 0;
        }
    }
}

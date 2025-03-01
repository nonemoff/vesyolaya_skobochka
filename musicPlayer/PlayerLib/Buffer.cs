using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicPlayerLib
{
    public class Buffer
    {
        private List<Track> _buffer = new List<Track>();

        private bool ValidateIndex(int index)
        {
            return index >= 0 && index < _buffer.Count;
        }
        private bool IsDuplicate(Track t1, Track t2)
        {
            int matchCount = 0;
            if (t1.Duration == t2.Duration)
            {
                matchCount++;
            }
            if (string.Equals(t1.Artist, t2.Artist, StringComparison.OrdinalIgnoreCase))
            {
                matchCount++;
            }
            if (string.Equals(t1.Title, t2.Title, StringComparison.OrdinalIgnoreCase))
            {
                matchCount++;
            }
            return matchCount >= 2;
        }

        public void Add(Track track)
        {
            if (_buffer.Any(existing => IsDuplicate(existing, track)))
            {
                return;
            }
            _buffer.Add(track);
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        public List<Track> GetTracks()
        {
            return new List<Track>(_buffer);
        }

        public List<Track> GetTracksByIndices(int[] indices)
        {
            var invalidIndices = indices.Where(i => !ValidateIndex(i)).ToArray();
            if (invalidIndices.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(indices), $"Invalid indices: {string.Join(", ", invalidIndices)}. Valid range is 0 - {_buffer.Count - 1}.");
            }
            List<Track> selectedTracks = new List<Track>();
            foreach (int index in indices)
            {
                selectedTracks.Add(_buffer[index]);
            }
            return selectedTracks;
        }
    }
}

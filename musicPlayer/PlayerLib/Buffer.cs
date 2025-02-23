using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerLib
{
    public class Buffer
    {
        private List<Track> _buffer = new List<Track>();

        private bool ValidateIndex(int index)
        {
            return index >= 0 && index < _buffer.Count;
        }

        public void Add(Track track)
        {
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
            List<Track> selectedTracks = new List<Track>();
            foreach (int index in indices)
            {
                if (!ValidateIndex(index))
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of the valid range for the buffer.");
                }
                selectedTracks.Add(_buffer[index]);
            }
            return selectedTracks;
        }
    }
}

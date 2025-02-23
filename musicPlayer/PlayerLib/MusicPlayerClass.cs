using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MusicPlayerLib
{

    public class MusicPlayer
    {
        private Buffer _buffer = new Buffer();
        private Queue _queue = new Queue();
        private string _defaultPath = Path.Combine("C:", "Users", "Matvey", "Desktop");

        public void LoadSongs(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _defaultPath;
            }

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist.");
            }

            var files = Directory.GetFiles(path, "*.mp3");
            foreach (var file in files)
            {
                if (!_buffer.GetTracks().Any(t => t.FullPath == file))
                {
                    _buffer.Add(new Track(file));
                }
            }

            if (_buffer.GetTracks().Count == 0)
            {
                throw new InvalidOperationException("Directory contains no mp3 files.");
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
    }
}

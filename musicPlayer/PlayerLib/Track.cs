using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayerLib
{
    public class Track
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
}

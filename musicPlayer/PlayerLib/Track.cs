using System;
using System.Diagnostics;
using System.IO;

namespace MusicPlayerLib
{
    public class Track
    {
        public string FullPath { get; }
        public string FileName { get; }
        public TimeSpan Duration { get; }
        public string Artist { get; }
        public string Title { get; }

        public Track(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw new ArgumentException("The full path cannot be null or empty.", nameof(fullPath));
            }
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File '{fullPath}' not found.", fullPath);
            }

            FullPath = fullPath;
            try
            {
                var tagFile = TagLib.File.Create(fullPath);

                Duration = tagFile.Properties.Duration;

                if (!string.IsNullOrWhiteSpace(tagFile.Tag.FirstPerformer))
                {
                    Artist = tagFile.Tag.FirstPerformer;
                }
                else
                {
                    Artist = "Unknown Artist";
                }

                if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title))
                {
                    Title = tagFile.Tag.Title;
                }
                else
                {
                    Title = Path.GetFileNameWithoutExtension(fullPath);
                }
                FileName = Title;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load track '{fullPath}'.", ex);
            }
        }
    }
}

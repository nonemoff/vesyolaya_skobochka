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

    public class MusicPlayer
    {
        private WaveOutEvent? _player;
        private AudioFileReader? _audioFile;
        private Track? _currentTrack;
        private bool _isPlaying;
        private List<Track> _songBuffer = new List<Track>();

        public bool LoadSongs(string? path, out string[] songs, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine("C:", "Users", "Matvey", "Desktop");
            }

            if (!Directory.Exists(path))
            {
                _songBuffer.Clear();
                errorMessage = $"Error: Folder '{path}' does not exist";
                songs = Array.Empty<string>();
                return false;
            }

            var files = Directory.GetFiles(path, "*.mp3");
            foreach (var file in files)
            {
                if (!_songBuffer.Any(t => t.FullPath == file))
                {
                    _songBuffer.Add(new Track(file));
                }
            }

            if (_songBuffer.Count == 0)
            {
                errorMessage = "No mp3 files in folder";
                songs = Array.Empty<string>();
                return false;
            }

            songs = _songBuffer.Select(t => t.FullPath).ToArray();
            errorMessage = string.Empty;
            return true;
        }

        public List<Track> GetLoadedSongs()
        {
            return new List<Track>(_songBuffer);
        }

        public bool SelectTrack(int index, out string errorMessage)
        {
            if (index < 0 || index >= _songBuffer.Count)
            {
                errorMessage = "Error: Invalid track index";
                return false;
            }

            _currentTrack = _songBuffer[index];
            _audioFile = new AudioFileReader(_currentTrack.Value.FullPath);
            _player = new WaveOutEvent();
            _player.Init(_audioFile);
            errorMessage = string.Empty;
            return true;
        }

        public bool Play(out string errorMessage)
        {
            if (_player == null || _audioFile == null)
            {
                errorMessage = "Error: No track selected";
                return false;
            }

            _player.Play();
            _isPlaying = true;
            errorMessage = string.Empty;
            return true;
        }

        public bool Pause(out string errorMessage)
        {
            if (!_isPlaying || _player == null)
            {
                errorMessage = "Error: No track is playing";
                return false;
            }

            _player.Pause();
            _isPlaying = false;
            errorMessage = string.Empty;
            return true;
        }
    }
}

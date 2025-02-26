﻿using MusicPlayerLib;
using System;
using System.Linq;
using System.Collections.Generic;

namespace musicPlayerCLI
{
    internal class PlayerCLI
    {
        private MusicPlayer player;
        public PlayerCLI()
        {
            player = new MusicPlayer();
        }
        public void Run()
        {
            while (true)
            {
                Console.Write("\nChoose option: ");
                string? command = Console.ReadLine()?.Trim().ToLower();
                switch (command)
                {
                    case "help":
                    case "h":
                        Help();
                        break;
                    case "load":
                    case "l":
                        LoadSongs();
                        break;
                    case "select":
                    case "s":
                        SelectTracks();
                        break;
                    case "remove":
                    case "r":
                        RemoveTracks();
                        break;
                    case "play":
                    case "p":
                        PlayTrack();
                        break;
                    case "pause":
                    case "pa":
                        PauseTrack();
                        break;
                    case "next":
                    case "n":
                        NextTrack();
                        break;
                    case "prev":
                    case "pr":
                        PreviousTrack();
                        break;
                    case "buffer":
                    case "b":
                        ShowBuffer();
                        break;
                    case "queue":
                    case "q":
                        ShowQueue();
                        break;
                    case "shuffle":
                    case "sh":
                        ShuffleQueue();
                        break;
                    case "clearq":
                    case "cq":
                        ClearQueue();
                        break;
                    case "clearb":
                    case "cb":
                        ClearBuffer();
                        break;
                    case "seek":
                    case "sk":
                        SeekTrack();
                        break;
                    case "exit":
                    case "e":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Unknown option. Print help to see list of commands");
                        break;
                }
            }
        }
        private void Help()
        {
            string[] options = new string[]
            {
                "help or h - print help",
                "load or l - load tracks from directory",
                "select or s - add track(s) to queue",
                "remove or r - remove track(s) from queue",
                "play or p - play the queue",
                "pause or pa - pause playback",
                "next or n - play next track",
                "prev or pr - play previous track",
                "buffer or b - show loaded tracks",
                "queue or q - show queue",
                "shuffle or sh - shuffle tracks in queue",
                "clearq or cq - clear queue",
                "clearb or cb - clear buffer with loaded songs",
                "seek or sk - seek track. Format:\n  if time starts with '-' then rewind\n  '+' then fast forward\n  otherwise jump to specified time",
                "exit or e - exit the program"
            };
            foreach (string option in options)
            {
                Console.WriteLine(option);
            }
        }
        private void LoadSongs()
        {
            Console.WriteLine("Input path to the music folder (enter for default):");
            string? path = Console.ReadLine()?.Trim();
            if (player.LoadSongs(path, out string[] songs, out string errorMessage))
            {
                Console.WriteLine("Loaded songs:");
                foreach (var song in songs)
                {
                    Console.WriteLine(song);
                }
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void SelectTracks()
        {
            var buffer = player.GetBuffer();
            if (buffer.Count == 0)
            {
                Console.WriteLine("No songs loaded.");
                return;
            }
            Console.WriteLine("Loaded songs:");
            for (int i = 0; i < buffer.Count; i++)
            {
                Console.WriteLine($"{i}: {buffer[i].FileName} ({buffer[i].Duration.Minutes:D2}:{buffer[i].Duration.Seconds:D2})");
            }
            Console.WriteLine("Input track indices to add to queue (separated by commas):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Error: Wrong input.");
                return;
            }
            var parts = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> indices = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out int idx))
                {
                    indices.Add(idx);
                }
                else
                {
                    Console.WriteLine("Error: Invalid index format.");
                    return;
                }
            }
            if (player.AddTrackToQueue(indices.ToArray(), out string errorMessage))
            {
                Console.WriteLine("Track(s) added to queue.");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void RemoveTracks()
        {
            Console.WriteLine("Input track indices to remove from queue (separated by commas):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Error: Wrong input.");
                return;
            }
            var parts = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<int> indices = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out int idx))
                {
                    indices.Add(idx);
                }
                else
                {
                    Console.WriteLine("Error: Invalid index format.");
                    return;
                }
            }
            if (player.RemoveTrackFromQueue(indices.ToArray(), out string errorMessage))
            {
                Console.WriteLine("Track(s) removed from queue.");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void PlayTrack()
        {
            if (player.Play(out string errorMessage))
            {
                Console.WriteLine("Playing track...");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void PauseTrack()
        {
            if (player.Pause(out string errorMessage))
            {
                Console.WriteLine("Playback paused.");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void NextTrack()
        {
            if (player.NextTrack(out string errorMessage))
            {
                Console.WriteLine("Playing next track...");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void PreviousTrack()
        {
            if (player.PreviousTrack(out string errorMessage))
            {
                Console.WriteLine("Playing previous track...");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private void ShowBuffer()
        {
            var buffer = player.GetBuffer();
            if (buffer.Count == 0)
            {
                Console.WriteLine("Buffer is empty.");
                return;
            }
            Console.WriteLine("Loaded tracks:");
            for (int i = 0; i < buffer.Count; i++)
            {
                Console.WriteLine($"{i}: {buffer[i].FileName} ({buffer[i].Duration.Minutes:D2}:{buffer[i].Duration.Seconds:D2})");
            }
        }
        private void ShowQueue()
        {
            var queue = player.GetQueue();
            if (queue.Count == 0)
            {
                Console.WriteLine("Queue is empty.");
                return;
            }
            int currentQueueIndex = -1;
            if (player.GetCurrentTrackIndex(out currentQueueIndex, out string err) && currentQueueIndex >= 0)
            {
                Console.WriteLine("Queue tracks:");
                for (int i = 0; i < queue.Count; i++)
                {
                    if (i == currentQueueIndex)
                        Console.WriteLine($"{i}: {queue[i].FileName} ({queue[i].Duration.Minutes:D2}:{queue[i].Duration.Seconds:D2}) [Playing]");
                    else
                        Console.WriteLine($"{i}: {queue[i].FileName} ({queue[i].Duration.Minutes:D2}:{queue[i].Duration.Seconds:D2})");
                }
            }
            else
            {
                Console.WriteLine("Queue tracks:");
                for (int i = 0; i < queue.Count; i++)
                {
                    Console.WriteLine($"{i}: {queue[i].FileName} ({queue[i].Duration.Minutes:D2}:{queue[i].Duration.Seconds:D2})");
                }
            }
        }
        private void ShuffleQueue()
        {
            if (player.ShuffleQueue(out string errorMessage))
                Console.WriteLine("Queue shuffled.");
            else
                Console.WriteLine(errorMessage);
        }
        private void ClearQueue()
        {
            if (player.ClearQueue(out string errorMessage))
                Console.WriteLine("Queue cleared.");
            else
                Console.WriteLine(errorMessage);
        }
        private void ClearBuffer()
        {
            if (player.ClearBuffer(out string errorMessage))
                Console.WriteLine("Buffer cleared.");
            else
                Console.WriteLine(errorMessage);
        }
        private void SeekTrack()
        {
            if (!player.Pause(out string pauseError))
            {
                Console.WriteLine("Error pausing track: " + pauseError);
                return;
            }

            if (!player.GetCurrentTime(out TimeSpan currentTime, out string timeError))
            {
                Console.WriteLine(timeError);
            }
            else if (!player.GetCurrentTrack(out Track currentTrack, out string trackError))
            {
                Console.WriteLine(trackError);
            }
            else
            {
                Console.WriteLine($"Current track: {currentTrack.FileName}");
                Console.WriteLine($"Current time: {currentTime} / Duration: {currentTrack.Duration}");
            }

            Console.WriteLine("Enter time for seeking in seconds:");
            Console.WriteLine("  If prefixed with '-' then rewind");
            Console.WriteLine("  If prefixed with '+' then fast forward");
            Console.WriteLine("  Otherwise, jump to specified time");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Error: Empty input.");
                return;
            }

            try
            {
                bool seekResult = false;
                string seekMessage = string.Empty;

                if (input.StartsWith("-"))
                {
                    string timeStr = input.Substring(1);
                    if (!double.TryParse(timeStr, out double seconds))
                    {
                        Console.WriteLine("Error: Invalid time format.");
                        return;
                    }
                    TimeSpan offset = TimeSpan.FromSeconds(seconds);
                    seekResult = player.RewindTrack(offset, out string errorMessage);
                    seekMessage = seekResult ? $"Rewound by {offset}" : errorMessage;
                }
                else if (input.StartsWith("+"))
                {
                    string timeStr = input.Substring(1);
                    if (!double.TryParse(timeStr, out double seconds))
                    {
                        Console.WriteLine("Error: Invalid time format.");
                        return;
                    }
                    TimeSpan offset = TimeSpan.FromSeconds(seconds);
                    seekResult = player.FastForwardTrack(offset, out string errorMessage);
                    seekMessage = seekResult ? $"Fast forwarded by {offset}" : errorMessage;
                }
                else
                {
                    if (!double.TryParse(input, out double seconds))
                    {
                        Console.WriteLine("Error: Invalid time format.");
                        return;
                    }
                    TimeSpan position = TimeSpan.FromSeconds(seconds);
                    seekResult = player.SeekTrack(position, out string errorMessage);
                    seekMessage = seekResult ? $"Jumped to {position}" : errorMessage;
                }

                Console.WriteLine(seekMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing time: " + ex.Message);
            }

            if (!player.Play(out string playError))
            {
                Console.WriteLine("Error resuming playback: " + playError);
            }
        }
    }
}
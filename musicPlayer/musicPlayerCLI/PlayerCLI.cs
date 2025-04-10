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
                Console.Write("\nEnter command: ");
                string? inputLine = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(inputLine))
                    continue;

                string[] tokens = inputLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = tokens[0].ToLower();
                string[] args = tokens.Skip(1).ToArray();

                try
                {
                    switch (command)
                    {
                        case "help":
                        case "h":
                            ShowHelp();
                            break;
                        case "load":
                        case "l":
                            LoadTracks(args);
                            break;
                        case "buffer":
                        case "b":
                            ShowBuffer();
                            break;
                        case "queue":
                        case "q":
                            ShowQueue();
                            break;
                        case "add":
                        case "a":
                            AddTracksToQueue(args);
                            break;
                        case "remove":
                        case "r":
                            RemoveTracksFromQueue(args);
                            break;
                        case "clearb":
                        case "cb":
                            ClearBuffer();
                            break;
                        case "clearq":
                        case "cq":
                            ClearQueue();
                            break;
                        case "shuffle":
                        case "sh":
                            ShuffleQueue();
                            break;
                        case "play":
                        case "pl":
                            Play();
                            break;
                        case "pause":
                        case "pa":
                            Pause();
                            break;
                        case "next":
                        case "n":
                            Next();
                            break;
                        case "prev":
                        case "pr":
                            Prev();
                            break;
                        case "seek":
                        case "sk":
                            Seek(args);
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private void ShowHelp()
        {
            string[] options = new string[]
            {
                "-- General --",
                "help    or h             - print help",
                "exit    or e             - exit the program",
                "\n-- Tracks Management --",
                "load    or l [dir]       - load tracks from directory (optional argument: directory path)",
                "buffer  or b             - show loaded tracks (buffer)",
                "queue   or q             - display queued tracks",
                "add     or a [i1,i2,...] - add track(s) to queue by indices (arguments separated by space or comma)",
                "remove  or r [i1,i2,...] - remove track(s) from queue by indices",
                "clearb  or cb            - clear the buffer",
                "clearq  or cq            - clear the queue",
                "shuffle or sh            - shuffle the queue",
                "\n-- Playback Control --",
                "play    or pl            - play/resume current track",
                "pause   or pa            - pause current track",
                "next    or n             - play next track",
                "prev    or pr            - play previous track",
                "seek    or sk [time]     - seek track to specified position (e.g., +10, -5, 30)",
                "                         - using + adds time to current position",
                "                         - using - subtracts time from current position",
                "                         - no sign sets track to specified position"
            };
            foreach (string option in options)
            {
                Console.WriteLine(option);
            }
        }

        private void LoadTracks(string[] args)
        {
            string? path;
            if (args.Length > 0)
            {
                path = args[0];
            }
            else
            {
                Console.Write("Enter directory path (or leave empty for default): ");
                path = Console.ReadLine();
            }
            player.LoadSongs(path);
            Console.WriteLine("Tracks loaded successfully.");
            Console.WriteLine("Buffer tracks:");
            ShowTracks(player.GetBuffer());
        }

        private void ShowTracks(List<Track> tracks)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"[{i}] {tracks[i].Artist} - {tracks[i].Title} ({tracks[i].Duration.Minutes:D2}:{tracks[i].Duration.Seconds:D2})");
            }
        }

        private void ShowBuffer()
        {
            List<Track> bufferTracks = player.GetBuffer();
            if (bufferTracks.Count == 0)
            {
                Console.WriteLine("Buffer is empty.");
                return;
            }
            Console.WriteLine("Buffer tracks:");
            ShowTracks(bufferTracks);
        }

        private void ShowQueue()
        {
            List<Track> queueTracks = player.GetQueue();
            if (queueTracks.Count == 0)
            {
                Console.WriteLine("Queue is empty.");
                return;
            }
            Console.WriteLine("Queue tracks:");
            ShowTracks(queueTracks);
        }

        private void AddTracksToQueue(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Enter track indices to add to queue (separated by spaces or commas): ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("No indices entered.");
                    return;
                }
                args = input.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            List<int> indices = new List<int>();
            foreach (string part in args)
            {
                if (int.TryParse(part, out int index))
                {
                    indices.Add(index);
                }
                else
                {
                    Console.WriteLine($"Invalid index: {part}");
                    return;
                }
            }
            player.AddTracksToQueueByIndices(indices.ToArray());
            Console.WriteLine("Selected tracks have been added to the queue.");
        }

        private void RemoveTracksFromQueue(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Enter track indices to remove from queue (separated by spaces or commas): ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("No indices entered.");
                    return;
                }
                args = input.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            List<int> indices = new List<int>();
            foreach (string part in args)
            {
                if (int.TryParse(part, out int index))
                {
                    indices.Add(index);
                }
                else
                {
                    Console.WriteLine($"Invalid index: {part}");
                    return;
                }
            }
            player.RemoveTracksFromQueueByIndices(indices.ToArray());
            Console.WriteLine("Selected tracks have been removed from the queue.");
        }

        private void ClearBuffer()
        {
            player.ClearBuffer();
            Console.WriteLine("Buffer has been cleared.");
        }

        private void ClearQueue()
        {
            player.ClearQueue();
            Console.WriteLine("Queue has been cleared.");
        }

        private void ShuffleQueue()
        {
            player.ShuffleQueue();
            Console.WriteLine("Queue has been shuffled.");
            ShowQueue();
        }

        private void Play()
        {
            player.PlayTrack();
            Console.WriteLine("Playback started/resumed.");
        }

        private void Pause()
        {
            player.PauseTrack();
            Console.WriteLine("Playback paused.");
        }

        private void Next()
        {
            player.NextTrack();
            Console.WriteLine("Switched to next track.");
        }

        private void Prev()
        {
            player.PrevTrack();
            Console.WriteLine("Switched to previous track.");
        }

        private void Seek(string[] args)
        {
            TimeSpan currentTime, totalTime;
            player.PauseTrack();
            if (args.Length == 0)
            {
                player.PauseTrack();
                currentTime = player.GetCurrentTrackTime();
                totalTime = player.GetCurrentTrackTotalDuration();
                Console.WriteLine($"Track paused at ({currentTime.Minutes:D2}:{currentTime.Seconds:D2}/{totalTime.Minutes:D2}:{totalTime.Seconds:D2}).");
                Console.Write("Enter seek time command (e.g., +10, -5, 30): ");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("No seek time entered.");
                    return;
                }
                args = new string[] { input };
            }
            player.SeekTrack(args[0]);
            player.PlayTrack();
            currentTime = player.GetCurrentTrackTime();
            totalTime = player.GetCurrentTrackTotalDuration();
            Console.WriteLine($"Track seeked at ({currentTime.Minutes:D2}:{currentTime.Seconds:D2}/{totalTime.Minutes:D2}:{totalTime.Seconds:D2}).");
        }
    }
}

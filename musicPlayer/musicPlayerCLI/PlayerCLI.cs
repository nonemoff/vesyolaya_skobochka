using MusicPlayerLib;
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
                try
                {
                    switch (command)
                    {
                        case "help":
                        case "h":
                            Help();
                            break;
                        case "load":
                        case "l":
                            Load();
                            break;
                        case "buffer":
                        case "b":
                            ShowBuffer();
                            break;
                        case "queue":
                        case "q":
                            ShowQueue();
                            break;
                        case "select":
                        case "s":
                            SelectTracks();
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
        private void Help()
        {
            string[] options = new string[]
            {
                "help or h - print help",
                "load or l - load tracks from directory",
                "buffer or b - show loaded tracks (buffer)",
                "queue or q - display queued tracks",
                "select or s - add track(s) to queue",
                "exit or e  - exit the program"
            };
            foreach (string option in options)
            {
                Console.WriteLine(option);
            }
        }
        private void Load()
        {
            Console.Write("Enter directory path (or leave empty for default): ");
            string? path = Console.ReadLine();
            try
            {
                player.LoadSongs(path);
                Console.WriteLine("Tracks loaded successfully.");
                Console.WriteLine("Buffer tracks:");
                ShowTracks(player.GetBuffer());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load tracks: {ex.Message}");
            }
        }
        private void ShowTracks(List<Track> tracks)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"[{i}] {tracks[i].Artist} - {tracks[i].FileName}");
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
        private void SelectTracks()
        {
            ShowBuffer();
            Console.Write("Enter track indices to add to queue (separated by spaces or commas): ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No indices entered.");
                return;
            }
            char[] separators = new char[] { ' ', ',' };
            string[] parts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            List<int> indices = new List<int>();
            foreach (string part in parts)
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
            try
            {
                player.AddTracksToQueueByIndices(indices.ToArray());
                Console.WriteLine("Selected tracks have been added to the queue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add tracks to queue: {ex.Message}");
            }
        }
    }
}

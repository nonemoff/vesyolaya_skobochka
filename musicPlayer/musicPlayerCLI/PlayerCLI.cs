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
                        case "add":
                        case "a":
                            AddTracksToQueue();
                            break;
                        case "remove":
                        case "r":
                            RemoveTracksFromQueue();
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
                "help    or h       - print help",
                "load    or l       - load tracks from directory",
                "buffer  or b       - show loaded tracks (buffer)",
                "queue   or q       - display queued tracks",
                "add     or a       - add track(s) to queue",
                "remove  or r       - remove track(s) from queue",
                "clearb  or cb      - clear the buffer",
                "clearq  or cq      - clear the queue",
                "shuffle or sh      - shuffle the queue",
                "exit    or e       - exit the program"
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
            player.LoadSongs(path);
            Console.WriteLine("Tracks loaded successfully.");
            Console.WriteLine("Buffer tracks:");
            ShowTracks(player.GetBuffer());
        }
        private void ShowTracks(List<Track> tracks)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                Console.WriteLine($"[{i}] {tracks[i].Artist} - {tracks[i].Title}");
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
        private void AddTracksToQueue()
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
            player.AddTracksToQueueByIndices(indices.ToArray());
            Console.WriteLine("Selected tracks have been added to the queue.");
        }
        private void RemoveTracksFromQueue()
        {
            ShowQueue();
            Console.Write("Enter track indices to remove from queue (separated by spaces or commas): ");
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
    }
}

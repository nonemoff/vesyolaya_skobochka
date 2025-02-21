using MusicPlayerLib;

namespace musicPlayerCLI
{
    internal class playerCLI
    {
        private MusicPlayer player;
        public playerCLI()
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
                        SelectTrack();
                        break;
                    case "play":
                    case "p":
                        PlayTrack();
                        break;
                    case "pause":
                    case "pa":
                        PauseTrack();
                        break;
                    case "quit":
                    case "q":
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
                "load or l - loaded tracks",
                "select or s - select a track",
                "play or p - play the selected track",
                "pause or pa - pause the playing track",
                "quit or q - quit the program"
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

        private void SelectTrack()
        {
            var songs = player.GetLoadedSongs();

            if (songs.Count == 0)
            {
                Console.WriteLine("No songs loaded.");
                return;
            }

            Console.WriteLine("Loaded songs:");
            for (int i = 0; i < songs.Count; i++)
            {
                Console.WriteLine($"{i}: {songs[i].FileName} ({songs[i].Duration.Minutes:D2}:{songs[i].Duration.Seconds:D2})");
            }

            Console.WriteLine("Input index:");
            if (int.TryParse(Console.ReadLine()?.Trim(), out int trackIndex))
            {
                if (player.SelectTrack(trackIndex, out string errorMessage))
                {
                    Console.WriteLine("Track selected successfully.");
                }
                else
                {
                    Console.WriteLine(errorMessage);
                }
            }
            else
            {
                Console.WriteLine("Error: Invalid input. Please enter a valid track index.");
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
                Console.WriteLine("Track paused.");
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
    }
}

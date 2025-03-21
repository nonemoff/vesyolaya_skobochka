using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MoodDiary.Models;
using Microsoft.Maui.Storage;

namespace MoodDiary.Services
{
    public static class MoodDataService
    {
        // Файл данных будет сохранен в каталоге данных приложения
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "moodData.json");
        private static List<MoodEntry> entries = new List<MoodEntry>();
        private static bool isLoaded = false;

        public static async Task LoadEntriesAsync()
        {
            try
            {
                if (isLoaded) return;
                if (File.Exists(FilePath))
                {
                    using var stream = File.OpenRead(FilePath);
                    entries = await JsonSerializer.DeserializeAsync<List<MoodEntry>>(stream) ?? new List<MoodEntry>();
                }
                else
                {
                    entries = new List<MoodEntry>();
                }
                isLoaded = true;
            }
            catch (Exception ex)
            {
                // В случае ошибки можно добавить логирование
                entries = new List<MoodEntry>();
            }
        }

        public static async Task SaveEntriesAsync()
        {
            try
            {
                using var stream = File.Create(FilePath);
                await JsonSerializer.SerializeAsync(stream, entries);
            }
            catch (Exception ex)
            {
                // Обработка ошибки сохранения
            }
        }

        public static async Task AddEntryAsync(MoodEntry entry)
        {
            await LoadEntriesAsync();
            entries.Add(entry);
            await SaveEntriesAsync();
        }

        public static List<MoodEntry> GetEntries()
        {
            return entries;
        }
    }
}

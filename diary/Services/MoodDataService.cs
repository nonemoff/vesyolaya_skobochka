using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MoodDiary.Models;
using Microsoft.Maui.Storage;

namespace MoodDiary.Services
{
    public static class MoodDataService
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "moodData.json");

        private static List<MoodEntry> entries = new List<MoodEntry>();
        private static bool isLoaded = false;

        // Настройки сериализации с обрезкой миллисекунд
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task LoadEntriesAsync()
        {
            try
            {
                if (isLoaded) return;

                if (File.Exists(FilePath))
                {
                    using var stream = File.OpenRead(FilePath);
                    entries = await JsonSerializer.DeserializeAsync<List<MoodEntry>>(stream, JsonOptions)
                        ?? new List<MoodEntry>();

                    // Удалим дробную часть секунд, если вдруг была в файле
                    foreach (var entry in entries)
                        entry.Timestamp = entry.Timestamp.AddTicks(-(entry.Timestamp.Ticks % TimeSpan.TicksPerSecond));
                }
                else
                {
                    entries = new List<MoodEntry>();
                }

                isLoaded = true;
            }
            catch (Exception ex)
            {
                // Добавь логирование при необходимости
                entries = new List<MoodEntry>();
            }
        }

        public static async Task SaveEntriesAsync()
        {
            try
            {
                // Гарантируем округление до секунды перед сохранением
                foreach (var entry in entries)
                    entry.Timestamp = entry.Timestamp.AddTicks(-(entry.Timestamp.Ticks % TimeSpan.TicksPerSecond));

                using var stream = File.Create(FilePath);
                await JsonSerializer.SerializeAsync(stream, entries, JsonOptions);
            }
            catch (Exception ex)
            {
                // Обработка ошибок при записи
            }
        }

        public static async Task AddEntryAsync(MoodEntry entry)
        {
            await LoadEntriesAsync();
            entry.Timestamp = entry.Timestamp.AddTicks(-(entry.Timestamp.Ticks % TimeSpan.TicksPerSecond));
            entries.Add(entry);
            await SaveEntriesAsync();
        }

        public static List<MoodEntry> GetEntries()
        {
            return entries;
        }
    }
}

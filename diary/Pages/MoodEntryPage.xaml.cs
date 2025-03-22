using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using MoodDiary.Models;
using MoodDiary.Services;

namespace MoodDiary
{
    public partial class MoodEntryPage : ContentPage
    {
        public MoodEntryPage()
        {
            InitializeComponent();
        }

        private async void OnMoodButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.CommandParameter.ToString(), out int moodIndex))
            {
                var entry = new MoodEntry
                {
                    Timestamp = DateTime.Now,
                    MoodValue = moodIndex
                };

                await MoodDataService.AddEntryAsync(entry);
                await DisplayAlert("Запись сохранена", $"Ваше настроение \"{GetMoodName(moodIndex)}\" сохранено.", "OK");
            }
        }

        private string GetMoodName(int index)
        {
            string[] moods = new string[]
            {
                "Счастлив",
                "Радостен",
                "Спокоен",
                "Нейтрален",
                "Озадачен",
                "Грустен",
                "Злюсь",
                "Разочарован",
                "Тревожен",
                "Подавлен"
            };
            if (index >= 0 && index < moods.Length)
                return moods[index];
            return "Неизвестно";
        }
    }
}

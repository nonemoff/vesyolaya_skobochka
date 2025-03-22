using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MoodDiary.Services;

namespace MoodDiary
{
    public partial class WeeklyStatisticsPage : ContentPage
    {
        public ObservableCollection<DailyMoodSummary> WeeklySummaries { get; set; } = new ObservableCollection<DailyMoodSummary>();

        public WeeklyStatisticsPage()
        {
            InitializeComponent();
            WeeklyCollectionView.ItemsSource = WeeklySummaries;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadWeeklyStatisticsAsync();
        }

        private async Task LoadWeeklyStatisticsAsync()
        {
            await MoodDataService.LoadEntriesAsync();
            WeeklySummaries.Clear();

            DateTime today = DateTime.Today;
            // Определяем начало недели (можно изменить логику, если неделя начинается не с воскресенья)
            DateTime startOfWeek = today.AddDays(-((int)today.DayOfWeek));

            var weeklyEntries = MoodDataService.GetEntries()
                .Where(x => x.Timestamp.Date >= startOfWeek && x.Timestamp.Date <= today)
                .GroupBy(x => x.Timestamp.Date)
                .OrderBy(g => g.Key);

            foreach (var group in weeklyEntries)
            {
                // Вычисляем среднее значение настроения
                // Теперь значение 0 это худшее настроение, 9 это лучшее
                double avg = group.Average(x => x.MoodValue);
                string moodDescription = GetMoodDescription(avg);
                string summary = $"Среднее настроение: {avg:F1} - {moodDescription}";
                WeeklySummaries.Add(new DailyMoodSummary { Date = group.Key, Summary = summary });
            }
        }

        private string GetMoodDescription(double moodValue)
        {
            if (moodValue >= 8.5) return "Счастливый";
            if (moodValue >= 7.5) return "Радостный";
            if (moodValue >= 6.5) return "Спокойный";
            if (moodValue >= 5.5) return "Нейтральный";
            if (moodValue >= 4.5) return "Озадаченный";
            if (moodValue >= 3.5) return "Грустный";
            if (moodValue >= 2.5) return "Злой";
            if (moodValue >= 1.5) return "Разочарованный";
            if (moodValue >= 0.5) return "Тревожный";
            return "Подавленный";
        }
    }

    public class DailyMoodSummary
    {
        public DateTime Date { get; set; }
        public string Summary { get; set; }
    }
}
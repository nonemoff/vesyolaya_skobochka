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
                // Поскольку MoodValue хранится с индексом 0-9, прибавляем 1 для удобства отображения (диапазон 1-10)
                double avg = group.Average(x => x.MoodValue) + 1;
                string summary = $"Среднее настроение: {avg:F1}";
                WeeklySummaries.Add(new DailyMoodSummary { Date = group.Key, Summary = summary });
            }
        }
    }

    public class DailyMoodSummary
    {
        public DateTime Date { get; set; }
        public string Summary { get; set; }
    }
}

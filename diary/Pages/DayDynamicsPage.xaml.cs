using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MoodDiary.Services;

namespace MoodDiary
{
    public partial class DayDynamicsPage : ContentPage
    {
        public ObservableCollection<MoodViewModel> TodayMoods { get; set; } = new ObservableCollection<MoodViewModel>();

        public DayDynamicsPage()
        {
            InitializeComponent();
            MoodCollectionView.ItemsSource = TodayMoods;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTodayMoodsAsync();
        }

        private async Task LoadTodayMoodsAsync()
        {
            await MoodDataService.LoadEntriesAsync();
            TodayMoods.Clear();
            var today = DateTime.Today;
            var entries = MoodDataService.GetEntries()
                .Where(x => x.Timestamp.Date == today)
                .OrderBy(x => x.Timestamp)
                .Select(x => new MoodViewModel
                {
                    Timestamp = x.Timestamp,
                    MoodName = GetMoodName(x.MoodValue)
                });
            foreach (var m in entries)
            {
                TodayMoods.Add(m);
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

    public class MoodViewModel
    {
        public DateTime Timestamp { get; set; }
        public string MoodName { get; set; }
    }
}

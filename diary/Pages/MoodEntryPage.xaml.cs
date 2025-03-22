using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MoodDiary.Models;
using MoodDiary.Services;
using MoodDiary.Graphics;

namespace MoodDiary
{
    public partial class MoodEntryPage : ContentPage
    {
        public ObservableCollection<MoodViewModel> TodayMoods { get; set; } = new ObservableCollection<MoodViewModel>();
        private IDispatcherTimer _timeUpdateTimer;

        public MoodEntryPage()
        {
            InitializeComponent();
            TodayMoodsCollectionView.ItemsSource = TodayMoods;

            // Настройка таймера для обновления линии текущего времени
            _timeUpdateTimer = Dispatcher.CreateTimer();
            _timeUpdateTimer.Interval = TimeSpan.FromMinutes(1); // Обновление каждую минуту
            _timeUpdateTimer.Tick += (s, e) => DrawMoodChart(); // Перерисовка графика каждый раз
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTodayMoodsAsync();
            DrawMoodChart();

            // Запуск таймера, когда страница становится видимой
            _timeUpdateTimer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Остановка таймера, когда страница исчезает
            _timeUpdateTimer.Stop();
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
                    MoodName = GetMoodName(x.MoodValue),
                    MoodValue = x.MoodValue
                });
            foreach (var m in entries)
            {
                TodayMoods.Add(m);
            }
        }

        private void DrawMoodChart()
        {
            MoodChartGrid.Children.Clear();

            if (TodayMoods.Count == 0)
            {
                var noDataLabel = new Label
                {
                    Text = "Нет данных за сегодня",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                MoodChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Create a canvas for drawing the chart
            var drawableView = new GraphicsView
            {
                Drawable = new MoodChartDrawable(TodayMoods),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            MoodChartGrid.Children.Add(drawableView);
        }

        private async void OnMoodButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.CommandParameter.ToString(), out int moodIndex))
            {
                // Создаем запись настроения с текущей датой и временем
                var entry = new MoodEntry
                {
                    Timestamp = DateTime.Now,
                    MoodValue = moodIndex  // индекс от 0 до 9
                };

                await MoodDataService.AddEntryAsync(entry);
                await LoadTodayMoodsAsync();
                DrawMoodChart();
                await DisplayAlert("Запись сохранена", $"Ваше настроение \"{GetMoodName(moodIndex)}\" сохранено.", "OK");
            }
        }

        private string GetMoodName(int index)
        {
            string[] moods = new string[]
            {
                "Подавлен",
                "Тревожен",
                "Разочарован",
                "Злюсь",
                "Грустен",
                "Озадачен",
                "Нейтрален",
                "Спокоен",
                "Радостен",
                "Счастлив"
            };
            if (index >= 0 && index < moods.Length)
                return moods[index];
            return "Неизвестно";
        }
    }
}
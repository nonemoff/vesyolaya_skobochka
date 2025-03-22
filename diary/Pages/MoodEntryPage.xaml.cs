using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
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
        // Коллекции для отображения данных
        public ObservableCollection<MoodViewModel> DayMoods { get; set; } = new ObservableCollection<MoodViewModel>();
        public ObservableCollection<WeekDayStatViewModel> WeeklyStats { get; set; } = new ObservableCollection<WeekDayStatViewModel>();

        // Таймер для обновления линии текущего времени
        private IDispatcherTimer _timeUpdateTimer;

        // Текущая дата для отображения
        private DateTime _currentDate = DateTime.Today;

        public MoodEntryPage()
        {
            InitializeComponent();
            TodayMoodsCollectionView.ItemsSource = DayMoods;
            WeeklyStatsCollectionView.ItemsSource = WeeklyStats;

            // Настройка таймера для обновления линии текущего времени
            _timeUpdateTimer = Dispatcher.CreateTimer();
            _timeUpdateTimer.Interval = TimeSpan.FromMinutes(1); // Обновление каждую минуту
            _timeUpdateTimer.Tick += (s, e) => DrawMoodChart(); // Перерисовка графика каждый раз

            UpdateDateDisplay();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();

            // Запуск таймера, когда страница становится видимой
            _timeUpdateTimer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Остановка таймера, когда страница исчезает
            _timeUpdateTimer.Stop();
        }

        private async Task LoadDataAsync()
        {
            await LoadDayMoodsAsync();
            await LoadWeeklyStatsAsync();
            DrawMoodChart();
        }

        private async Task LoadDayMoodsAsync()
        {
            await MoodDataService.LoadEntriesAsync();
            DayMoods.Clear();

            var entries = MoodDataService.GetEntries()
                .Where(x => x.Timestamp.Date == _currentDate.Date)
                .OrderBy(x => x.Timestamp)
                .Select(x => new MoodViewModel
                {
                    Timestamp = x.Timestamp,
                    MoodName = GetMoodName(x.MoodValue),
                    MoodValue = x.MoodValue
                });

            foreach (var m in entries)
            {
                DayMoods.Add(m);
            }

            // Обновляем заголовок для списка записей
            DayRecordsLabel.Text = _currentDate.Date == DateTime.Today.Date
                ? "Записи за сегодня:"
                : $"Записи за {_currentDate:dd.MM.yyyy}:";
        }

        private async Task LoadWeeklyStatsAsync()
        {
            await MoodDataService.LoadEntriesAsync();
            WeeklyStats.Clear();

            // Определяем начало и конец недели (понедельник - воскресенье)
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            // Получаем все данные за неделю
            var weekEntries = MoodDataService.GetEntries()
                .Where(x => x.Timestamp.Date >= startOfWeek && x.Timestamp.Date <= endOfWeek)
                .ToList();

            // Группируем данные по дням и вычисляем среднее настроение
            for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                var dayEntries = weekEntries.Where(x => x.Timestamp.Date == date.Date).ToList();

                if (dayEntries.Any())
                {
                    float avgMood = (float)dayEntries.Average(x => x.MoodValue);

                    WeeklyStats.Add(new WeekDayStatViewModel
                    {
                        Date = date,
                        AverageMood = avgMood,
                        EntryCount = dayEntries.Count,
                        MoodColor = GetMoodColor(avgMood)
                    });
                }
                else
                {
                    // Добавляем пустую запись для дней без настроений
                    WeeklyStats.Add(new WeekDayStatViewModel
                    {
                        Date = date,
                        AverageMood = float.NaN,
                        EntryCount = 0,
                        MoodColor = Colors.Gray
                    });
                }
            }
        }

        private void DrawMoodChart()
        {
            MoodChartGrid.Children.Clear();

            if (DayMoods.Count == 0)
            {
                var noDataLabel = new Label
                {
                    Text = _currentDate.Date == DateTime.Today.Date
                        ? "Нет данных за сегодня"
                        : $"Нет данных за {_currentDate:dd.MM.yyyy}",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                MoodChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Create a canvas for drawing the chart
            var drawableView = new GraphicsView
            {
                Drawable = new MoodChartDrawable(DayMoods, _currentDate),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            MoodChartGrid.Children.Add(drawableView);
        }

        private void UpdateDateDisplay()
        {
            // Обновляем текст на кнопке с текущей датой
            CurrentDayButton.Text = _currentDate.Date == DateTime.Today.Date
                ? $"Сегодня: {_currentDate:dd.MM.yyyy}"
                : _currentDate.ToString("dd.MM.yyyy");
        }

        // Обработчики для кнопок навигации по дням
        private async void OnPreviousDayClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(-1);
            UpdateDateDisplay();
            await LoadDayMoodsAsync();
            DrawMoodChart();
        }

        private async void OnNextDayClicked(object sender, EventArgs e)
        {
            // Не позволяем выбрать дату в будущем
            if (_currentDate.Date >= DateTime.Today.Date)
                return;

            _currentDate = _currentDate.AddDays(1);
            UpdateDateDisplay();
            await LoadDayMoodsAsync();
            DrawMoodChart();
        }

        private async void OnTodayClicked(object sender, EventArgs e)
        {
            _currentDate = DateTime.Today;
            UpdateDateDisplay();
            await LoadDayMoodsAsync();
            DrawMoodChart();
        }

        private async void OnMoodButtonClicked(object sender, EventArgs e)
        {
            // Можно добавлять настроение только сегодня
            if (_currentDate.Date != DateTime.Today.Date)
            {
                await DisplayAlert("Внимание", "Настроение можно добавить только за сегодня", "ОК");
                return;
            }

            if (sender is Button btn && int.TryParse(btn.CommandParameter.ToString(), out int moodIndex))
            {
                // Создаем запись настроения с текущей датой и временем
                var entry = new MoodEntry
                {
                    Timestamp = DateTime.Now,
                    MoodValue = moodIndex  // индекс от 0 до 9
                };

                await MoodDataService.AddEntryAsync(entry);
                await LoadDataAsync(); // Перезагружаем и дневные данные, и недельную статистику
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

        private Color GetMoodColor(float moodValue)
        {
            // Return different colors based on mood value (0 = worst mood, 9 = best mood)
            int intValue = (int)Math.Round(moodValue);
            switch (intValue)
            {
                case 0: return Colors.DarkRed;
                case 1: return Colors.Red;
                case 2: return Colors.OrangeRed;
                case 3: return Colors.Orange;
                case 4: return Colors.Yellow;
                case 5: return Colors.CornflowerBlue;
                case 6: return Colors.LightBlue;
                case 7: return Colors.LightGreen;
                case 8: return Colors.Green;
                case 9: return Colors.DarkGreen;
                default: return Colors.Gray;
            }
        }
    }

    // Модель для статистики дня недели
    public class WeekDayStatViewModel
    {
        public DateTime Date { get; set; }
        public float AverageMood { get; set; }
        public int EntryCount { get; set; }
        public Color MoodColor { get; set; }
    }
}
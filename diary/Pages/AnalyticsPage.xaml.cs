using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoodDiary.Models;
using MoodDiary.Services;
using MoodDiary.Graphics;

namespace MoodDiary
{
    public partial class AnalyticsPage : ContentPage
    {
        // Перечисление режимов просмотра
        private enum ViewMode
        {
            Week,
            Month
        }

        // Текущий режим просмотра
        private ViewMode _currentMode = ViewMode.Week;

        // Текущий период (для недель - начало недели, для месяцев - первый день месяца)
        private DateTime _currentPeriodStart;

        // Кэш данных настроения для быстрого доступа
        private List<MoodEntry> _allMoodEntries = new List<MoodEntry>();

        public AnalyticsPage()
        {
            InitializeComponent();

            // Устанавливаем текущий период по умолчанию
            SetInitialPeriod();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Загружаем данные и обновляем интерфейс
            await LoadDataAsync();
            UpdateUI();
        }

        private void SetInitialPeriod()
        {
            DateTime today = DateTime.Today;

            if (_currentMode == ViewMode.Week)
            {
                // Определяем начало текущей недели (понедельник)
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                _currentPeriodStart = today.AddDays(-diff);
            }
            else // Month mode
            {
                // Первый день текущего месяца
                _currentPeriodStart = new DateTime(today.Year, today.Month, 1);
            }

            UpdatePeriodButtonText();
        }

        private async Task LoadDataAsync()
        {
            // Загружаем все данные
            await MoodDataService.LoadEntriesAsync();
            _allMoodEntries = MoodDataService.GetEntries().ToList();
        }

        private void UpdateUI()
        {
            // Обновляем визуальное состояние кнопок режима
            UpdateModeButtonsState();

            // Обновляем данные и графики для текущего периода
            UpdatePeriodData();

            // Отрисовываем графики
            DrawTrendChart();
            DrawDistributionChart();
        }

        private void UpdateModeButtonsState()
        {
            if (_currentMode == ViewMode.Week)
            {
                WeekModeButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
                MonthModeButton.BackgroundColor = (Color)Application.Current.Resources["Gray400"];
            }
            else
            {
                WeekModeButton.BackgroundColor = (Color)Application.Current.Resources["Gray400"];
                MonthModeButton.BackgroundColor = (Color)Application.Current.Resources["Primary"];
            }
        }

        private void UpdatePeriodButtonText()
        {
            if (_currentMode == ViewMode.Week)
            {
                DateTime weekEnd = _currentPeriodStart.AddDays(6);
                CurrentPeriodButton.Text = $"Неделя: {_currentPeriodStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}";
            }
            else
            {
                CurrentPeriodButton.Text = $"Месяц: {_currentPeriodStart:MMMM yyyy}";
            }
        }

        private void UpdatePeriodData()
        {
            // Получаем данные для текущего периода
            var periodEntries = GetEntriesForCurrentPeriod();

            // Если нет данных, показываем сообщение и выходим
            if (!periodEntries.Any())
            {
                AverageMoodLabel.Text = "—";
                AmplitudeLabel.Text = "—";
                TrendLabel.Text = "—";
                EntryCountLabel.Text = "0";
                PreviousPeriodLabel.Text = "—";
                CurrentPeriodLabel.Text = "—";
                ComparisonLabel.Text = "Недостаточно данных для анализа";
                return;
            }

            // Вычисляем метрики для текущего периода
            double averageMood = periodEntries.Average(e => e.MoodValue);
            int minMood = periodEntries.Min(e => e.MoodValue);
            int maxMood = periodEntries.Max(e => e.MoodValue);
            int amplitude = maxMood - minMood;
            int entryCount = periodEntries.Count;

            // Отображаем метрики
            AverageMoodLabel.Text = $"{averageMood:F1}";
            AmplitudeLabel.Text = amplitude.ToString();
            EntryCountLabel.Text = entryCount.ToString();
            CurrentPeriodLabel.Text = $"{averageMood:F1}";

            // Вычисляем и отображаем тренд
            double trend = CalculateTrend(periodEntries);
            TrendLabel.Text = trend >= 0 ? $"+{trend:F1}" : $"{trend:F1}";
            TrendLabel.TextColor = trend >= 0 ? Colors.Green : Colors.Red;

            // Сравниваем с предыдущим периодом
            var previousPeriodEntries = GetEntriesForPreviousPeriod();
            if (previousPeriodEntries.Any())
            {
                double previousAverage = previousPeriodEntries.Average(e => e.MoodValue);
                PreviousPeriodLabel.Text = $"{previousAverage:F1}";

                // Вычисляем процент изменения
                double changePercent = (averageMood - previousAverage) / previousAverage * 100;
                string changeDirection = changePercent >= 0 ? "улучшилось" : "ухудшилось";
                ComparisonLabel.Text = $"Ваше настроение {changeDirection} на {Math.Abs(changePercent):F1}% по сравнению с предыдущим периодом";
            }
            else
            {
                PreviousPeriodLabel.Text = "—";
                ComparisonLabel.Text = "Нет данных за предыдущий период для сравнения";
            }
        }

        private List<MoodEntry> GetEntriesForCurrentPeriod()
        {
            DateTime periodEnd;

            if (_currentMode == ViewMode.Week)
            {
                // Неделя заканчивается через 7 дней от начала
                periodEnd = _currentPeriodStart.AddDays(7).AddSeconds(-1); // 23:59:59 последнего дня
            }
            else // Month mode
            {
                // Месяц заканчивается в последний день месяца
                periodEnd = _currentPeriodStart.AddMonths(1).AddSeconds(-1); // 23:59:59 последнего дня
            }

            return _allMoodEntries
                .Where(e => e.Timestamp >= _currentPeriodStart && e.Timestamp <= periodEnd)
                .ToList();
        }

        private List<MoodEntry> GetEntriesForPreviousPeriod()
        {
            DateTime previousPeriodStart;
            DateTime previousPeriodEnd;

            if (_currentMode == ViewMode.Week)
            {
                previousPeriodStart = _currentPeriodStart.AddDays(-7);
                previousPeriodEnd = _currentPeriodStart.AddSeconds(-1); // 23:59:59 последнего дня предыдущей недели
            }
            else // Month mode
            {
                previousPeriodStart = _currentPeriodStart.AddMonths(-1);
                previousPeriodEnd = _currentPeriodStart.AddSeconds(-1); // 23:59:59 последнего дня предыдущего месяца
            }

            return _allMoodEntries
                .Where(e => e.Timestamp >= previousPeriodStart && e.Timestamp <= previousPeriodEnd)
                .ToList();
        }

        private double CalculateTrend(List<MoodEntry> entries)
        {
            // Если менее 2 записей, тренд посчитать невозможно
            if (entries.Count < 2)
                return 0;

            // Простой метод вычисления тренда - разница между средним во второй и первой половине периода
            var sortedEntries = entries.OrderBy(e => e.Timestamp).ToList();
            int midPoint = sortedEntries.Count / 2;

            var firstHalf = sortedEntries.Take(midPoint).ToList();
            var secondHalf = sortedEntries.Skip(midPoint).ToList();

            double firstHalfAvg = firstHalf.Average(e => e.MoodValue);
            double secondHalfAvg = secondHalf.Average(e => e.MoodValue);

            return Math.Round(secondHalfAvg - firstHalfAvg, 1);
        }

        private void DrawTrendChart()
        {
            TrendChartGrid.Children.Clear();

            var periodEntries = GetEntriesForCurrentPeriod();
            if (!periodEntries.Any())
            {
                var noDataLabel = new Label
                {
                    Text = "Нет данных за выбранный период",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                TrendChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Создаем график тренда
            var trendChart = new GraphicsView
            {
                Drawable = new TrendChartDrawable(periodEntries, _currentMode == ViewMode.Week),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            TrendChartGrid.Children.Add(trendChart);
        }

        private void DrawDistributionChart()
        {
            DistributionChartGrid.Children.Clear();

            var periodEntries = GetEntriesForCurrentPeriod();
            if (!periodEntries.Any())
            {
                var noDataLabel = new Label
                {
                    Text = "Нет данных за выбранный период",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                DistributionChartGrid.Children.Add(noDataLabel);
                return;
            }

            // Создаем график распределения настроения
            var distributionChart = new GraphicsView
            {
                Drawable = new DistributionChartDrawable(periodEntries),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            DistributionChartGrid.Children.Add(distributionChart);
        }

        // Обработчики событий для кнопок переключения режима
        private void OnWeekModeClicked(object sender, EventArgs e)
        {
            if (_currentMode != ViewMode.Week)
            {
                _currentMode = ViewMode.Week;
                SetInitialPeriod();
                UpdateUI();
            }
        }

        private void OnMonthModeClicked(object sender, EventArgs e)
        {
            if (_currentMode != ViewMode.Month)
            {
                _currentMode = ViewMode.Month;
                SetInitialPeriod();
                UpdateUI();
            }
        }

        // Обработчики событий для кнопок навигации по периодам
        private void OnPreviousPeriodClicked(object sender, EventArgs e)
        {
            if (_currentMode == ViewMode.Week)
            {
                _currentPeriodStart = _currentPeriodStart.AddDays(-7);
            }
            else // Month mode
            {
                _currentPeriodStart = _currentPeriodStart.AddMonths(-1);
            }

            UpdatePeriodButtonText();
            UpdatePeriodData();
            DrawTrendChart();
            DrawDistributionChart();
        }

        private void OnNextPeriodClicked(object sender, EventArgs e)
        {
            DateTime nextPeriodStart;

            if (_currentMode == ViewMode.Week)
            {
                nextPeriodStart = _currentPeriodStart.AddDays(7);
            }
            else // Month mode
            {
                nextPeriodStart = _currentPeriodStart.AddMonths(1);
            }

            // Нельзя выбрать период в будущем
            if (nextPeriodStart > DateTime.Today)
                return;

            _currentPeriodStart = nextPeriodStart;

            UpdatePeriodButtonText();
            UpdatePeriodData();
            DrawTrendChart();
            DrawDistributionChart();
        }

        private void OnCurrentPeriodClicked(object sender, EventArgs e)
        {
            SetInitialPeriod();
            UpdatePeriodData();
            DrawTrendChart();
            DrawDistributionChart();
        }
    }
}
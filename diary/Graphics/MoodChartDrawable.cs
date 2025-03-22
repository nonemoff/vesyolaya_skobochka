using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace MoodDiary.Graphics
{
    public class MoodChartDrawable : IDrawable
    {
        private readonly ObservableCollection<MoodViewModel> _moodData;
        private readonly DateTime _currentDate;

        public MoodChartDrawable(ObservableCollection<MoodViewModel> moodData, DateTime currentDate)
        {
            _moodData = moodData;
            _currentDate = currentDate;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_moodData == null || _moodData.Count == 0)
            {
                canvas.FontSize = 14;
                canvas.FontColor = Colors.White;
                string message = _currentDate.Date == DateTime.Today.Date
                    ? "Нет данных за сегодня"
                    : $"Нет данных за {_currentDate:dd.MM.yyyy}";
                canvas.DrawString(message, dirtyRect.Width / 2, dirtyRect.Height / 2, HorizontalAlignment.Center);
                return;
            }

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 80;
            float rightPadding = 15;
            float topPadding = 30;
            float bottomPadding = 40;
            float chartWidth = width - (leftPadding + rightPadding);
            float chartHeight = height - (topPadding + bottomPadding);

            var sortedMoods = _moodData.OrderBy(m => m.Timestamp).ToList();

            canvas.FillColor = Colors.White.WithAlpha(0.1f);
            canvas.FillRectangle(leftPadding, topPadding, chartWidth, chartHeight);

            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            string title = _currentDate.Date == DateTime.Today.Date
                ? "Динамика настроения сегодня"
                : $"Динамика настроения {_currentDate:dd.MM.yyyy}";
            canvas.DrawString(title, width / 2, 10, HorizontalAlignment.Center);
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;

            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            string[] moodLabels = {
                "Подавлен", // 0
                "",        // 1
                "Тревожен", // 2
                "",        // 3
                "Грустен",  // 4
                "",        // 5
                "Нейтрален", // 6
                "",        // 7
                "Радостен", // 8
                "",        // 9
                "Счастлив"  // 10
            };

            float yStep = chartHeight / 10.0f;

            canvas.FontSize = 10;
            canvas.FontColor = Colors.Gray;

            for (int i = 0; i <= 10; i++)
            {
                float y = height - bottomPadding - i * yStep;
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(leftPadding, y, width - rightPadding, y);
                if (!string.IsNullOrEmpty(moodLabels[i]))
                {
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(moodLabels[i], 5, y - 5, HorizontalAlignment.Left);
                }
            }
            DateTime startOfDay = _currentDate.Date;
            DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1);
            TimeSpan dayTimeSpan = endOfDay - startOfDay;
            float timeScale = chartWidth / (float)dayTimeSpan.TotalMinutes;

            canvas.DrawString(
                "00:00",
                leftPadding,
                height - bottomPadding + 20,
                HorizontalAlignment.Center);

            canvas.DrawString(
                "12:00",
                leftPadding + chartWidth / 2,
                height - bottomPadding + 20,
                HorizontalAlignment.Center);

            canvas.DrawString(
                "23:59",
                width - rightPadding,
                height - bottomPadding + 20,
                HorizontalAlignment.Center);

            if (sortedMoods.Count > 0)
            {
                float avgMood = (float)sortedMoods.Average(m => m.MoodValue);
                float avgY = height - bottomPadding - avgMood * yStep;
                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 5, 5 };
                canvas.DrawLine(leftPadding, avgY, width - rightPadding, avgY);
                canvas.StrokeDashPattern = null;

                canvas.FillColor = Colors.Yellow.WithAlpha(0.7f);
                canvas.FillRectangle(leftPadding + 5, avgY - 15, 120, 20);
                canvas.FontColor = Colors.Black;
                canvas.DrawString($"Среднее: {avgMood:F1}", leftPadding + 10, avgY - 5, HorizontalAlignment.Left);

                PointF? prevPoint = null;

                foreach (var mood in sortedMoods)
                {
                    float x = leftPadding + (float)(mood.Timestamp - startOfDay).TotalMinutes * timeScale;
                    float y = height - bottomPadding - mood.MoodValue * yStep;

                    if (prevPoint.HasValue)
                    {
                        canvas.StrokeColor = Colors.Blue;
                        canvas.StrokeSize = 2;
                        canvas.DrawLine(prevPoint.Value.X, prevPoint.Value.Y, x, y);
                    }
                    var moodColor = GetMoodColor(mood.MoodValue);
                    canvas.FillColor = moodColor;
                    canvas.FillCircle(x, y, 7);

                    canvas.StrokeColor = Colors.White;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(x, y, 7);

                    prevPoint = new PointF(x, y);
                }
            }

            if (_currentDate.Date == DateTime.Today.Date)
            {
                DateTime now = DateTime.Now;
                float currentTimeX = leftPadding + (float)(now - startOfDay).TotalMinutes * timeScale;
                if (currentTimeX >= leftPadding && currentTimeX <= width - rightPadding)
                {
                    canvas.StrokeColor = Colors.Red;
                    canvas.StrokeSize = 2;
                    canvas.StrokeDashPattern = new float[] { 3, 3 };
                    canvas.DrawLine(currentTimeX, topPadding, currentTimeX, height - bottomPadding);
                    canvas.StrokeDashPattern = null;

                    canvas.FillColor = Colors.Red.WithAlpha(0.7f);
                    canvas.FillRectangle(currentTimeX - 25, topPadding - 20, 50, 20);
                    canvas.FontColor = Colors.White;
                    canvas.DrawString(now.ToString("HH:mm"), currentTimeX, topPadding - 10, HorizontalAlignment.Center);
                }
            }
        }

        private Color GetMoodColor(int moodValue)
        {
            switch (moodValue)
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
}
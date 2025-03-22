using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using MoodDiary.Models;

namespace MoodDiary.Graphics
{
    public class TrendChartDrawable : IDrawable
    {
        private readonly List<MoodEntry> _moodEntries;
        private readonly bool _isWeekMode;

        public TrendChartDrawable(List<MoodEntry> moodEntries, bool isWeekMode)
        {
            _moodEntries = moodEntries;
            _isWeekMode = isWeekMode;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_moodEntries == null || _moodEntries.Count == 0)
            {
                canvas.FontSize = 14;
                canvas.FontColor = Colors.White;
                canvas.DrawString("Нет данных за выбранный период", dirtyRect.Width / 2, dirtyRect.Height / 2, HorizontalAlignment.Center);
                return;
            }
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 50;
            float rightPadding = 15;
            float topPadding = 30;
            float bottomPadding = 40;
            float chartWidth = width - (leftPadding + rightPadding);
            float chartHeight = height - (topPadding + bottomPadding);
            var sortedEntries = _moodEntries.OrderBy(e => e.Timestamp).ToList();
            DateTime startDate = sortedEntries.First().Timestamp.Date;
            DateTime endDate;

            if (_isWeekMode)
            {
                endDate = startDate.AddDays(6);
            }
            else
            {
                endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
            }
            canvas.FillColor = Colors.White.WithAlpha(0.1f);
            canvas.FillRectangle(leftPadding, topPadding, chartWidth, chartHeight);
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;

            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            canvas.FontSize = 10;
            float yStep = chartHeight / 9.0f;

            for (int i = 0; i <= 9; i++)
            {
                float y = height - bottomPadding - i * yStep;
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(leftPadding, y, width - rightPadding, y);
                canvas.FontColor = Colors.Gray;
                canvas.DrawString(i.ToString(), leftPadding - 15, y - 5, HorizontalAlignment.Left);
            }

            var entriesByDate = sortedEntries
                .GroupBy(e => e.Timestamp.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AverageMood = g.Average(e => e.MoodValue)
                })
                .ToDictionary(g => g.Date, g => g.AverageMood);

            int totalDays = (int)((endDate - startDate).TotalDays) + 1;
            float dayWidth = chartWidth / totalDays;

            if (_isWeekMode)
            {
                for (int i = 0; i < totalDays; i++)
                {
                    DateTime date = startDate.AddDays(i);
                    float x = leftPadding + i * dayWidth + dayWidth / 2;

                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(date.ToString("ddd"), x, height - bottomPadding + 15, HorizontalAlignment.Center);
                }
            }
            else
            {
                int step = Math.Max(1, totalDays / 6);
                for (int i = 0; i < totalDays; i += step)
                {
                    DateTime date = startDate.AddDays(i);
                    float x = leftPadding + i * dayWidth + dayWidth / 2;
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(date.Day.ToString(), x, height - bottomPadding + 15, HorizontalAlignment.Center);
                }
            }

            var dataPoints = new List<PointF>();

            Dictionary<DateTime, float> dailyAverages = new Dictionary<DateTime, float>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (entriesByDate.TryGetValue(date, out double value))
                {
                    dailyAverages[date] = (float)value;
                }
                else
                {
                    DateTime prevDate = date.AddDays(-1);
                    DateTime nextDate = date.AddDays(1);

                    if (entriesByDate.TryGetValue(prevDate, out double prevValue) &&
                        entriesByDate.TryGetValue(nextDate, out double nextValue))
                    {
                        dailyAverages[date] = (float)((prevValue + nextValue) / 2);
                    }
                    else if (entriesByDate.TryGetValue(prevDate, out prevValue))
                    {
                        dailyAverages[date] = (float)prevValue;
                    }
                    else if (entriesByDate.TryGetValue(nextDate, out nextValue))
                    {
                        dailyAverages[date] = (float)nextValue;
                    }
                }
            }
            if (dailyAverages.Count > 0)
            {
                var points = dailyAverages
                    .OrderBy(kv => kv.Key)
                    .Select(kv => new PointF(
                        leftPadding + (float)((kv.Key - startDate).TotalDays) * dayWidth + dayWidth / 2,
                        height - bottomPadding - kv.Value * yStep))
                    .ToList();

                canvas.StrokeColor = Colors.Blue;
                canvas.StrokeSize = 3;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    canvas.DrawLine(points[i], points[i + 1]);
                }

                foreach (var point in points)
                {
                    canvas.FillColor = Colors.Blue;
                    canvas.FillCircle(point.X, point.Y, 5);

                    canvas.StrokeColor = Colors.White;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(point.X, point.Y, 5);
                }

                if (points.Count >= 2)
                {
                    float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
                    int n = points.Count;

                    for (int i = 0; i < n; i++)
                    {
                        float x = i;
                        float y = points[i].Y;

                        sumX += x;
                        sumY += y;
                        sumXY += x * y;
                        sumX2 += x * x;
                    }

                    float slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
                    float intercept = (sumY - slope * sumX) / n;

                    float startY = intercept;
                    float endY = intercept + slope * (n - 1);

                    startY = Math.Max(topPadding, Math.Min(height - bottomPadding, startY));
                    endY = Math.Max(topPadding, Math.Min(height - bottomPadding, endY));

                    canvas.StrokeColor = Colors.Red;
                    canvas.StrokeSize = 2;
                    canvas.StrokeDashPattern = new float[] { 5, 5 };
                    canvas.DrawLine(points[0].X, startY, points[n - 1].X, endY);
                    canvas.StrokeDashPattern = null;
                }
            }

            if (_moodEntries.Count > 0)
            {
                float avgMood = (float)_moodEntries.Average(e => e.MoodValue);
                float avgY = height - bottomPadding - avgMood * yStep;

                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 3, 3 };
                canvas.DrawLine(leftPadding, avgY, width - rightPadding, avgY);
                canvas.StrokeDashPattern = null;

                canvas.FillColor = Colors.Yellow.WithAlpha(0.7f);
                canvas.FillRectangle(leftPadding + 5, avgY - 15, 80, 20);
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 10;
                canvas.DrawString($"Среднее: {avgMood:F1}", leftPadding + 10, avgY - 5, HorizontalAlignment.Left);
            }
        }
    }
}
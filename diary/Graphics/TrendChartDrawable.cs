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
                // Draw empty chart message
                canvas.FontSize = 14;
                canvas.FontColor = Colors.White;
                canvas.DrawString("Нет данных за выбранный период", dirtyRect.Width / 2, dirtyRect.Height / 2, HorizontalAlignment.Center);
                return;
            }

            // Set up chart dimensions
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 50;
            float rightPadding = 15;
            float topPadding = 30;
            float bottomPadding = 40;
            float chartWidth = width - (leftPadding + rightPadding);
            float chartHeight = height - (topPadding + bottomPadding);

            // Sort entries by date
            var sortedEntries = _moodEntries.OrderBy(e => e.Timestamp).ToList();

            // Get date range for the chart
            DateTime startDate = sortedEntries.First().Timestamp.Date;
            DateTime endDate;

            if (_isWeekMode)
            {
                // Week mode - end date is start date + 6 days
                endDate = startDate.AddDays(6);
            }
            else
            {
                // Month mode - end date is the last day of the month
                endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
            }

            // Draw chart background
            canvas.FillColor = Colors.White.WithAlpha(0.1f);
            canvas.FillRectangle(leftPadding, topPadding, chartWidth, chartHeight);

            // Draw chart axes
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;

            // X-axis
            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);

            // Y-axis
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            // Draw horizontal grid lines for Y-axis
            canvas.FontSize = 10;
            float yStep = chartHeight / 9.0f; // 0-9 mood scale

            for (int i = 0; i <= 9; i++)
            {
                float y = height - bottomPadding - i * yStep;

                // Grid line
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(leftPadding, y, width - rightPadding, y);

                // Label
                canvas.FontColor = Colors.Gray;
                canvas.DrawString(i.ToString(), leftPadding - 15, y - 5, HorizontalAlignment.Left);
            }

            // Group entries by date
            var entriesByDate = sortedEntries
                .GroupBy(e => e.Timestamp.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AverageMood = g.Average(e => e.MoodValue)
                })
                .ToDictionary(g => g.Date, g => g.AverageMood);

            // Draw X-axis labels
            int totalDays = (int)((endDate - startDate).TotalDays) + 1;
            float dayWidth = chartWidth / totalDays;

            if (_isWeekMode)
            {
                // For week mode, show each day
                for (int i = 0; i < totalDays; i++)
                {
                    DateTime date = startDate.AddDays(i);
                    float x = leftPadding + i * dayWidth + dayWidth / 2;

                    // Day name label
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(date.ToString("ddd"), x, height - bottomPadding + 15, HorizontalAlignment.Center);
                }
            }
            else
            {
                // For month mode, show every 5 days or so
                int step = Math.Max(1, totalDays / 6); // Adjust to show approximately 6 labels
                for (int i = 0; i < totalDays; i += step)
                {
                    DateTime date = startDate.AddDays(i);
                    float x = leftPadding + i * dayWidth + dayWidth / 2;

                    // Date label
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(date.Day.ToString(), x, height - bottomPadding + 15, HorizontalAlignment.Center);
                }
            }

            // Prepare data points for the line chart
            var dataPoints = new List<PointF>();

            // Generate daily averages for full period
            Dictionary<DateTime, float> dailyAverages = new Dictionary<DateTime, float>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (entriesByDate.TryGetValue(date, out double value))
                {
                    dailyAverages[date] = (float)value;
                }
                else
                {
                    // For days without data, use interpolation if possible
                    DateTime prevDate = date.AddDays(-1);
                    DateTime nextDate = date.AddDays(1);

                    if (entriesByDate.TryGetValue(prevDate, out double prevValue) &&
                        entriesByDate.TryGetValue(nextDate, out double nextValue))
                    {
                        // Linear interpolation
                        dailyAverages[date] = (float)((prevValue + nextValue) / 2);
                    }
                    else if (entriesByDate.TryGetValue(prevDate, out prevValue))
                    {
                        // Use previous value
                        dailyAverages[date] = (float)prevValue;
                    }
                    else if (entriesByDate.TryGetValue(nextDate, out nextValue))
                    {
                        // Use next value
                        dailyAverages[date] = (float)nextValue;
                    }
                    // If no nearby values, skip this day
                }
            }

            // Draw trend line
            if (dailyAverages.Count > 0)
            {
                // Convert to points
                var points = dailyAverages
                    .OrderBy(kv => kv.Key)
                    .Select(kv => new PointF(
                        leftPadding + (float)((kv.Key - startDate).TotalDays) * dayWidth + dayWidth / 2,
                        height - bottomPadding - kv.Value * yStep))
                    .ToList();

                // Draw line segments
                canvas.StrokeColor = Colors.Blue;
                canvas.StrokeSize = 3;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    canvas.DrawLine(points[i], points[i + 1]);
                }

                // Draw points
                foreach (var point in points)
                {
                    // Draw point
                    canvas.FillColor = Colors.Blue;
                    canvas.FillCircle(point.X, point.Y, 5);

                    // Draw outline
                    canvas.StrokeColor = Colors.White;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(point.X, point.Y, 5);
                }

                // Draw trend line
                if (points.Count >= 2)
                {
                    // Simple linear regression
                    float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
                    int n = points.Count;

                    for (int i = 0; i < n; i++)
                    {
                        float x = i; // Use index as x value
                        float y = points[i].Y;

                        sumX += x;
                        sumY += y;
                        sumXY += x * y;
                        sumX2 += x * x;
                    }

                    float slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
                    float intercept = (sumY - slope * sumX) / n;

                    // Draw trend line
                    float startY = intercept;
                    float endY = intercept + slope * (n - 1);

                    // Clamp values to chart bounds
                    startY = Math.Max(topPadding, Math.Min(height - bottomPadding, startY));
                    endY = Math.Max(topPadding, Math.Min(height - bottomPadding, endY));

                    canvas.StrokeColor = Colors.Red;
                    canvas.StrokeSize = 2;
                    canvas.StrokeDashPattern = new float[] { 5, 5 }; // Dashed line
                    canvas.DrawLine(points[0].X, startY, points[n - 1].X, endY);
                    canvas.StrokeDashPattern = null; // Reset dash pattern
                }
            }

            // Draw average line
            if (_moodEntries.Count > 0)
            {
                float avgMood = (float)_moodEntries.Average(e => e.MoodValue);
                float avgY = height - bottomPadding - avgMood * yStep;

                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 3, 3 }; // Dashed line
                canvas.DrawLine(leftPadding, avgY, width - rightPadding, avgY);
                canvas.StrokeDashPattern = null; // Reset dash pattern

                // Label
                canvas.FillColor = Colors.Yellow.WithAlpha(0.7f);
                canvas.FillRectangle(leftPadding + 5, avgY - 15, 80, 20);
                canvas.FontColor = Colors.Black;
                canvas.FontSize = 10;
                canvas.DrawString($"Среднее: {avgMood:F1}", leftPadding + 10, avgY - 5, HorizontalAlignment.Left);
            }
        }
    }
}
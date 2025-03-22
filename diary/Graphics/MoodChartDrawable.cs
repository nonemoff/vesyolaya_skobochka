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
                // Draw empty chart message
                canvas.FontSize = 14;
                canvas.FontColor = Colors.White;
                string message = _currentDate.Date == DateTime.Today.Date
                    ? "Нет данных за сегодня"
                    : $"Нет данных за {_currentDate:dd.MM.yyyy}";
                canvas.DrawString(message, dirtyRect.Width / 2, dirtyRect.Height / 2, HorizontalAlignment.Center);
                return;
            }

            // Set up chart dimensions with increased padding
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float leftPadding = 80; // Significantly increased for mood text labels
            float rightPadding = 15;
            float topPadding = 30; // Increased for title
            float bottomPadding = 40; // For time labels
            float chartWidth = width - (leftPadding + rightPadding);
            float chartHeight = height - (topPadding + bottomPadding);

            // Sort moods by timestamp first to avoid potential issues
            var sortedMoods = _moodData.OrderBy(m => m.Timestamp).ToList();

            // Draw chart background
            canvas.FillColor = Colors.White.WithAlpha(0.1f);
            canvas.FillRectangle(leftPadding, topPadding, chartWidth, chartHeight);

            // Draw chart title at the very top
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            string title = _currentDate.Date == DateTime.Today.Date
                ? "Динамика настроения сегодня"
                : $"Динамика настроения {_currentDate:dd.MM.yyyy}";
            canvas.DrawString(title, width / 2, 10, HorizontalAlignment.Center);

            // Draw chart axes
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;

            // X-axis
            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);

            // Y-axis
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            // Define mood names for specific points on the scale
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

            // Calculate the step size for grid lines
            float yStep = chartHeight / 10.0f;

            // Draw horizontal grid lines and labels for Y-axis with mood names
            canvas.FontSize = 10;
            canvas.FontColor = Colors.Gray;

            for (int i = 0; i <= 10; i++)
            {
                float y = height - bottomPadding - i * yStep;

                // Grid line for all points
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(leftPadding, y, width - rightPadding, y);

                // Label with mood name for specific points
                if (!string.IsNullOrEmpty(moodLabels[i]))
                {
                    canvas.FontColor = Colors.Gray;
                    canvas.DrawString(moodLabels[i], 5, y - 5, HorizontalAlignment.Left);
                }
            }

            // Calculate the fixed time range for the day (00:00 to 23:59)
            DateTime startOfDay = _currentDate.Date;
            DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1); // 23:59:59
            TimeSpan dayTimeSpan = endOfDay - startOfDay;
            float timeScale = chartWidth / (float)dayTimeSpan.TotalMinutes;

            // Draw time labels on X-axis
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

            // Calculate average mood if data exists
            if (sortedMoods.Count > 0)
            {
                // Calculate average mood
                float avgMood = (float)sortedMoods.Average(m => m.MoodValue);

                // Calculate average mood line position with precise step calculation
                float avgY = height - bottomPadding - avgMood * yStep;

                // Draw average mood line
                canvas.StrokeColor = Colors.Yellow;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 5, 5 }; // Dashed line
                canvas.DrawLine(leftPadding, avgY, width - rightPadding, avgY);

                // Reset dash pattern
                canvas.StrokeDashPattern = null;

                // Add average mood label
                canvas.FillColor = Colors.Yellow.WithAlpha(0.7f);
                canvas.FillRectangle(leftPadding + 5, avgY - 15, 120, 20);
                canvas.FontColor = Colors.Black;
                canvas.DrawString($"Среднее: {avgMood:F1}", leftPadding + 10, avgY - 5, HorizontalAlignment.Left);

                // Draw the mood points and lines
                PointF? prevPoint = null;

                foreach (var mood in sortedMoods)
                {
                    // Calculate X position based on time of day (relative to start of day)
                    float x = leftPadding + (float)(mood.Timestamp - startOfDay).TotalMinutes * timeScale;

                    // Calculate Y position with precise step calculation
                    float y = height - bottomPadding - mood.MoodValue * yStep;

                    // Draw line to previous point
                    if (prevPoint.HasValue)
                    {
                        canvas.StrokeColor = Colors.Blue;
                        canvas.StrokeSize = 2;
                        canvas.DrawLine(prevPoint.Value.X, prevPoint.Value.Y, x, y);
                    }

                    // Draw the point after the line so it's on top
                    var moodColor = GetMoodColor(mood.MoodValue);
                    canvas.FillColor = moodColor;
                    canvas.FillCircle(x, y, 7); // Larger point for better visibility

                    // Draw outline for better visibility
                    canvas.StrokeColor = Colors.White;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(x, y, 7);

                    prevPoint = new PointF(x, y);
                }
            }

            // Draw current time line only for today
            if (_currentDate.Date == DateTime.Today.Date)
            {
                DateTime now = DateTime.Now;
                float currentTimeX = leftPadding + (float)(now - startOfDay).TotalMinutes * timeScale;

                // Only draw if within the chart's range
                if (currentTimeX >= leftPadding && currentTimeX <= width - rightPadding)
                {
                    // Draw vertical current time line
                    canvas.StrokeColor = Colors.Red;
                    canvas.StrokeSize = 2;
                    canvas.StrokeDashPattern = new float[] { 3, 3 }; // Dashed line
                    canvas.DrawLine(currentTimeX, topPadding, currentTimeX, height - bottomPadding);

                    // Reset dash pattern
                    canvas.StrokeDashPattern = null;

                    // Add current time label
                    canvas.FillColor = Colors.Red.WithAlpha(0.7f);
                    canvas.FillRectangle(currentTimeX - 25, topPadding - 20, 50, 20);
                    canvas.FontColor = Colors.White;
                    canvas.DrawString(now.ToString("HH:mm"), currentTimeX, topPadding - 10, HorizontalAlignment.Center);
                }
            }
        }

        private Color GetMoodColor(int moodValue)
        {
            // Return different colors based on mood value (0 = worst mood, 9 = best mood)
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
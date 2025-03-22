using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using MoodDiary.Models;

namespace MoodDiary.Graphics
{
    public class DistributionChartDrawable : IDrawable
    {
        private readonly List<MoodEntry> _moodEntries;

        public DistributionChartDrawable(List<MoodEntry> moodEntries)
        {
            _moodEntries = moodEntries;
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
            float topPadding = 10;
            float bottomPadding = 30;
            float chartWidth = width - (leftPadding + rightPadding);
            float chartHeight = height - (topPadding + bottomPadding);

            var distribution = new int[10];
            foreach (var entry in _moodEntries)
            {
                if (entry.MoodValue >= 0 && entry.MoodValue <= 9)
                {
                    distribution[entry.MoodValue]++;
                }
            }

            int maxCount = distribution.Max();
            if (maxCount == 0)
            {
                canvas.FontSize = 14;
                canvas.FontColor = Colors.White;
                canvas.DrawString("Недостаточно данных для анализа", dirtyRect.Width / 2, dirtyRect.Height / 2, HorizontalAlignment.Center);
                return;
            }

            canvas.FillColor = Colors.White.WithAlpha(0.1f);
            canvas.FillRectangle(leftPadding, topPadding, chartWidth, chartHeight);

            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;


            canvas.DrawLine(leftPadding, height - bottomPadding, width - rightPadding, height - bottomPadding);
            canvas.DrawLine(leftPadding, topPadding, leftPadding, height - bottomPadding);

            string[] moodLabels = {
                "Подв", // 0 - Подавлен
                "Трев", // 1 - Тревожен
                "Разч", // 2 - Разочарован
                "Злюсь", // 3
                "Груст", // 4 - Грустен
                "Озад", // 5 - Озадачен
                "Нейт", // 6 - Нейтрален
                "Спок", // 7 - Спокоен
                "Рад", // 8 - Радостен
                "Счст" // 9 - Счастлив
            };

            float barWidth = chartWidth / 10 * 0.8f;
            float barSpacing = chartWidth / 10 * 0.2f;
            float barCenter = chartWidth / 10;

            for (int i = 0; i < 10; i++)
            {
                float x = leftPadding + i * barCenter;
                float barHeight = distribution[i] * chartHeight / maxCount;
                float barX = x + barSpacing / 2;
                float barY = height - bottomPadding - barHeight;

                canvas.FillColor = GetMoodColor(i);
                canvas.FillRectangle(barX, barY, barWidth, barHeight);
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(barX, barY, barWidth, barHeight);

                if (barHeight > 15)
                {
                    canvas.FontColor = Colors.White;
                    canvas.FontSize = 10;
                    canvas.DrawString(distribution[i].ToString(), barX + barWidth / 2, barY + 10, HorizontalAlignment.Center);
                }

                canvas.FontColor = Colors.Gray;
                canvas.FontSize = 10;
                canvas.DrawString(moodLabels[i], barX + barWidth / 2, height - bottomPadding + 15, HorizontalAlignment.Center);
            }

            canvas.FontSize = 10;
            canvas.FontColor = Colors.Gray;
            int stepSize = Math.Max(1, maxCount / 4);
            for (int i = 0; i <= maxCount; i += stepSize)
            {
                float y = height - bottomPadding - i * chartHeight / maxCount;
                canvas.DrawString(i.ToString(), leftPadding - 15, y - 5, HorizontalAlignment.Left);
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(leftPadding, y, width - rightPadding, y);
            }
            int mostCommonMoodIndex = Array.IndexOf(distribution, maxCount);
            string mostCommonMood = GetFullMoodName(mostCommonMoodIndex);

            canvas.FillColor = Colors.White.WithAlpha(0.7f);
            canvas.FillRectangle(leftPadding + 10, topPadding + 10, 180, 20);
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 10;
            canvas.DrawString($"Самое частое настроение: {mostCommonMood}", leftPadding + 15, topPadding + 20, HorizontalAlignment.Left);
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

        private string GetFullMoodName(int index)
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
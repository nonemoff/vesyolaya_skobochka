using System;

namespace MoodDiary.Models
{
    public class MoodEntry
    {
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Значение настроения (индекс от 0 до 9)
        /// </summary>
        public int MoodValue { get; set; }
    }
}

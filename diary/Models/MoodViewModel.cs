using System;

namespace MoodDiary
{
    public class MoodViewModel
    {
        public DateTime Timestamp { get; set; }
        public string MoodName { get; set; }
        public int MoodValue { get; set; }
    }
}
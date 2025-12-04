﻿namespace JeopardyGame.Models.GameModels
{
    public class Question
    {
        public int Value { get; set; }
        public bool IsUsed { get; set; }
        public string? MusicFile { get; set; } // Path to music file in wwwroot
        public string? Answer { get; set; } // Song title/answer
    }
}

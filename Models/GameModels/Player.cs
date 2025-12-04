﻿namespace JeopardyGame.Models.GameModels
{
    public class Player
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; } = 0;
        public string ConnectionId { get; set; } = string.Empty;
        public string? TeamId { get; set; } = null; // null for solo mode
    }
}

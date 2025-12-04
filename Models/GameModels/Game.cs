﻿namespace JeopardyGame.Models.GameModels
{
    public class Game
    {
        public string GameCode { get; set; } = string.Empty;
        public string HostConnectionId { get; set; } = string.Empty;
        public Dictionary<string, Player> Players { get; set; } = new();
        public GameBoard Board { get; set; } = new();
        public List<BuzzEntry> CurrentBuzzes { get; set; } = new();
        public string CurrentQuestion { get; set; } = string.Empty;
        public int CurrentQuestionValue { get; set; } = 0;
        public bool IsBuzzingActive { get; set; } = false;
        
        // Team mode properties
        public bool IsTeamMode { get; set; } = false;
        public Dictionary<string, Team> Teams { get; set; } = new();
        public bool IsGameStarted { get; set; } = false; // true when host starts the game from lobby
        public HashSet<string> BuzzedTeamIds { get; set; } = new(); // Track which teams have buzzed
        public bool IsTimerRunning { get; set; } = false; // Track if a team timer is currently running
        public bool IsExtraTimeActive { get; set; } = false; // Track if extra time is active
    }
}

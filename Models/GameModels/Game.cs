namespace JeopardyGame.Models.GameModels
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
    }
}

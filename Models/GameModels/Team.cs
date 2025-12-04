namespace JeopardyGame.Models.GameModels
{
    public class Team
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; } = 0;
        public List<Player> Players { get; set; } = new();
        public string Color { get; set; } = "#667eea"; // Default color for team
    }
}


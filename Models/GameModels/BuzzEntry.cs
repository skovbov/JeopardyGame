namespace JeopardyGame.Models.GameModels
{
    public class BuzzEntry
    {
        public Player Player { get; set; } = new();
        public DateTime Time { get; set; }
        public int Order { get; set; }
    }
}

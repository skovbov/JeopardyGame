namespace JeopardyGame.Models.GameModels
{
    public class Category
    {
        public string Name { get; set; } = string.Empty;
        public List<Question> Questions { get; set; } = new();
    }
}

namespace JeopardyGame.Models.GameModels
{
    public class GameBoard
    {
        public List<Category> Categories { get; set; } = new()
    {
        new Category { Name = "Historie", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false },
            new Question { Value = 200, IsUsed = false },
            new Question { Value = 300, IsUsed = false },
            new Question { Value = 400, IsUsed = false },
            new Question { Value = 500, IsUsed = false }
        }},
        new Category { Name = "Geografi", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false },
            new Question { Value = 200, IsUsed = false },
            new Question { Value = 300, IsUsed = false },
            new Question { Value = 400, IsUsed = false },
            new Question { Value = 500, IsUsed = false }
        }},
        new Category { Name = "Sport", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false },
            new Question { Value = 200, IsUsed = false },
            new Question { Value = 300, IsUsed = false },
            new Question { Value = 400, IsUsed = false },
            new Question { Value = 500, IsUsed = false }
        }},
        new Category { Name = "Videnskab", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false },
            new Question { Value = 200, IsUsed = false },
            new Question { Value = 300, IsUsed = false },
            new Question { Value = 400, IsUsed = false },
            new Question { Value = 500, IsUsed = false }
        }},
        new Category { Name = "Underholdning", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false },
            new Question { Value = 200, IsUsed = false },
            new Question { Value = 300, IsUsed = false },
            new Question { Value = 400, IsUsed = false },
            new Question { Value = 500, IsUsed = false }
        }}
    };
    }
}

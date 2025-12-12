﻿namespace JeopardyGame.Models.GameModels
{
    public class GameBoard
    {
        public List<Category> Categories { get; set; } = new()
    {
        new Category { Name = "Pop Musik", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/pop1.mp3", Answer = "Pop sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/pop2.mp3", Answer = "Pop sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/pop3.mp3", Answer = "Pop sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/pop4.mp3", Answer = "Pop sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/pop5.mp3", Answer = "Pop sang 5" }
        }},
        new Category { Name = "Rock", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/rock1.mp3", Answer = "Rock sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/rock2.mp3", Answer = "Rock sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/rock3.mp3", Answer = "Rock sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/rock4.mp3", Answer = "Rock sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/rock5.mp3", Answer = "Rock sang 5" }
        }},
        new Category { Name = "Rap/Hip-Hop", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/rap1.mp3", Answer = "Rap sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/rap2.mp3", Answer = "Rap sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/rap3.mp3", Answer = "Rap sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/rap4.mp3", Answer = "Rap sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/rap5.mp3", Answer = "Rap sang 5" }
        }},
        new Category { Name = "Dansk Musik", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/dansk1.mp3", Answer = "Dansk sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/dansk2.mp3", Answer = "Dansk sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/dansk3.mp3", Answer = "Dansk sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/dansk4.mp3", Answer = "Dansk sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/dansk5.mp3", Answer = "Dansk sang 5" }
        }},
        new Category { Name = "Klassikere", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/classic1.mp3", Answer = "Klassisk sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/classic2.mp3", Answer = "Klassisk sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/classic3.mp3", Answer = "Klassisk sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/classic4.mp3", Answer = "Klassisk sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/classic5.mp3", Answer = "Klassisk sang 5" }
        }},
        new Category { Name = "90'erne", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/music/90s1.mp3", Answer = "90'erne sang 1" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/music/90s2.mp3", Answer = "90'erne sang 2" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/music/90s3.mp3", Answer = "90'erne sang 3" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/music/90s4.mp3", Answer = "90'erne sang 4" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/music/90s5.mp3", Answer = "90'erne sang 5" }
        }}
    };
    }
}

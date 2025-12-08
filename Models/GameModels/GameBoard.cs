﻿namespace JeopardyGame.Models.GameModels
{
    public class GameBoard
    {
        public List<Category> Categories { get; set; } = new()
    {
        new Category { Name = "Number 1 on billboard", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/audio/bil100.mp3", Answer = "Rolling In The Deep - Adele" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/audio/bil200.mp3", Answer = "Rude Boy - Rihanna" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/audio/bil300.mp3", Answer = "The Hills - The Weeknd" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/audio/bil400.mp3", Answer = "Without Me - Halsey" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/audio/bil500.mp3", Answer = "Blurred Lines - Pharrell Williams, Robin Thicke" }
        }},
        new Category { Name = "Mixed", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/audio/mix100.mp3", Answer = "Endnu en krig - Gobs, Lasse Skriver" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/audio/mix200.mp3", Answer = "Mirror - Lil Wayne, Bruno Mars" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/audio/mix300.mp3", Answer = "Den Anden Side - Rasmus Seebach" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/audio/mix400.mp3", Answer = "Ride - Twenty One Pilots" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/audio/mix500.mp3", Answer = "The Man Who Can't Be Moved - The Script" }
        }},
        new Category { Name = "Perker", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/audio/per100.mp3", Answer = "L.U.V - Gilli" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/audio/per200.mp3", Answer = "Sicario - Sleimann" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/audio/per300.mp3", Answer = "Pengeseddel - ATYPISK, Branco" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/audio/per400.mp3", Answer = "INGEN INTRODUKTION - Branco" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/audio/per500.mp3", Answer = "Rally - Sivas, Gilli" }
        }},
        new Category { Name = "Tv-Series & Movies", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/audio/tv100.mp3", Answer = "Dexter" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/audio/tv200.mp3", Answer = "Klassefesten" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/audio/tv300.mp3", Answer = "James Bond: Skyfall" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/audio/tv400.mp3", Answer = "Avatar" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/audio/tv500.mp3", Answer = "Gladiator" }
        }},
        new Category { Name = "Top 100 most streamed", Questions = new List<Question>
        {
            new Question { Value = 100, IsUsed = false, MusicFile = "/audio/top100.mp3", Answer = "As It Was - Harry Styles" },
            new Question { Value = 200, IsUsed = false, MusicFile = "/audio/top200.mp3", Answer = "Photograph - Ed Sheeran" },
            new Question { Value = 300, IsUsed = false, MusicFile = "/audio/top300.mp3", Answer = "Drivers License - Olivia Rodrigo" },
            new Question { Value = 400, IsUsed = false, MusicFile = "/audio/top400.mp3", Answer = "Sweater Weather - The Neighbourhood" },
            new Question { Value = 500, IsUsed = false, MusicFile = "/audio/top500.mp3", Answer = "Iris - The Goo Goo Dolls" }
        }}
    };
    }
}

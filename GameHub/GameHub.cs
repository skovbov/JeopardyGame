using JeopardyGame.Models.GameModels;
using JeopardyGame.Services;
using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task CreateGame(string hostName)
    {
        var game = _gameManager.CreateGame(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.GameCode);

        await Clients.Caller.SendAsync("GameCreated", new
        {
            gameCode = game.GameCode,
            board = game.Board
        });
    }

    public async Task JoinGame(string gameCode, string playerName)
    {
        var game = _gameManager.GetGame(gameCode);
        if (game == null)
        {
            await Clients.Caller.SendAsync("Error", "Spil ikke fundet");
            return;
        }

        var player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = playerName,
            ConnectionId = Context.ConnectionId
        };

        if (_gameManager.JoinGame(gameCode, player))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);

            // Send til spilleren
            await Clients.Caller.SendAsync("JoinedGame", new
            {
                gameCode = gameCode,
                playerId = player.Id,
                playerName = player.Name,
                board = game.Board
            });

            // Opdater alle i spillet
            await Clients.Group(gameCode).SendAsync("PlayersUpdated",
                game.Players.Values.Select(p => new { p.Id, p.Name, p.Score }));
        }
    }

    public async Task SelectQuestion(int categoryIndex, int questionIndex)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        var question = game.Board.Categories[categoryIndex].Questions[questionIndex];
        if (question.IsUsed) return;

        question.IsUsed = true;
        game.CurrentQuestionValue = question.Value;
        game.IsBuzzingActive = true;
        game.CurrentBuzzes.Clear();

        await Clients.Group(game.GameCode).SendAsync("QuestionSelected", new
        {
            categoryIndex,
            questionIndex,
            value = question.Value,
            board = game.Board
        });

        // Start 10 sekund timer for auto-reset
        _ = Task.Run(async () =>
        {
            await Task.Delay(10000); // 10 sekunder
            if (game.IsBuzzingActive && game.CurrentBuzzes.Count == 0)
            {
                game.IsBuzzingActive = false;
                await Clients.Group(game.GameCode).SendAsync("BuzzTimeExpired");
                await Clients.Client(game.HostConnectionId).SendAsync("BuzzTimeExpired");
            }
        });

        await Clients.Group(game.GameCode).SendAsync("BuzzingStarted");
    }

    public async Task Buzz()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || !game.IsBuzzingActive) return;

        var player = game.Players.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (player == null) return;

        // Tjek om spilleren allerede har buzzet
        if (game.CurrentBuzzes.Any(b => b.Player.Id == player.Id)) return;

        var buzzEntry = new BuzzEntry
        {
            Player = player,
            Time = DateTime.Now,
            Order = game.CurrentBuzzes.Count + 1
        };

        game.CurrentBuzzes.Add(buzzEntry);

        // Send opdatering til værten
        await Clients.Client(game.HostConnectionId).SendAsync("BuzzReceived", new
        {
            playerName = player.Name,
            order = buzzEntry.Order,
            buzzes = game.CurrentBuzzes.Select(b => new
            {
                playerName = b.Player.Name,
                order = b.Order
            }).ToList()
        });

        // Deaktiver buzz knappen for denne spiller
        await Clients.Caller.SendAsync("BuzzDisabled");
    }

    public async Task ResetBuzz()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        game.CurrentBuzzes.Clear();
        game.IsBuzzingActive = false;

        await Clients.Group(game.GameCode).SendAsync("BuzzReset");
        await Clients.Client(game.HostConnectionId).SendAsync("BuzzCleared");
    }

    public async Task UpdateScore(string playerId, int points)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        if (game.Players.TryGetValue(playerId, out var player))
        {
            player.Score += points;

            await Clients.Group(game.GameCode).SendAsync("ScoreUpdated", new
            {
                playerId,
                playerName = player.Name,
                newScore = player.Score,
                players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score })
            });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game != null)
        {
            if (game.HostConnectionId == Context.ConnectionId)
            {
                // Vært forlod - luk spillet
                await Clients.Group(game.GameCode).SendAsync("GameEnded", "Værten forlod spillet");
                _gameManager.RemoveGame(game.GameCode);
            }
            else
            {
                // Spiller forlod
                _gameManager.RemovePlayer(Context.ConnectionId);
                await Clients.Group(game.GameCode).SendAsync("PlayersUpdated",
                    game.Players.Values.Select(p => new { p.Id, p.Name, p.Score }));
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
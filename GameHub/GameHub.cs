﻿using JeopardyGame.Models.GameModels;
using JeopardyGame.Services;
using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    private readonly GameManager _gameManager;

    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task CreateGame(string hostName, bool isTeamMode = false)
    {
        var game = _gameManager.CreateGame(Context.ConnectionId, isTeamMode);
        await Groups.AddToGroupAsync(Context.ConnectionId, game.GameCode);

        await Clients.Caller.SendAsync("GameCreated", new
        {
            gameCode = game.GameCode,
            board = game.Board,
            isTeamMode = game.IsTeamMode
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
            ConnectionId = Context.ConnectionId,
            TeamId = null
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
                board = game.Board,
                isTeamMode = game.IsTeamMode,
                isGameStarted = game.IsGameStarted,
                teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() })
            });

            // Opdater alle i spillet
            await Clients.Group(gameCode).SendAsync("PlayersUpdated",
                game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId }));
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
        game.BuzzedTeamIds.Clear();
        game.IsTimerRunning = false;
        game.IsExtraTimeActive = false;

        await Clients.Group(game.GameCode).SendAsync("QuestionSelected", new
        {
            categoryIndex,
            questionIndex,
            value = question.Value,
            board = game.Board
        });

        // Send music file to host only
        if (!string.IsNullOrEmpty(question.MusicFile))
        {
            await Clients.Client(game.HostConnectionId).SendAsync("PlayMusic", new
            {
                musicFile = question.MusicFile,
                answer = question.Answer
            });
        }

        // Don't start timer here - timer will start when first player buzzes
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

        // In team mode, check if the team has already buzzed
        if (game.IsTeamMode && player.TeamId != null)
        {
            if (game.BuzzedTeamIds.Contains(player.TeamId)) return;
            game.BuzzedTeamIds.Add(player.TeamId);
        }

        var buzzEntry = new BuzzEntry
        {
            Player = player,
            Time = DateTime.Now,
            Order = game.CurrentBuzzes.Count + 1
        };

        game.CurrentBuzzes.Add(buzzEntry);

        // In team mode, disable buzz for all team members
        if (game.IsTeamMode && player.TeamId != null && game.Teams.TryGetValue(player.TeamId, out var team))
        {
            foreach (var teamMember in team.Players)
            {
                await Clients.Client(teamMember.ConnectionId).SendAsync("BuzzDisabled");
            }
            
            // Send team buzz notification to host
            await Clients.Client(game.HostConnectionId).SendAsync("BuzzReceived", new
            {
                playerName = $"{team.Name} ({player.Name})",
                order = buzzEntry.Order,
                teamId = team.Id,
                teamName = team.Name,
                buzzes = game.CurrentBuzzes.Select(b => new
                {
                    playerName = game.IsTeamMode && b.Player.TeamId != null && game.Teams.TryGetValue(b.Player.TeamId, out var t) 
                        ? $"{t.Name} ({b.Player.Name})" 
                        : b.Player.Name,
                    order = b.Order
                }).ToList()
            });

            // Handle timer logic for team mode
            if (game.CurrentBuzzes.Count == 1)
            {
                // First team buzzed - stop music and start their 10 second timer
                await Clients.Client(game.HostConnectionId).SendAsync("StopMusic");
                game.IsTimerRunning = true;
                game.IsExtraTimeActive = false;
                await Clients.GroupExcept(game.GameCode, game.HostConnectionId).SendAsync("BuzzTimerStarted", new { teamId = team.Id, duration = 10 });
                
                _ = Task.Run(async () => await ProcessTeamTimers(game));
            }
            else if (!game.IsTimerRunning && game.IsExtraTimeActive)
            {
                // Team buzzed during extra time - stop music and start their 10 second timer immediately
                await Clients.Client(game.HostConnectionId).SendAsync("StopMusic");
                game.IsTimerRunning = true;
                game.IsExtraTimeActive = false;
                await Clients.GroupExcept(game.GameCode, game.HostConnectionId).SendAsync("BuzzTimerStarted", new { teamId = team.Id, duration = 10 });
                
                _ = Task.Run(async () => await ProcessTeamTimers(game));
            }
            // If timer is running, the team's timer will start after current timer expires
        }
        else
        {
            // Solo mode - original logic with 10 second extra time
            if (game.CurrentBuzzes.Count == 1)
            {
                await Clients.GroupExcept(game.GameCode, game.HostConnectionId).SendAsync("BuzzTimerStarted");
                
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    if (game.IsBuzzingActive)
                    {
                        await Clients.Group(game.GameCode).SendAsync("BuzzTimeExpired");
                        
                        // Changed to 10 seconds extra time
                        await Task.Delay(10000);
                        if (game.IsBuzzingActive)
                        {
                            game.IsBuzzingActive = false;
                            await Clients.Group(game.GameCode).SendAsync("ExtendedBuzzTimeExpired");
                            await Clients.Client(game.HostConnectionId).SendAsync("ExtendedBuzzTimeExpired");
                        }
                    }
                });
            }

            // Solo mode - send opdatering til værten
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

            await Clients.Caller.SendAsync("BuzzDisabled");
        }
    }

    private async Task ProcessTeamTimers(Game game)
    {
        while (game.IsBuzzingActive && game.IsTeamMode)
        {
            // Check if all teams have buzzed
            if (game.BuzzedTeamIds.Count >= game.Teams.Count)
            {
                // All teams have buzzed - process remaining timers sequentially without extra time
                game.IsExtraTimeActive = false;
                
                // Wait for current timer if running
                if (game.IsTimerRunning)
                {
                    await Task.Delay(10000);
                    // Resume music after timer
                    await Clients.Client(game.HostConnectionId).SendAsync("ResumeMusic");
                }
                
                // Check if there are more teams that buzzed while timer was running
                int processedBuzzes = 1;
                while (processedBuzzes < game.CurrentBuzzes.Count && game.IsBuzzingActive)
                {
                    var nextBuzz = game.CurrentBuzzes[processedBuzzes];
                    if (game.Teams.TryGetValue(nextBuzz.Player.TeamId!, out var nextTeam))
                    {
                        // Stop music for next team's timer
                        await Clients.Client(game.HostConnectionId).SendAsync("StopMusic");
                        await Clients.GroupExcept(game.GameCode, game.HostConnectionId)
                            .SendAsync("BuzzTimerStarted", new { teamId = nextTeam.Id, duration = 10 });
                        await Task.Delay(10000);
                        // Resume music after timer
                        await Clients.Client(game.HostConnectionId).SendAsync("ResumeMusic");
                    }
                    processedBuzzes++;
                }
                
                game.IsTimerRunning = false;
                return; // All teams processed
            }

            // Wait for current timer (10 seconds)
            await Task.Delay(10000);
            
            if (!game.IsBuzzingActive) return;

            var currentBuzzCount = game.CurrentBuzzes.Count;
            
            // Resume music after timer expires
            await Clients.Client(game.HostConnectionId).SendAsync("ResumeMusic");
            
            // Check if another team buzzed during the timer
            if (currentBuzzCount > 1 && game.BuzzedTeamIds.Count > 1)
            {
                // Another team buzzed - stop music and start their timer
                await Clients.Client(game.HostConnectionId).SendAsync("StopMusic");
                var nextBuzz = game.CurrentBuzzes[currentBuzzCount - 1];
                if (nextBuzz.Player.TeamId != null && game.Teams.TryGetValue(nextBuzz.Player.TeamId, out var nextTeam))
                {
                    await Clients.GroupExcept(game.GameCode, game.HostConnectionId)
                        .SendAsync("BuzzTimerStarted", new { teamId = nextTeam.Id, duration = 10 });
                    continue; // Continue loop to process next timer
                }
            }

            // No new buzzes - start extra time if not all teams have buzzed
            if (game.BuzzedTeamIds.Count < game.Teams.Count)
            {
                game.IsTimerRunning = false;
                game.IsExtraTimeActive = true;
                await Clients.Group(game.GameCode).SendAsync("BuzzExtraTimeStarted", new { duration = 10 });
                
                await Task.Delay(10000);
                
                if (!game.IsBuzzingActive) return;
                
                // Check if team buzzed during extra time
                if (game.CurrentBuzzes.Count > currentBuzzCount)
                {
                    // Team buzzed during extra time - their timer was already started in Buzz method
                    continue;
                }
                
                // No one buzzed during extra time - end buzzing
                game.IsBuzzingActive = false;
                game.IsTimerRunning = false;
                game.IsExtraTimeActive = false;
                await Clients.Group(game.GameCode).SendAsync("ExtendedBuzzTimeExpired");
                await Clients.Client(game.HostConnectionId).SendAsync("ExtendedBuzzTimeExpired");
                return;
            }
            
            game.IsTimerRunning = false;
            return;
        }
    }

    public async Task ResetBuzz()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        game.CurrentBuzzes.Clear();
        game.IsBuzzingActive = false;
        game.BuzzedTeamIds.Clear();
        game.IsTimerRunning = false;
        game.IsExtraTimeActive = false;

        await Clients.Group(game.GameCode).SendAsync("BuzzReset");
        await Clients.Client(game.HostConnectionId).SendAsync("BuzzCleared");
    }

    // New method for when host returns to board without full reset
    public async Task HostReturnToBoard()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        // Don't clear buzzes or change buzzing state, just notify players to return to board view
        await Clients.GroupExcept(game.GameCode, Context.ConnectionId).SendAsync("HostReturnedToBoard");
    }

    public async Task UpdateScore(string playerId, int points)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        if (game.Players.TryGetValue(playerId, out var player))
        {
            if (game.IsTeamMode && player.TeamId != null && game.Teams.TryGetValue(player.TeamId, out var team))
            {
                // Update team score
                team.Score += points;

                await Clients.Group(game.GameCode).SendAsync("ScoreUpdated", new
                {
                    playerId,
                    playerName = player.Name,
                    teamId = team.Id,
                    teamName = team.Name,
                    newScore = team.Score,
                    players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId }),
                    teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() })
                });
            }
            else
            {
                // Solo mode - update player score
                player.Score += points;

                await Clients.Group(game.GameCode).SendAsync("ScoreUpdated", new
                {
                    playerId,
                    playerName = player.Name,
                    newScore = player.Score,
                    players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId })
                });
            }
        }
    }

    public async Task StopMusic()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        await Clients.Caller.SendAsync("MusicStopped");
    }

    // Team management methods
    public async Task CreateTeam(string teamName, string teamColor)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId || !game.IsTeamMode) return;

        var team = new Team
        {
            Id = Guid.NewGuid().ToString(),
            Name = teamName,
            Color = teamColor,
            Score = 0,
            Players = new List<Player>()
        };

        game.Teams[team.Id] = team;

        await Clients.Group(game.GameCode).SendAsync("TeamCreated", new
        {
            teamId = team.Id,
            teamName = team.Name,
            teamColor = team.Color,
            teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() })
        });
    }

    public async Task JoinTeam(string teamId)
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || !game.IsTeamMode) return;

        var player = game.Players.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
        if (player == null || !game.Teams.TryGetValue(teamId, out var team)) return;

        // Remove from previous team if any
        if (player.TeamId != null && game.Teams.TryGetValue(player.TeamId, out var oldTeam))
        {
            oldTeam.Players.RemoveAll(p => p.Id == player.Id);
        }

        // Add to new team
        player.TeamId = teamId;
        team.Players.Add(player);

        await Clients.Group(game.GameCode).SendAsync("TeamsUpdated", new
        {
            teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() }),
            players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId })
        });
    }

    public async Task StartGame()
    {
        var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
        if (game == null || game.HostConnectionId != Context.ConnectionId) return;

        game.IsGameStarted = true;

        await Clients.Group(game.GameCode).SendAsync("GameStarted", new
        {
            board = game.Board,
            teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() }),
            players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId })
        });
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
                var player = game.Players.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null && player.TeamId != null && game.Teams.TryGetValue(player.TeamId, out var team))
                {
                    team.Players.RemoveAll(p => p.Id == player.Id);
                }
                
                _gameManager.RemovePlayer(Context.ConnectionId);
                await Clients.Group(game.GameCode).SendAsync("PlayersUpdated",
                    game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId }));
                
                if (game.IsTeamMode)
                {
                    await Clients.Group(game.GameCode).SendAsync("TeamsUpdated", new
                    {
                        teams = game.Teams.Values.Select(t => new { t.Id, t.Name, t.Color, t.Score, PlayerIds = t.Players.Select(p => p.Id).ToList() }),
                        players = game.Players.Values.Select(p => new { p.Id, p.Name, p.Score, p.TeamId })
                    });
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
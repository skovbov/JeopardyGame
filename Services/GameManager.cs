﻿using JeopardyGame.Models.GameModels;

namespace JeopardyGame.Services
{
    public class GameManager
    {
        private readonly Dictionary<string, Game> _games = new();
        private readonly Random _random = new();

        public Game CreateGame(string hostConnectionId, bool isTeamMode = false)
        {
            var gameCode = GenerateGameCode();
            var game = new Game
            {
                GameCode = gameCode,
                HostConnectionId = hostConnectionId,
                IsTeamMode = isTeamMode,
                IsGameStarted = false
            };

            _games[gameCode] = game;
            return game;
        }

        public Game? GetGame(string gameCode)
        {
            return _games.TryGetValue(gameCode, out var game) ? game : null;
        }

        public Game? GetGameByConnectionId(string connectionId)
        {
            return _games.Values.FirstOrDefault(g =>
                g.HostConnectionId == connectionId ||
                g.Players.Values.Any(p => p.ConnectionId == connectionId));
        }

        public bool JoinGame(string gameCode, Player player)
        {
            if (_games.TryGetValue(gameCode, out var game))
            {
                game.Players[player.Id] = player;
                return true;
            }
            return false;
        }

        public void RemovePlayer(string connectionId)
        {
            foreach (var game in _games.Values)
            {
                var playerToRemove = game.Players.Values.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (playerToRemove != null)
                {
                    game.Players.Remove(playerToRemove.Id);
                    break;
                }
            }
        }

        public void RemoveGame(string gameCode)
        {
            _games.Remove(gameCode);
        }

        private string GenerateGameCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}

using System.Reflection;
using Dystopia.Bridge;
using Dystopia.Database.Game;
using Dystopia.Hubs;
using Dystopia.Patches;
using DystopiaShared.SharedModels;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace Dystopia.Game;

public static class PolydystopiaGameManager
{
    static PolydystopiaGameManager()
    {
        PolytopiaDataManager.provider = new MyProvider();
    }

    public static async Task<bool> CreateGame(LobbyGameViewModel lobby, IPolydystopiaGameRepository gameRepository)
    {
        var bridge = new DystopiaBridge();
        var serializedGameState = bridge.CreateGame(lobby.Map());

        serializedGameState = bridge.Update(serializedGameState);

        var gameViewModel = new GameViewModel();
        gameViewModel.Id = lobby.Id;
        gameViewModel.OwnerId = lobby.OwnerId;
        gameViewModel.DateCreated = DateTime.Now; //?
        gameViewModel.DateLastCommand = DateTime.Now; //?
        gameViewModel.State = GameSessionState.Started;
        gameViewModel.GameSettingsJson = bridge.GetGameSettingsJson(serializedGameState); //TODO: Check all serialized?
        gameViewModel.InitialGameStateData = serializedGameState;
        gameViewModel.CurrentGameStateData = serializedGameState;
        gameViewModel.TimerSettings = new TimerSettings(); //??? Used?
        gameViewModel.DateCurrentTurnDeadline = DateTime.Now.AddDays(1); //TODO: Calc
        gameViewModel.GameContext = new GameContext(); //TODO?

        await gameRepository.CreateAsync(gameViewModel);

        return true;
    }

    public static async Task<bool> Resign(ResignBindingModel model, IPolydystopiaGameRepository gameRepository,
        Guid senderId)
    {
        var game = await gameRepository.GetByIdAsync(model.GameId);
        if (game == null) return false;

        var bridge = new DystopiaBridge();

        var serializedCommand = bridge.Resign(game.CurrentGameStateData, senderId.ToString());

        if (serializedCommand == null)
        {
            return false;
        }

        var commandModel = new SendCommandBindingModel
        {
            GameId = model.GameId,
            Command = new PolytopiaCommandViewModel(serializedCommand)
        };

        await SendCommand(commandModel, gameRepository, senderId);

        return true;
    }

    public static async Task<bool> SendCommand(SendCommandBindingModel commandBindingModel,
        IPolydystopiaGameRepository gameRepository, Guid senderId)
    {
        var game = await gameRepository.GetByIdAsync(commandBindingModel.GameId);

        if (game == null) return false;

        var bridge = new DystopiaBridge();
        var ended = bridge.SendCommand(commandBindingModel.Command.SerializedData, game.CurrentGameStateData,
            out var newGameState,
            out var newCommands);

        await SendCommandBack(game.Id, commandBindingModel.Command, senderId);
        foreach (var command in newCommands)
        {
            await SendCommandBack(game.Id, new PolytopiaCommandViewModel(command));
        }

        game.CurrentGameStateData = newGameState;
        game.DateLastCommand = DateTime.Now;

        if (ended)
        {
            game.State = GameSessionState.Ended;
        }

        await gameRepository.UpdateAsync(game);

        return true;
    }

    private static async Task SendCommandBack(Guid gameId, PolytopiaCommandViewModel command, Guid? senderId = null)
    {
        var commandArray = new CommandArrayViewModel();
        commandArray.GameId = gameId;
        commandArray.Commands = new List<PolytopiaCommandViewModel>()
        {
            command
        };

        if (PolytopiaHub.GameSubscribers.TryGetValue(gameId, out var gameSubscribersList))
        {
            var gameSubscribers = gameSubscribersList.Where(u => senderId == null || u.id != senderId)
                .Select(gs => gs.proxy).ToList();
            var tasks = gameSubscribers.Select(async gameSubscriber =>
            {
                try
                {
                    await gameSubscriber.SendAsync("OnCommand", commandArray);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    public static GameSummaryViewModel GetGameSummaryViewModelByGameViewModel(GameViewModel game)
    {
        var bridge = new DystopiaBridge();
        var serializedSummary = bridge.GetSummary(game.CurrentGameStateData);

        var gameSettings = JsonConvert.DeserializeObject<SharedGameSettings>(game.GameSettingsJson); // TODO DTO!

        var summary = new GameSummaryViewModel();
        summary.GameId = game.Id;
        summary.MatchmakingGameId = null;
        summary.OwnerId = game.OwnerId;
        summary.DateCreated = game.DateCreated;
        summary.DateLastCommand = game.DateLastCommand;
        summary.DateLastEndTurn = DateTime.Now.Subtract(TimeSpan.FromMinutes(10)); //TODO
        summary.DateEnded = null; //TODO
        summary.TimeLimit = 3600; //gameSettings.TimeLimit TODO
        summary.State = game.State;
        summary.Participators = new List<ParticipatorViewModel>();
        foreach (var player in gameSettings.players)
        {
            var playerData = player.Value;

            var participator = new ParticipatorViewModel()
            {
                UserId = player.Key,
                Name = playerData.Name, //TODO
                NumberOfFriends = playerData.Profile.NumFriends,
                NumberOfMultiplayerGames = playerData.Profile.NumMultiplayerGames,
                GameVersion = new List<ClientGameVersionViewModel>() { }, //TODO
                MultiplayerRating = playerData.Profile.MultiplayerRating,
                SelectedTribe = 2, //TODO
                SelectedTribeSkin = 0, //TODO
                AvatarStateData = playerData.Profile.SerializedAvatarState,
                InvitationState = PlayerInvitationState.Accepted
            };

            summary.Participators.Add(participator);
        }

        summary.Result = null; //?

        summary.GameSummaryData = serializedSummary;
        summary.GameContext = new GameContext(); //?

        return summary;
    }
}
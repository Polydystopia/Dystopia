using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Bridge;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Hubs;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace PolytopiaB2.Carrier.Game;

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

        var succ = SerializationHelpers.FromByteArray<GameState>(game.CurrentGameStateData, out GameState gameState);

        foreach (var player in gameState.PlayerStates)
        {
            if (player.AutoPlay) continue;
            if (player.AccountId != senderId) continue;

            var resignCommand = new ResignCommand(gameState.CurrentPlayer, player.Id, 0, false);

            var commandModel = new SendCommandBindingModel
            {
                GameId = model.GameId,
                Command = new PolytopiaCommandViewModel(CommandBase.ToByteArray(resignCommand, gameState.Version))
            };

            await SendCommand(commandModel, gameRepository, senderId);

            break;
        }

        return true;
    }

    public static async Task<bool> SendCommand(SendCommandBindingModel commandBindingModel,
        IPolydystopiaGameRepository gameRepository, Guid senderId)
    {
        var game = await gameRepository.GetByIdAsync(commandBindingModel.GameId);

        if (game == null) return false;

        var succ1 = CommandBase.FromByteArray(commandBindingModel.Command.SerializedData, out var cmd, out var version);

        var succ2 = SerializationHelpers.FromByteArray<GameState>(game.CurrentGameStateData, out GameState gameState);

        var currCommandCount = gameState.CommandStack.Count;

        GameStateUtils.PerformCommands(gameState, new List<CommandBase>() { cmd }, out List<CommandBase> list,
            out var events);

        await SendCommandBack(game.Id, commandBindingModel.Command, senderId);

        var newCommandsCount = gameState.CommandStack.Count - currCommandCount - 1;

        for (int i = 0; i < newCommandsCount; i++)
        {
            var command = gameState.CommandStack[gameState.CommandStack.Count - newCommandsCount + i];

            await SendCommandBack(game.Id, new PolytopiaCommandViewModel(CommandBase.ToByteArray(command, version)));
        }

        if (succ1 && succ2)
        {
            game.CurrentGameStateData = SerializationHelpers.ToByteArray<GameState>(gameState, version);
            game.DateLastCommand = DateTime.Now;

            if (gameState.CurrentState == GameState.State.Ended)
            {
                game.State = GameSessionState.Ended;
            }

            await gameRepository.UpdateAsync(game);

            return true;
        }

        return false;
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
        var succ = GameStateSummary.FromGameStateByteArray(game.CurrentGameStateData,
            out GameStateSummary stateSummary, out var gameState);

        var gameSettings = JsonConvert.DeserializeObject<GameSettings>(game.GameSettingsJson);

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
                Name = playerData.GetNameInternal(), //TODO
                NumberOfFriends = playerData.profile.numFriends,
                NumberOfMultiplayerGames = playerData.profile.numMultiplayerGames,
                GameVersion = new List<ClientGameVersionViewModel>() { }, //TODO
                MultiplayerRating = playerData.profile.multiplayerRating,
                SelectedTribe = 2, //TODO
                SelectedTribeSkin = 0, //TODO
                AvatarStateData =
                    SerializationHelpers.ToByteArray(playerData.profile.avatarState, gameState.Version),
                InvitationState = PlayerInvitationState.Accepted
            };

            summary.Participators.Add(participator);
        }

        summary.Result = null; //?

        summary.GameSummaryData = SerializationHelpers.ToByteArray(stateSummary, gameState.Version);
        summary.GameContext = new GameContext(); //?

        return summary;
    }
}
using Dystopia.Bridge;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Hubs;
using Dystopia.Patches;
using DystopiaShared.SharedModels;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace Dystopia.Managers.Game;

public class PolydystopiaGameManager : IPolydystopiaGameManager
{
    private readonly IPolydystopiaGameRepository _gameRepository;

    private readonly ILogger<PolytopiaHub> _logger;

    public PolydystopiaGameManager(IPolydystopiaGameRepository gameRepository, ILogger<PolytopiaHub> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;

        PolytopiaDataManager.provider = new MyProvider();
    }

    public async Task<bool> CreateGame(LobbyGameViewModel lobby)
    {
        var bridge = new DystopiaBridge();
        var (serializedGameState, gameSettingsJson) = bridge.CreateGame(lobby.Map());

        serializedGameState = bridge.Update(serializedGameState);

        var gameEntity = new GameEntity()
        {
            Id = lobby.Id,
            LobbyId = lobby.Id,
            OwnerId = lobby.OwnerId,
            DateCreated = DateTime.Now,
            DateLastCommand = DateTime.Now,
            State = GameSessionState.Started,
            GameSettings = gameSettingsJson,
            InitialGameStateData = serializedGameState,
            CurrentGameStateData = serializedGameState,
            TimerSettings = new TimerSettings(), //TODO
            DateCurrentTurnDeadline = DateTime.Now.AddDays(1), //TODO
            ExternalMatchId = null, //TODO
            ExternalTournamentId = null, //TODO
        };

        var gameParticipators = new List<GameParticipatorUserEntity>();
        foreach (var lobbyParticipator in lobby.Participators)
        {
            if(lobbyParticipator.InvitationState != PlayerInvitationState.Accepted) continue;

            gameParticipators.Add(new GameParticipatorUserEntity()
            {
                UserId = lobbyParticipator.UserId,
                InvitationState = PlayerInvitationState.Accepted,
                SelectedTribe = lobbyParticipator.SelectedTribe,
                SelectedTribeSkin = lobbyParticipator.SelectedTribeSkin,
                GameId = gameEntity.Id,
                //TODO others?
            });
        }
        gameEntity.Participators = gameParticipators;

        await _gameRepository.CreateAsync(gameEntity);

        return true;
    }

    public async Task<bool> Resign(ResignBindingModel model, Guid senderId)
    {
        var game = await _gameRepository.GetByIdAsync(model.GameId);
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

        await SendCommand(commandModel, senderId);

        return true;
    }

    public async Task<bool> SendCommand(SendCommandBindingModel commandBindingModel, Guid senderId)
    {
        var game = await _gameRepository.GetByIdAsync(commandBindingModel.GameId);

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

        await _gameRepository.UpdateAsync(game);

        if (PolytopiaHub.GameSummariesSubscribers.TryGetValue(game.Id, out var gameSummarySubscribersList))
        {
            var gameSummaryModel = GetGameSummaryViewModelByGameViewModel(game.ToViewModel());
            var pushReason = StateUpdateReason.ValidCommand;

            var gameSummarySubscribers = gameSummarySubscribersList.Where(u => senderId == null || u.id != senderId)
                .Select(gs => gs.proxy).ToList();
            var tasks = gameSummarySubscribers.Select(async gameSummarySubscriber =>
            {
                try
                {
                    await gameSummarySubscriber.SendAsync("OnGameSummaryUpdated", gameSummaryModel, pushReason);
                }
                catch (Exception e)
                {
                    _logger.LogDebug("Error sending game summary update to client: {exception}", e);
                }
            });

            await Task.WhenAll(tasks);
        }
        return true;
    }

    private async Task SendCommandBack(Guid gameId, PolytopiaCommandViewModel command, Guid? senderId = null)
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
                    _logger.LogDebug("Error sending new command to client: {exception}", e);
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    public GameSummaryViewModel GetGameSummaryViewModelByGameViewModel(GameViewModel game)
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
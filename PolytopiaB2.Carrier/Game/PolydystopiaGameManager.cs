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
        var t = new DystopiaBridge().CreateGame(lobby);

        var settings = new GameSettings();

        settings.ApplyLobbySettings(lobby);

        settings.players = new Dictionary<Guid, PlayerData>();

        foreach (var participatorViewModel in lobby.Participators)
        {
            if (participatorViewModel.SelectedTribe == 0) participatorViewModel.SelectedTribe = 2; //TODO: Remove later

            var humanPlayer = new PlayerData();
            humanPlayer.type = PlayerData.Type.Local;
            humanPlayer.state = PlayerData.State.Accepted;
            humanPlayer.knownTribe = true;
            humanPlayer.tribe = (TribeData.Type)participatorViewModel.SelectedTribe;
            humanPlayer.tribeMix = (TribeData.Type)participatorViewModel.SelectedTribe; //?
            humanPlayer.skinType = (SkinType)participatorViewModel.SelectedTribeSkin;
            humanPlayer.defaultName = participatorViewModel.GetNameInternal();
            humanPlayer.profile.id = participatorViewModel.UserId;
            humanPlayer.profile.SetName(participatorViewModel.GetNameInternal());

            settings.AddPlayer(humanPlayer);
        }

        foreach (var botDifficulty in lobby.Bots)
        {
            var botGuid = Guid.NewGuid();

            var botPlayer = new PlayerData();
            botPlayer.type = PlayerData.Type.Bot;
            botPlayer.state = PlayerData.State.Accepted;
            botPlayer.knownTribe = true;
            botPlayer.tribe = Enum.GetValues<TribeData.Type>().Where(t => t != TribeData.Type.None)
                .OrderBy(x => Guid.NewGuid()).First();
            ;
            botPlayer.botDifficulty = (GameSettings.Difficulties)botDifficulty;
            botPlayer.skinType = SkinType.Default; //TODO
            botPlayer.defaultName = "Bot" + botGuid;
            botPlayer.profile.id = botGuid;

            settings.AddPlayer(botPlayer);
        }

        var unused_player_states = new List<PlayerState>(); //?? Seems unused


        GameState gameState = new GameState()
        {
            Version = VersionManager.GameVersion,
            Settings = settings,
            PlayerStates = new List<PlayerState>()
        };
        for (int index = 0; index < settings.Players.Length; ++index)
        {
            PlayerData player = settings.Players[index];
            if (player.type != PlayerData.Type.Bot)
            {
                PlayerState playerState = new PlayerState()
                {
                    Id = (byte)(index + 1),
                    AccountId = player.profile.id,
                    AutoPlay = player.type == PlayerData.Type.Bot,
                    UserName = player.GetNameInternal(),
                    tribe = player.tribe,
                    tribeMix = player.tribeMix,
                    hasChosenTribe = true,
                    skinType = player.skinType
                };
                gameState.PlayerStates.Add(playerState);
                Log.Verbose("Created player: {0}", (object)playerState);
            }
            else
            {
                GameStateUtils.AddAIOpponent(gameState, GameStateUtils.GetRandomPickableTribe(gameState),
                    GameSettings.HandicapFromDifficulty(player.botDifficulty), player.skinType);
            }
        }

        GameStateUtils.SetPlayerColors(gameState);
        GameStateUtils.AddNaturePlayer(gameState);
        Log.Verbose("{0} Creating world...", (object)"Verbose");
        ushort num = (ushort)Math.Max(settings.MapSize,
            (int)MapDataExtensions.GetMinimumMapSize(gameState.PlayerCount));
        gameState.Map = new MapData(num, num);
        MapGeneratorSettings generatorSettings = settings.GetMapGeneratorSettings();
        new MapGenerator().Generate(gameState, generatorSettings);
        Log.Verbose("{0} Creating initial state for {1} players...", (object)"Verbose", (object)gameState.PlayerCount);

        foreach (PlayerState playerState3 in gameState.PlayerStates)
        {
            foreach (PlayerState playerState4 in gameState.PlayerStates)
                playerState3.aggressions[playerState4.Id] = 0;
            if (playerState3.Id != byte.MaxValue)
            {
                playerState3.Currency = 5;
                TribeData data3;
                UnitData data4;
                if (gameState.GameLogicData.TryGetData(playerState3.tribe, out data3) &&
                    gameState.GameLogicData.TryGetData(data3.startingUnit.type, out data4))
                {
                    TileData tile = gameState.Map.GetTile(playerState3.startTile);
                    UnitState unitState = ActionUtils.TrainUnitScored(gameState, playerState3, tile, data4);
                    unitState.attacked = false;
                    unitState.moved = false;
                }
            }
        }

        Log.Verbose("{0} Session created successfully", (object)"Verbose");
        gameState.CommandStack.Add((CommandBase)new StartMatchCommand((byte)1));

        Update(gameState);

        var gameViewModel = new GameViewModel();
        gameViewModel.Id = lobby.Id;
        gameViewModel.OwnerId = lobby.OwnerId;
        gameViewModel.DateCreated = DateTime.Now; //?
        gameViewModel.DateLastCommand = DateTime.Now; //?
        gameViewModel.State = GameSessionState.Started;
        gameViewModel.GameSettingsJson =
            JsonConvert.SerializeObject(gameState.Settings); //TODO: Check all serialized?
        gameViewModel.InitialGameStateData =
            SerializationHelpers.ToByteArray<GameState>(gameState, gameState.Version);
        gameViewModel.CurrentGameStateData =
            SerializationHelpers.ToByteArray<GameState>(gameState, gameState.Version);
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

    private static void Update(GameState gameState)
    {
        bool pendingCommandTrigger =
            gameState.TryGetPendingCommandTrigger(gameState.CurrentPlayer, out CommandTrigger _);
        if (gameState.ActionStack != null && gameState.ActionStack.Count > 0 && !pendingCommandTrigger)
        {
            int index = gameState.ActionStack.Count - 1;
            ActionBase action = gameState.ActionStack[index];
            if (action.IsValid(gameState))
            {
                Log.Spam("{0} Executing action ({2}):\t{1}", (object)"Spam", (object)action, (object)(index + 1));
                action.Execute(gameState);
            }
            else
                Log.Spam("{0} Action is invalid ({2}):\t{1}", (object)"Spam", (object)action, (object)(index + 1));

            gameState.ActionStack.RemoveAt(index);
            Update(gameState);
        }
        else if ((int)gameState.LastProcessedCommand < gameState.CommandStack.Count)
        {
            CommandBase command = gameState.CommandStack[(int)gameState.LastProcessedCommand++];
            Log.Spam("{0} Executing command ({2}):\t{1}", (object)"Spam", (object)command,
                (object)(gameState.CommandStack.Count - (int)gameState.LastProcessedCommand + 1));
            command.Execute(gameState);
            Update(gameState);
        }
        else
        {
            Log.Verbose("{0} Finished processing", (object)"Verbose");
            //this.StopProcessing(); TODO
        }
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
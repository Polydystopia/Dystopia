using System.Reflection;
using DystopiaShared;
using DystopiaShared.SharedModels;
using Newtonsoft.Json;
using Polytopia.Data;
using Dystopia.Bridge.Mappings;
using PolytopiaBackendBase.Game;

namespace Dystopia.Bridge;

public class DystopiaWhiteCastle : IDystopiaCastle
{
    public string GetVersion()
    {
        return VersionManager.GameVersion.ToString();
    }

    public (byte[] serializedGamestate, string gameSettingsJson) CreateGame(SharedLobbyGameViewModel lobby)
    {
        var managedLobby = lobby.Map();

        var settings = new GameSettings();

        settings.ApplyLobbySettings(managedLobby);

        settings.players = new Dictionary<Guid, PlayerData>();

        foreach (var participatorViewModel in managedLobby.Participators)
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
            SerializationHelpers.FromByteArray<AvatarState>(participatorViewModel.AvatarStateData, out var avatarState);
            humanPlayer.profile.avatarState = avatarState;

            settings.AddPlayer(humanPlayer);
        }

        foreach (var botDifficulty in managedLobby.Bots)
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

        return (SerializationHelpers.ToByteArray(gameState, gameState.Version), JsonConvert.SerializeObject(gameState.Settings.MapToShared()));
    }

    public byte[] Update(byte[] serializedGameState)
    {
        var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out var gameState);

        var updatedGameState = Update(gameState);

        return SerializationHelpers.ToByteArray(updatedGameState, updatedGameState.Version);
    }

    private class ProxyActionManager : ActionManager
    {
        public ProxyActionManager(GameState state) : base(state)
        {
        }

        public void Update()
        {
            base.Update();
        }
    }

    private GameState Update(GameState gameState)
    {
        new ProxyActionManager(gameState).Update(); //TODO check if this works
        return gameState;

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

        return gameState;
    }

    public byte[]? Resign(byte[] serializedGameState, string senderId)
    {
        var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

        foreach (var player in gameState.PlayerStates)
        {
            if (player.AutoPlay) continue;
            if (player.AccountId != new Guid(senderId)) continue;

            var resignCommand = new ResignCommand(gameState.CurrentPlayer, player.Id, 0, false);

            return CommandBase.ToByteArray(resignCommand, gameState.Version);
        }

        return null;
    }

    public bool SendCommand(byte[] serializedCommand, byte[] serializedGameState, out byte[] newGameState, out byte[][] newCommands)
    {
        var succ1 = CommandBase.FromByteArray(serializedCommand, out var cmd, out var version);

        var succ2 = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

        var currCommandCount = gameState.CommandStack.Count;

        GameStateUtils.PerformCommands(gameState, new List<CommandBase>() { cmd }, out List<CommandBase> list,
            out var events);

            var newCommandsCount = gameState.CommandStack.Count - currCommandCount - 1;
            newCommands = new byte[newCommandsCount][];
            for (int i = 0; i < newCommandsCount; i++)
            {
                var command = gameState.CommandStack[gameState.CommandStack.Count - newCommandsCount + i];
                newCommands[i] = CommandBase.ToByteArray(command, version);
            }

        newGameState = SerializationHelpers.ToByteArray(gameState, version);

        return gameState.CurrentState == GameState.State.Ended;
    }

    public bool IsPlayerInGame(string playerId, byte[] serializedGameState)
    {
        var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

        return gameState.TryGetPlayer(Guid.Parse(playerId), out _ );
    }

    public byte[] GetSummary(byte[] serializedGameState)
    {
        var succ = GameStateSummary.FromGameStateByteArray(serializedGameState,
            out GameStateSummary stateSummary, out var gameState);

        return SerializationHelpers.ToByteArray(stateSummary, gameState.Version);
    }
}
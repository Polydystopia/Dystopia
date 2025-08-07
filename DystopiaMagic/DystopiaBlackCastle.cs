using System.Text.RegularExpressions;
using DystopiaShared;
using DystopiaShared.SharedModels;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using Guid = Il2CppSystem.Guid;

namespace DystopiaMagic;

public class DystopiaBlackCastle : IDystopiaCastle
{
    public static T Run<T>(Func<T> fn)
    {
        var thread = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        try
        {
            return fn();
        }
        finally
        {
            IL2CPP.il2cpp_thread_detach(thread);
        }
    }

    public static void Run(Action fn)
    {
        var thread = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        try
        {
            fn();
        }
        finally
        {
            IL2CPP.il2cpp_thread_detach(thread);
        }
    }

    public string GetVersion()
    {
        return VersionManager.GameVersion.ToString();
    }

    public (byte[] serializedGamestate, string gameSettingsJson) CreateGame(SharedLobbyGameViewModel lobby, int gameVersion)
    {
        return Run(() =>
        {
            var nativeLobby = lobby.MapToNative();

            var settings = new GameSettings();
            settings.ApplyLobbySettings(nativeLobby);
            settings.players = new Il2CppSystem.Collections.Generic.Dictionary<Guid, PlayerData>();

            foreach (var participatorViewModel in nativeLobby.Participators)
            {
                if (participatorViewModel.SelectedTribe == 0)
                    participatorViewModel.SelectedTribe = 2; //TODO: Remove later

                var humanPlayer = new PlayerData();
                humanPlayer.type = PlayerData.Type.Local;
                humanPlayer.state = PlayerData.State.Accepted;
                humanPlayer.knownTribe = true;
                humanPlayer.tribe = (TribeData.Type)participatorViewModel.SelectedTribe;
                humanPlayer.tribeMix = (TribeData.Type)participatorViewModel.SelectedTribe; //?
                humanPlayer.skinType = (SkinType)participatorViewModel.SelectedTribeSkin;
                humanPlayer.defaultName = participatorViewModel.GetNameInternal();
                humanPlayer.profile.id = new Guid(participatorViewModel.UserId.ToString());
                humanPlayer.profile.SetName(participatorViewModel.GetNameInternal());
                SerializationHelpers.FromByteArray<AvatarState>(participatorViewModel.AvatarStateData, out var avatarState);
                humanPlayer.profile.avatarState = avatarState;

                settings.AddPlayer(humanPlayer);
            }

            foreach (var botDifficulty in nativeLobby.Bots)
            {
                var botGuid = Guid.NewGuid();

                var botPlayer = new PlayerData();
                botPlayer.type = PlayerData.Type.Bot;
                botPlayer.state = PlayerData.State.Accepted;
                botPlayer.knownTribe = true;

                var tribesEnumValues = Enum.GetValues<TribeData.Type>()
                    .Where(t => t != TribeData.Type.None && t != TribeData.Type.Nature)
                    .ToArray();
                botPlayer.tribe = tribesEnumValues[Random.Shared.Next(tribesEnumValues.Length)];

                botPlayer.botDifficulty = (BotDifficulty)botDifficulty;
                botPlayer.skinType = SkinType.Default; //TODO
                botPlayer.defaultName = "Bot" + botGuid;
                botPlayer.profile.id = botGuid;

                settings.AddPlayer(botPlayer);
            }

            var unused_player_states = new List<PlayerState>(); //?? Seems unused


            GameState gameState = new GameState()
            {
                Version = gameVersion,
                Settings = settings,
                PlayerStates = new Il2CppSystem.Collections.Generic.List<PlayerState>()
            };
            for (int index = 0; index < settings.Players.Length; ++index)
            {
                PlayerData player = settings.Players[index];
                if (player.type != PlayerData.Type.Bot)
                {
                    PlayerState playerState = new PlayerState()
                    {
                        Id = (byte)(index + 1),
                        AccountId = new Il2CppSystem.Nullable<Guid>(player.profile.id),
                        AutoPlay = player.type == PlayerData.Type.Bot,
                        UserName = player.GetNameInternal(),
                        tribe = player.tribe,
                        tribeMix = player.tribeMix,
                        hasChosenTribe = true,
                        skinType = player.skinType
                    };
                    gameState.PlayerStates.Add(playerState);
                }
                else
                {
                    GameStateUtils.AddAIOpponent(gameState, GameStateUtils.GetRandomPickableTribe(gameState),
                        GameSettings.HandicapFromDifficulty(player.botDifficulty), player.skinType);
                }
            }

            GameStateUtils.SetPlayerColors(gameState);
            GameStateUtils.AddNaturePlayer(gameState);

            ushort num = (ushort)Math.Max(settings.MapSize,
                (int)MapDataExtensions.GetMinimumMapSize(gameState.PlayerCount));
            gameState.Map = new MapData(num, num);
            MapGeneratorSettings generatorSettings = settings.GetMapGeneratorSettings();
            new MapGenerator().Generate(gameState, generatorSettings);

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

            gameState.CommandStack.Add((CommandBase)new StartMatchCommand((byte)1));

            return (SerializationHelpers.ToByteArray(gameState, gameState.Version), JsonConvert.SerializeObject(gameState.Settings.MapToShared()));
        });
    }

    public byte[] Update(byte[] serializedGameState)
    {
        return Run(() =>
        {
            var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out var gameState);

            var updatedGameState = Update(gameState);

            return SerializationHelpers.ToByteArray(updatedGameState, updatedGameState.Version);
        });
    }

    private GameState Update(GameState gameState)
    {
        var actionManager = new ActionManager(gameState);
        actionManager.Update();

        return gameState;
    }

    public byte[]? Resign(byte[] serializedGameState, string senderId)
    {
        return Run(() =>
        {
            var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

            foreach (var player in gameState.PlayerStates)
            {
                if (player.AutoPlay) continue;

                var playerId = Regex.Match(player.ToString(),
                    @"\((?<guid>[0-9A-Fa-f]{8}(?:-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12})\)\s*$"); //TODO hack bc of weird behaviour see https://github.com/Polydystopia/Dystopia/issues/26

                if (playerId.Groups["guid"].Value != senderId) continue;

                var resignCommand = new ResignCommand(gameState.CurrentPlayer, player.Id, 0, false);

                return CommandBase.ToByteArray(resignCommand, gameState.Version);
            }

            return null;
        });
    }

    public bool SendCommand(byte[] serializedCommand, byte[] serializedGameState, out byte[] newGameState,
        out byte[][] newCommands)
    {
        byte[] localGameState = null!;
        byte[][] localCommands = null!;
        bool result = Run(() =>
        {
            var succ1 = CommandBase.FromByteArray(serializedCommand, out var cmd, out var version);

            var succ2 = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

            var currCommandCount = gameState.CommandStack.Count;

            var commands = new Il2CppSystem.Collections.Generic.List<CommandBase>();
            commands.Add(cmd);

            Wrapper.PerformCommands(gameState, commands, out var list, out var events);

            var newCommandsCount = gameState.CommandStack.Count - currCommandCount - 1;
            localCommands = new byte[newCommandsCount][];
            for (int i = 0; i < newCommandsCount; i++)
            {
                var command = gameState.CommandStack[(Index)(gameState.CommandStack.Count - newCommandsCount + i)];
                localCommands[i] = CommandBase.ToByteArray((CommandBase)command, version);
            }

            localGameState = SerializationHelpers.ToByteArray(gameState, version);

            return gameState.CurrentState == GameState.State.Ended;
        });

        newGameState = localGameState;
        newCommands = localCommands;
        return result;
    }

    public bool IsPlayerInGame(string playerId, byte[] serializedGameState)
    {
        return Run(() =>
        {
            var succ = SerializationHelpers.FromByteArray<GameState>(serializedGameState, out GameState gameState);

            return gameState.TryGetPlayer(Guid.Parse(playerId), out _);
        });
    }

    public byte[] GetSummary(byte[] serializedGameState)
    {
        return Run(() =>
        {
            var succ = GameStateSummary.FromGameStateByteArray(serializedGameState,
                out GameStateSummary stateSummary, out var gameState);

            return SerializationHelpers.ToByteArray(stateSummary, gameState.Version);
        });
    }
}
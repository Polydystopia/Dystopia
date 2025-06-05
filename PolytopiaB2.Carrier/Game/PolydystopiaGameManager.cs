using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace PolytopiaB2.Carrier.Game;

public static class PolydystopiaGameManager
{
    public static readonly Dictionary<Guid, List<(Guid id, IClientProxy proxy)>> GameSubscribers;

    static PolydystopiaGameManager()
    {
        PolytopiaDataManager.provider = new MyProvider();
        GameSubscribers = new Dictionary<Guid, List<(Guid id, IClientProxy proxy)>>();
    }

    public static async Task<bool> CreateGame(LobbyGameViewModel lobby, IPolydystopiaGameRepository gameRepository)
    {
        var client = new HotseatClient();

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

        var result = await client.CreateSession(settings, unused_player_states);

        Update(client.GameState);

        if (result == CreateSessionResult.Success)
        {
            var gamestate = client.GameState;

            var gameViewModel = new GameViewModel();
            gameViewModel.Id = lobby.Id;
            gameViewModel.OwnerId = lobby.OwnerId;
            gameViewModel.DateCreated = DateTime.Now; //?
            gameViewModel.DateLastCommand = DateTime.Now; //?
            gameViewModel.State = GameSessionState.Started;
            gameViewModel.GameSettingsJson =
                JsonConvert.SerializeObject(gamestate.Settings); //TODO: Check all serialized?
            gameViewModel.InitialGameStateData =
                SerializationHelpers.ToByteArray<GameState>(gamestate, gamestate.Version);
            gameViewModel.CurrentGameStateData =
                SerializationHelpers.ToByteArray<GameState>(gamestate, gamestate.Version);
            gameViewModel.TimerSettings = new TimerSettings(); //??? Used?
            gameViewModel.DateCurrentTurnDeadline = DateTime.Now.AddDays(1); //TODO: Calc
            gameViewModel.GameContext = new GameContext(); //TODO?

            await gameRepository.CreateAsync(gameViewModel);

            return true;
        }

        return false;
    }

    public static async Task<bool> SendCommand(SendCommandBindingModel commandBindingModel,
        IPolydystopiaGameRepository gameRepository, Guid senderId)
    {
        //GameManager.Instance = new GameManager();
        //GameManager.Instance.SetHotseatClient();

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

        var gameSubscribers = GameSubscribers[gameId].Where(u => senderId == null || u.id != senderId)
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
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Patches;
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
        var client = new HotseatClient();

        var settings = new GameSettings();
        settings.players = new Dictionary<Guid, PlayerData>();

        foreach (var participatorViewModel in lobby.Participators)
        {
            var playerA = new PlayerData();
            playerA.type = PlayerData.Type.Local;
            playerA.state = PlayerData.State.Accepted;
            playerA.knownTribe = true;
            playerA.tribe = (TribeData.Type)participatorViewModel.SelectedTribe;
            playerA.tribeMix = TribeData.Type.None; //?
            playerA.skinType = (SkinType) participatorViewModel.SelectedTribeSkin;
            playerA.defaultName = participatorViewModel.GetNameInternal();
            playerA.profile.id = participatorViewModel.UserId;
            playerA.profile.SetName(participatorViewModel.GetNameInternal());
            settings.players.Add(participatorViewModel.UserId, playerA);
        }

        foreach (var botDifficulty in lobby.Bots)
        {
            var botGuid = Guid.NewGuid();

            var playerB = new PlayerData();
            playerB.type = PlayerData.Type.Bot;
            playerB.state = PlayerData.State.Accepted;
            playerB.knownTribe = true;
            playerB.tribe = Enum.GetValues<TribeData.Type>().Where(t => t != TribeData.Type.None).OrderBy(x => Guid.NewGuid()).First();;
            playerB.botDifficulty = (GameSettings.Difficulties) botDifficulty;
            playerB.skinType = SkinType.Default; //TODO
            playerB.defaultName = "Bot" + botGuid;
            playerB.profile.id = botGuid;
            settings.players.Add(botGuid, playerB);
        }

        var players = new List<PlayerState>(); //?? Seems unused

        var result = await client.CreateSession(settings, players);

        if (result == CreateSessionResult.Success)
        {
            var gamestate = client.GameState;
            
            var gameViewModel = new GameViewModel();
            gameViewModel.Id = lobby.Id;
            gameViewModel.OwnerId = lobby.OwnerId;
            gameViewModel.DateCreated = DateTime.Now; //?
            gameViewModel.DateLastCommand = DateTime.Now; //?
            gameViewModel.State = GameSessionState.Started;
            gameViewModel.GameSettingsJson = JsonConvert.SerializeObject(gamestate.Settings); //TODO: Check all serialized?
            gameViewModel.InitialGameStateData = SerializationHelpers.ToByteArray<GameState>(gamestate, gamestate.Version);
            gameViewModel.CurrentGameStateData = SerializationHelpers.ToByteArray<GameState>(gamestate, gamestate.Version);
            gameViewModel.TimerSettings = new TimerSettings(); //??? Used?
            gameViewModel.DateCurrentTurnDeadline = DateTime.Now.AddDays(1); //TODO: Calc
            gameViewModel.GameContext = new GameContext(); //TODO?
            
            await gameRepository.CreateAsync(gameViewModel);
            
            return true;
        }
        
        return false;
    }
}
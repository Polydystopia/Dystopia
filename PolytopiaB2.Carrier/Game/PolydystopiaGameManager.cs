using Polytopia.Data;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Game;

public static class PolydystopiaGameManager
{
    static PolydystopiaGameManager()
    {
        PolytopiaDataManager.provider = new MyProvider();
    }

    public static bool CreateGame(LobbyGameViewModel lobby)
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

        var result = client.CreateSession(settings, players);
        
        return true;
    }
}
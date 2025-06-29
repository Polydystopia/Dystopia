using Dystopia.Database.User;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Game.Lobby;

public static class PolydystopiaLobbyManager
{
    public static LobbyGameViewModel CreateLobby(CreateLobbyBindingModel model, PolytopiaUserViewModel ownUser)
    {
        var lobby = new LobbyGameViewModel();
        lobby.Id = Guid.NewGuid();
        lobby.UpdatedReason = LobbyUpdatedReason.Created;
        lobby.DateCreated = DateTime.Now;
        lobby.DateModified = DateTime.Now;
        lobby.Name = "Love you " + model.GameName; //TODO: Cooler names
        lobby.MapPreset = model.MapPreset;
        lobby.MapSize = model.MapSize;
        lobby.OpponentCount = model.OpponentCount;
        lobby.GameMode = model.GameMode;
        lobby.OwnerId = ownUser.PolytopiaId;
        lobby.DisabledTribes = model.DisabledTribes;
        //response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        lobby.IsPersistent = model.IsPersistent; //?
        lobby.IsSharable = true; //?
        lobby.TimeLimit = model.TimeLimit;
        lobby.ScoreLimit = model.ScoreLimit;
        lobby.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360"; //TODO ?
        //response.MatchmakingGameId = response.Id.GetHashCode(); //?
        //response.ChallengermodeGameId = response.Id; //?
        //response.StartTime = DateTime.Now; //?
        lobby.GameContext = new GameContext()
        {
            ExternalMatchId = lobby.Id, //?
            ExternalTournamentId = lobby.Id, //?
        };
        lobby.Participators = new List<ParticipatorViewModel>();

        lobby.Participators.Add(new ParticipatorViewModel()
        {
            UserId = ownUser.PolytopiaId,
            Name = ownUser.GetUniqueNameInternal(),
            NumberOfFriends = ownUser.NumFriends ?? 0,
            NumberOfMultiplayerGames = ownUser.NumMultiplayergames ?? 0,
            GameVersion = ownUser.GameVersions,
            MultiplayerRating = ownUser.MultiplayerRating ?? 0,
            SelectedTribe = model.OwnerTribe,
            SelectedTribeSkin = model.OwnerTribeSkin,
            AvatarStateData = ownUser.AvatarStateData,
            InvitationState = PlayerInvitationState.Accepted
        });


        lobby.Bots = new List<int>();

        return lobby;
    }

    public static LobbyGameViewModel CreateLobby(SubmitMatchmakingBindingModel model, PolytopiaUserViewModel ownUser)
    {
        var opponents = (short)(model.OpponentCount != 0 ? model.OpponentCount : Random.Shared.Next(2, 9));

        var mapSize = opponents switch
        {
            1 => MapSize.Tiny.ToMapWidth(),
            2 => MapSize.Small.ToMapWidth(),
            3 => MapSize.Normal.ToMapWidth(),
            >= 4 => MapSize.Large.ToMapWidth(),
            _ => 0
        };

        var gameId = Guid.NewGuid();

        var lobby = new LobbyGameViewModel();
        lobby.Id = gameId;
        lobby.UpdatedReason = LobbyUpdatedReason.Created;
        lobby.DateCreated = DateTime.Now;
        lobby.DateModified = DateTime.Now;
        lobby.Name = "Love you " + Guid.NewGuid(); //TODO: Cooler names
        lobby.MapPreset = model.MapPreset == MapPreset.None
            ? Enum.GetValues<MapPreset>().Where(x => x != MapPreset.None).OrderBy(x => Random.Shared.Next()).First()
            : model.MapPreset;
        lobby.MapSize = mapSize;
        lobby.OpponentCount = opponents;
        lobby.GameMode = model.GameMode != GameMode.None ? model.GameMode : GameMode.Domination; //TODO rdm
        lobby.OwnerId = ownUser.PolytopiaId;
        lobby.DisabledTribes = new List<int>();
        //response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        lobby.IsPersistent = true; //?
        lobby.IsSharable = false; //?
        lobby.TimeLimit = model.TimeLimit;
        lobby.ScoreLimit = model.ScoreLimit;
        lobby.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360"; //TODO ?
        lobby.MatchmakingGameId = Random.Shared.NextInt64(1000000000L, long.MaxValue);
        //response.ChallengermodeGameId = response.Id; //?
        //response.StartTime = DateTime.Now; //?
        lobby.GameContext = new GameContext()
        {
            ExternalMatchId = lobby.Id, //?
            ExternalTournamentId = lobby.Id, //?
        };
        lobby.Participators = new List<ParticipatorViewModel>();

        lobby.Bots = new List<int>();

        return lobby;
    }
}
using Dystopia.Database.Lobby;
using Dystopia.Database.User;
using Dystopia.Managers.Names;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Managers.Lobby;

public static class PolydystopiaLobbyManager
{
    public static LobbyEntity CreateLobby(CreateLobbyBindingModel model, UserEntity ownUser)
    {
        var gameId = Guid.NewGuid();

        var lobby = new LobbyEntity()
        {
            Id = gameId,
            DateCreated = DateTime.Now,
            DateModified = DateTime.Now,
            Name = LobbyNameGenerator.GenerateName(gameId.ToString()),
            MapPreset = model.MapPreset,
            MapSize = model.MapSize,
            GameMode = model.GameMode,
            OwnerId = ownUser.Id,
            DisabledTribes = model.DisabledTribes,
            TimeLimit = model.TimeLimit,
            ScoreLimit = model.ScoreLimit,
            InviteLink = "https://polydystopia.xyz/todo",
            State = GameSessionState.Lobby,
            Participators = new List<LobbyParticipatorUserEntity>()
            {
                new()
                {
                    UserId = ownUser.Id,
                    InvitationState = PlayerInvitationState.Accepted,
                    SelectedTribe = model.OwnerTribe,
                    SelectedTribeSkin = model.OwnerTribeSkin,
                }
            },
            Bots = new List<int>()
        };

        return lobby;
    }

    public static LobbyEntity CreateLobby(SubmitMatchmakingBindingModel model, UserEntity ownUser)
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

        var lobby = new LobbyEntity()
        {
            Id = gameId,
            DateCreated = DateTime.Now,
            DateModified = DateTime.Now,
            Name = LobbyNameGenerator.GenerateName(gameId.ToString()),
            MapPreset = model.MapPreset == MapPreset.None
                ? Enum.GetValues<MapPreset>().Where(x => x != MapPreset.None).OrderBy(x => Random.Shared.Next()).First()
                : model.MapPreset,
            MapSize = mapSize,
            MaxPlayers = (short)(opponents+1),
            GameMode = model.GameMode != GameMode.None ? model.GameMode : GameMode.Domination,
            OwnerId = ownUser.Id,
            DisabledTribes = new List<int>(),
            TimeLimit = model.TimeLimit,
            ScoreLimit = model.ScoreLimit,
            InviteLink = "",
            MatchmakingGameId = Random.Shared.NextInt64(1000000000L, long.MaxValue),
            Participators = new List<LobbyParticipatorUserEntity>()
            {
                new()
                {
                    UserId = ownUser.Id,
                    InvitationState = PlayerInvitationState.Invited,
                }
            },
            State = GameSessionState.Lobby,
            ExternalMatchId = gameId, //TODO ?
            ExternalTournamentId = gameId, //TODO ?
            Bots = new List<int>()
        };

        return lobby;
    }
}
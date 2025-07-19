using Dystopia.Database.Lobby;
using Dystopia.Database.User;
using Dystopia.Game.Names;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Game.Lobby;

public static class PolydystopiaLobbyManager
{
    public static LobbyEntity CreateLobby(CreateLobbyBindingModel model, UserEntity ownUser)
    {
        var lobbyId = Guid.NewGuid();
        var lobby = new LobbyEntity
        {
            Id = lobbyId,
            Name = LobbyNameGenerator.GenerateName(lobbyId.ToString()),
            StartedGameId = null,
            StartedGame = null,
            MapPreset = model.MapPreset,
            OwnerId = ownUser.PolytopiaId,
            TimeLimit = model.TimeLimit,
            ScoreLimit = model.ScoreLimit,
            MatchmakingGameId = null,
            MatchmakingGame = null,
            StartTime = null,
            Participators = new List<LobbyPlayerEntity>(),
            DisabledTribes = new List<int>(),
            Bots = new List<int>(),
            MapSize = model.MapSize,
            GameMode = model.GameMode
        };
        
        lobby.Participators.Add(new LobbyPlayerEntity
        {
            UserId = ownUser.PolytopiaId,
            User = ownUser,
            LobbyId = lobby.Id,
            Lobby = lobby,
            DateLastCommand = null,
            DateLastStartTurn = null,
            DateLastEndTurn = null,
            DateCurrentTurnDeadline = null,
            TimeBank = null,
            LastConsumedTimeBank = null,
            InvitationState = PlayerInvitationState.Accepted,
            SelectedTribe = model.OwnerTribe,
            SelectedTribeSkin = model.OwnerTribeSkin,
            AutoSkipStrikeCount = 0
        });
        
        return lobby;
    }

    public static LobbyEntity CreateLobby(SubmitMatchmakingBindingModel model, UserEntity ownUser, out short opponents)
    {
        opponents = (short)(model.OpponentCount != 0 ? model.OpponentCount : Random.Shared.Next(2, 9)); // TODO only 2,4,9

        var mapSize = model.MapSize == 0 ? opponents switch
        {
            1 => MapSize.Tiny.ToMapWidth(),
            2 => MapSize.Small.ToMapWidth(),
            3 => MapSize.Normal.ToMapWidth(),
            4 => MapSize.Large.ToMapWidth(),
            5 => MapSize.Huge.ToMapWidth(), // bc why not
            6 => MapSize.Massive.ToMapWidth(),
            >= 7 => 40, // Colossal
            _ => 0
        } : model.MapSize;
        var lobbyId = Guid.NewGuid();
        var lobby = new LobbyEntity
        {
            Id = lobbyId,
            Name = LobbyNameGenerator.GenerateName(lobbyId.ToString()),
            StartedGameId = null,
            StartedGame = null,
            MapPreset = model.MapPreset,
            OwnerId = ownUser.PolytopiaId,
            TimeLimit = model.TimeLimit,
            ScoreLimit = model.ScoreLimit,
            MatchmakingGameId = null,
            MatchmakingGame = null,
            StartTime = null,
            Participators = new List<LobbyPlayerEntity>(),
            DisabledTribes = new List<int>(),
            Bots = new List<int>(),
            MapSize = mapSize,
            GameMode = model.GameMode
        };
        
        lobby.Participators.Add(new LobbyPlayerEntity
        {
            UserId = ownUser.PolytopiaId,
            User = ownUser,
            LobbyId = lobby.Id,
            Lobby = lobby,
            DateLastCommand = null,
            DateLastStartTurn = null,
            DateLastEndTurn = null,
            DateCurrentTurnDeadline = null,
            TimeBank = null,
            LastConsumedTimeBank = null,
            InvitationState = PlayerInvitationState.Accepted,
            SelectedTribe = model.SelectedTribe,
            SelectedTribeSkin = 0, // TODO if we ever decide to support tribe skins,
            AutoSkipStrikeCount = 0
        });
        
        return lobby;
    }
}
using DystopiaShared;
using DystopiaShared.SharedModels;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using Guid = Il2CppSystem.Guid;

namespace DystopiaMagic;

public static class NativeLobbyMapping
{
    public static LobbyGameViewModel MapToNative(this SharedLobbyGameViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var lobby = new LobbyGameViewModel();
        lobby.Id = new Guid(src.Id.ToString());
        lobby.UpdatedReason = (LobbyUpdatedReason)src.UpdatedReason;
        lobby.DateCreated = src.DateCreated != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateCreated.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();
        lobby.DateModified = src.DateModified != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateModified.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();
        lobby.Name = src.Name;
        lobby.MapPreset = (MapPreset)src.MapPreset;
        lobby.MapSize = src.MapSize;
        lobby.OpponentCount = src.OpponentCount;
        lobby.GameMode = (GameMode)src.GameMode;
        lobby.OwnerId = new Guid(src.OwnerId.ToString());

        lobby.DisabledTribes = new Il2CppSystem.Collections.Generic.List<int>();
        foreach (var srcDisabledTribe in src.DisabledTribes)
        {
            lobby.DisabledTribes.Add(srcDisabledTribe);
        }

        lobby.StartedGameId = src.StartedGameId != null
            ? new Il2CppSystem.Nullable<Guid>(new Guid(src.StartedGameId.ToString()))
            : new Il2CppSystem.Nullable<Guid>();

        lobby.IsPersistent = src.IsPersistent;
        lobby.IsSharable = src.IsSharable;
        lobby.TimeLimit = src.TimeLimit;
        lobby.ScoreLimit = src.ScoreLimit;
        lobby.InviteLink = src.InviteLink;

        lobby.MatchmakingGameId = src.MatchmakingGameId != null
            ? new Il2CppSystem.Nullable<long>(src.MatchmakingGameId.Value)
            : new Il2CppSystem.Nullable<long>();
        lobby.ChallengermodeGameId = src.ChallengermodeGameId != null
            ? new Il2CppSystem.Nullable<Guid>(new Guid(src.ChallengermodeGameId.ToString()))
            : new Il2CppSystem.Nullable<Guid>();


        lobby.StartTime = src.StartTime != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.StartTime.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();

        lobby.GameContext = src.GameContext.MapToNative();

        lobby.Participators = new Il2CppSystem.Collections.Generic.List<ParticipatorViewModel>();
        foreach (var sharedParticipatorViewModel in src.Participators)
        {
            lobby.Participators.Add(MapToNative(sharedParticipatorViewModel));
        }

        lobby.Bots = new Il2CppSystem.Collections.Generic.List<int>();
        foreach (var srcBot in src.Bots)
        {
            lobby.Bots.Add(srcBot);
        }

        return lobby;
    }

    public static GameContext MapToNative(this SharedGameContext? src)
    {
        if (src is null) return null!;

        var gameContext = new GameContext();
        gameContext.ExternalTournamentId = src.ExternalTournamentId != null
            ? new Il2CppSystem.Nullable<Guid>(new Guid(src.ExternalTournamentId.ToString()))
            : new Il2CppSystem.Nullable<Guid>();

        gameContext.ExternalMatchId = src.ExternalMatchId != null
            ? new Il2CppSystem.Nullable<Guid>(new Guid(src.ExternalMatchId.ToString()))
            : new Il2CppSystem.Nullable<Guid>();

        return gameContext;
    }

    public static ParticipatorViewModel MapToNative(this SharedParticipatorViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var participator = new ParticipatorViewModel();

        participator.UserId = new Guid(src.UserId.ToString());;
        participator.Name = src.GetNameInternal();
        participator.NumberOfFriends = src.NumberOfFriends;
        participator.NumberOfMultiplayerGames = src.NumberOfMultiplayerGames;
        participator.GameVersion = new Il2CppSystem.Collections.Generic.List<ClientGameVersionViewModel>();
        foreach (var sharedClientGameVersionViewModel in src.GameVersion)
        {
            participator.GameVersion.Add(MapToNative(sharedClientGameVersionViewModel));
        }
        participator.MultiplayerRating = src.MultiplayerRating;


        participator.DateLastCommand = src.DateLastCommand != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateLastCommand.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();
        participator.DateLastStartTurn = src.DateLastStartTurn != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateLastStartTurn.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();
        participator.DateLastEndTurn = src.DateLastEndTurn != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateLastEndTurn.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();
        participator.DateCurrentTurnDeadline = src.DateCurrentTurnDeadline != null
            ? new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(
                new Il2CppSystem.DateTime(src.DateCurrentTurnDeadline.Value.Ticks))
            : new Il2CppSystem.Nullable<Il2CppSystem.DateTime>();

        participator.TimeBank = src.TimeBank.HasValue
            ? new Il2CppSystem.Nullable<Il2CppSystem.TimeSpan>(
                new Il2CppSystem.TimeSpan(src.TimeBank.Value.Ticks)
            )
            : new Il2CppSystem.Nullable<Il2CppSystem.TimeSpan>();
        participator.LastConsumedTimeBank = src.LastConsumedTimeBank.HasValue
            ? new Il2CppSystem.Nullable<Il2CppSystem.TimeSpan>(
                new Il2CppSystem.TimeSpan(src.LastConsumedTimeBank.Value.Ticks)
            )
            : new Il2CppSystem.Nullable<Il2CppSystem.TimeSpan>();


        participator.InvitationState = (PlayerInvitationState)src.InvitationState;
        participator.SelectedTribe = src.SelectedTribe;
        participator.SelectedTribeSkin = src.SelectedTribeSkin;
        participator.HasFailedParse = src.HasFailedParse;
        participator.AvatarStateData = src.AvatarStateData != null
            ? (byte[])src.AvatarStateData.Clone()
            : null;
        participator.AutoSkipStrikeCount = src.AutoSkipStrikeCount;
        return participator;
    }

    public static ClientGameVersionViewModel MapToNative(this SharedClientGameVersionViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var gameVersion = new ClientGameVersionViewModel();

        gameVersion.Platform = (Platform)src.Platform;
        //gameVersion.DeviceId = src.DeviceId; TODO: does not exist anymore?
        gameVersion.GameVersion = src.GameVersion;

        return gameVersion;
    }
}
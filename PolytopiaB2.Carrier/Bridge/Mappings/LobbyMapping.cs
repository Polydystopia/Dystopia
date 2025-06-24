using DystopiaShared;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

public static class LobbyMapping
{
    public static SharedLobbyGameViewModel Map(this LobbyGameViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedLobbyGameViewModel
        {
            Id = src.Id,
            UpdatedReason = (SharedLobbyUpdatedReason)src.UpdatedReason,
            DateCreated = src.DateCreated,
            DateModified = src.DateModified,
            Name = src.Name,
            MapPreset = (SharedMapPreset)src.MapPreset,
            MapSize = src.MapSize,
            OpponentCount = src.OpponentCount,
            GameMode = (SharedGameMode)src.GameMode,
            OwnerId = src.OwnerId,
            DisabledTribes = new List<int>(src.DisabledTribes),
            StartedGameId = src.StartedGameId,
            IsPersistent = src.IsPersistent,
            IsSharable = src.IsSharable,
            TimeLimit = src.TimeLimit,
            ScoreLimit = src.ScoreLimit,
            InviteLink = src.InviteLink,
            MatchmakingGameId = src.MatchmakingGameId,
            ChallengermodeGameId = src.ChallengermodeGameId,
            StartTime = src.StartTime,
            GameContext = Map(src.GameContext),
            Participators = src.Participators
                .Select(p => Map(p))
                .ToList(),
            Bots = new List<int>(src.Bots)
        };
    }

    public static LobbyGameViewModel Map(this SharedLobbyGameViewModel src)
    {

        if (src is null) throw new ArgumentNullException(nameof(src));

        return new LobbyGameViewModel
        {
            Id = src.Id,
            UpdatedReason = (LobbyUpdatedReason)src.UpdatedReason,
            DateCreated = src.DateCreated,
            DateModified = src.DateModified,
            Name = src.Name,
            MapPreset = (MapPreset)src.MapPreset,
            MapSize = src.MapSize,
            OpponentCount = src.OpponentCount,
            GameMode = (GameMode)src.GameMode,
            OwnerId = src.OwnerId,
            DisabledTribes = new List<int>(src.DisabledTribes),
            StartedGameId = src.StartedGameId,
            IsPersistent = src.IsPersistent,
            IsSharable = src.IsSharable,
            TimeLimit = src.TimeLimit,
            ScoreLimit = src.ScoreLimit,
            InviteLink = src.InviteLink,
            MatchmakingGameId = src.MatchmakingGameId,
            ChallengermodeGameId = src.ChallengermodeGameId,
            StartTime = src.StartTime,
            GameContext = Map(src.GameContext),
            Participators = src.Participators
                .Select(Map)
                .ToList(),
            Bots = new List<int>(src.Bots)
        };
    }

    public static SharedGameContext Map(this GameContext? src)
    {
        if (src is null) return null!;
        return new SharedGameContext
        {
            ExternalTournamentId = src.ExternalTournamentId,
            ExternalMatchId = src.ExternalMatchId
        };
    }

    public static GameContext Map(this SharedGameContext src)
    {
        if (src is null) return null!;
        return new GameContext
        {
            ExternalTournamentId = src.ExternalTournamentId,
            ExternalMatchId = src.ExternalMatchId
        };
    }

    public static SharedParticipatorViewModel Map(this ParticipatorViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedParticipatorViewModel
        {
            UserId = src.UserId,
            Name = src.GetNameInternal(),
            NumberOfFriends = src.NumberOfFriends,
            NumberOfMultiplayerGames = src.NumberOfMultiplayerGames, GameVersion = src.GameVersion
                .Select(v => Map(v))
                .ToList(),
            MultiplayerRating = src.MultiplayerRating,
            DateLastCommand = src.DateLastCommand,
            DateLastStartTurn = src.DateLastStartTurn,
            DateLastEndTurn = src.DateLastEndTurn,
            DateCurrentTurnDeadline = src.DateCurrentTurnDeadline,
            TimeBank = src.TimeBank,
            LastConsumedTimeBank = src.LastConsumedTimeBank,
            InvitationState = (SharedPlayerInvitationState)src.InvitationState,
            SelectedTribe = src.SelectedTribe,
            SelectedTribeSkin = src.SelectedTribeSkin,
            HasFailedParse = src.HasFailedParse,
            AvatarStateData = src.AvatarStateData,
            AutoSkipStrikeCount = src.AutoSkipStrikeCount
        };
    }

    public static ParticipatorViewModel Map(this SharedParticipatorViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new ParticipatorViewModel
        {
            UserId = src.UserId,
            Name = src.GetNameInternal(),
            NumberOfFriends = src.NumberOfFriends,
            NumberOfMultiplayerGames = src.NumberOfMultiplayerGames, GameVersion = src.GameVersion
                .Select(Map)
                .ToList(),
            MultiplayerRating = src.MultiplayerRating,
            DateLastCommand = src.DateLastCommand,
            DateLastStartTurn = src.DateLastStartTurn,
            DateLastEndTurn = src.DateLastEndTurn,
            DateCurrentTurnDeadline = src.DateCurrentTurnDeadline,
            TimeBank = src.TimeBank,
            LastConsumedTimeBank = src.LastConsumedTimeBank,
            InvitationState = (PlayerInvitationState)src.InvitationState,
            SelectedTribe = src.SelectedTribe,
            SelectedTribeSkin = src.SelectedTribeSkin,
            HasFailedParse = src.HasFailedParse,
            AvatarStateData = src.AvatarStateData,
            AutoSkipStrikeCount = src.AutoSkipStrikeCount
        };
    }

    public static SharedClientGameVersionViewModel Map(this ClientGameVersionViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new SharedClientGameVersionViewModel
        {
            Platform = (SharedPlatform)src.Platform,
            DeviceId = src.DeviceId,
            GameVersion = src.GameVersion
        };
    }

    public static ClientGameVersionViewModel Map(this SharedClientGameVersionViewModel src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new ClientGameVersionViewModel
        {
            Platform = (Platform)src.Platform,
            DeviceId = src.DeviceId,
            GameVersion = src.GameVersion
        };
    }
}
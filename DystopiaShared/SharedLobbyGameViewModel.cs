namespace DystopiaShared;

public class SharedLobbyGameViewModel
{
    public Guid Id { get; set; }

    public SharedLobbyUpdatedReason UpdatedReason { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Name { get; set; }

    public SharedMapPreset MapPreset { get; set; }

    public int MapSize { get; set; }

    public short OpponentCount { get; set; }

    public SharedGameMode GameMode { get; set; }

    public Guid OwnerId { get; set; }

    public List<int> DisabledTribes { get; set; }

    public Guid? StartedGameId { get; set; }

    public bool IsPersistent { get; set; }

    public bool IsSharable { get; set; }

    public int TimeLimit { get; set; }

    public int ScoreLimit { get; set; }

    public string InviteLink { get; set; }

    public long? MatchmakingGameId { get; set; }

    public Guid? ChallengermodeGameId { get; set; }

    public DateTime? StartTime { get; set; }

    public SharedGameContext GameContext { get; set; }

    public List<SharedParticipatorViewModel> Participators { get; set; }

    public List<int> Bots { get; set; }
}

public enum SharedLobbyUpdatedReason
{
    Unknown,
    Created,
    Get,
    UpdatedSettings,
    ActivatedLinkInvitations,
    PlayerRespondedToInvitation,
    PlayerChangedTribe,
    PlayerLeftDueToDisconnect,
    Deleted,
    PlayerLeftByRequest,
    PlayersInvited,
    PlayersKicked,
    DeleteUser,
}

public enum SharedMapPreset
{
    None,
    Dryland,
    Lakes,
    Continents,
    Archipelago,
    WaterWorld,
    Pangea,
}

public enum SharedGameMode
{
    None,
    Perfection,
    Domination,
    Glory,
    Might,
    Custom,
    Sandbox,
    Tutorial,
}

public class SharedGameContext
{
    public Guid? ExternalTournamentId { get; set; }

    public Guid? ExternalMatchId { get; set; }
}

public class SharedParticipatorViewModel
{
    public Guid UserId { get; set; }

    public string Name { private get; set; }

    public int NumberOfFriends { get; set; }

    public int NumberOfMultiplayerGames { get; set; }

    public List<SharedClientGameVersionViewModel> GameVersion { get; set; }

    public int MultiplayerRating { get; set; }

    public DateTime? DateLastCommand { get; set; }

    public DateTime? DateLastStartTurn { get; set; }

    public DateTime? DateLastEndTurn { get; set; }

    public DateTime? DateCurrentTurnDeadline { get; set; }

    public TimeSpan? TimeBank { get; set; }

    public TimeSpan? LastConsumedTimeBank { get; set; }

    public SharedPlayerInvitationState InvitationState { get; set; }

    public int SelectedTribe { get; set; }

    public int SelectedTribeSkin { get; set; }

    public bool HasFailedParse { get; set; }

    public byte[] AvatarStateData { get; set; }

    public int AutoSkipStrikeCount { get; set; }

    public string GetNameInternal() => this.Name;
}

public enum SharedPlayerInvitationState
{
    Unknown,
    Invited,
    Accepted,
    Declined,
    Resigned,
    Done,
}

public class SharedClientGameVersionViewModel
{
    public SharedPlatform Platform { get; set; }

    public string DeviceId { get; set; }

    public int GameVersion { get; set; }
}

public enum SharedPlatform
{
    Unknown,
    None,
    Steam,
    Android,
    Ios,
    Tesla,
    AndroidAlpha,
    IosAlpha,
    AppleArcade,
    NintendoSwitchWithoutMultiplayer,
    NintendoSwitchWithMultiplayer,
}
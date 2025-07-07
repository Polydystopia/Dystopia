namespace DystopiaShared.SharedModels;

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
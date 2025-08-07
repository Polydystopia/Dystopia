using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Database.Lobby;

public class LobbyEntity
{
    [Key] public Guid Id { get; init; }

    public virtual GameEntity Game { get; set; } = null!;

    public DateTime DateCreated { get; init; }
    public DateTime? DateModified { get; set; }

    [MaxLength(32)] public string Name { get; init; }

    public MapPreset MapPreset { get; init; }
    public int MapSize { get; init; }
    public GameMode GameMode { get; init; }

    public Guid OwnerId { get; set; }
    public virtual UserEntity Owner { get; init; } = null!;

    public GameSessionState State { get; set; }

    public List<int>? DisabledTribes { get; set; }

    public int TimeLimit { get; init; }
    public int ScoreLimit { get; init; }

    public string InviteLink { get; init; }

    public long? MatchmakingGameId { get; set; }

    public Guid? ChallengermodeGameId { get; set; }

    public DateTime? StartTime { get; set; }

    public Guid? ExternalTournamentId { get; init; }

    public Guid? ExternalMatchId { get; init; }

    public short MaxPlayers { get; init; }

    public virtual ICollection<LobbyParticipatorUserEntity> Participators { get; set; }
        = new List<LobbyParticipatorUserEntity>();

    public List<int> Bots { get; set; }

    public PlayerInvitationState? GetInvitationStateForPlayer(Guid userId)
    {
        return Participators.First(participator => participator.UserId == userId)?.InvitationState;
    }
}

public static class LobbyGameMappingExtensions
{
    public static LobbyGameViewModel ToViewModel(this LobbyEntity e)
    {
        return new LobbyGameViewModel
        {
            Id = e.Id,
            StartedGameId = e.Game?.Id,
            DateCreated = e.DateCreated,
            DateModified = e.DateModified,
            Name = e.Name,
            MapPreset = e.MapPreset,
            MapSize = e.MapSize,
            OpponentCount = (short)(e.MaxPlayers-1),
            GameMode = e.GameMode,
            OwnerId = e.OwnerId,
            DisabledTribes = e.DisabledTribes,
            IsPersistent = true,
            IsSharable = true,
            TimeLimit = e.TimeLimit,
            ScoreLimit = e.ScoreLimit,
            InviteLink = e.InviteLink,
            MatchmakingGameId = e.MatchmakingGameId,
            ChallengermodeGameId = e.ChallengermodeGameId,
            StartTime = e.StartTime,
            GameContext = new GameContext()
                { ExternalMatchId = e.ExternalMatchId, ExternalTournamentId = e.ExternalTournamentId },
            Participators = e.Participators.ToViewModels(),
            Bots = e.Bots
        };
    }
}

public static class LobbyCollectionMappingExtensions
{
    public static List<LobbyGameViewModel> ToViewModels(this IEnumerable<LobbyEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<LobbyGameViewModel>();
}

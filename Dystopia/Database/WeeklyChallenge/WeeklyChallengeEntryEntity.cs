using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Database.WeeklyChallenge.League;

namespace Dystopia.Database.WeeklyChallenge;

public class WeeklyChallengeEntryEntity
{
    [Key]
    public int Id { get; set; }

    public Guid WeeklyChallengeId { get; set; }
    public virtual WeeklyChallengeEntity WeeklyChallenge { get; init; } = null!;

    public Guid LeagueId { get; set; }
    public virtual LeagueEntity League { get; init; } = null!;

    public Guid UserId { get; set; }
    public virtual UserEntity User { get; init; } = null!;

    public Guid GameId { get; set; }
    protected virtual GameEntity Game { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int Score { get; set; }

    public bool HasFinished { get; set; }
    public bool HasReplay { get; set; }
    public bool IsValid { get; set; }
}
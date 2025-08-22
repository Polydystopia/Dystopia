using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Database.WeeklyChallenge.League;
using Dystopia.Models.WeeklyChallenge;

namespace Dystopia.Database.WeeklyChallenge;

public class WeeklyChallengeEntryEntity
{
    [Key] public int Id { get; set; }

    public int WeeklyChallengeId { get; set; }
    public virtual WeeklyChallengeEntity WeeklyChallenge { get; init; } = null!;

    public int Day { get; set; }

    public int LeagueId { get; set; }
    public virtual LeagueEntity League { get; init; } = null!;

    public Guid UserId { get; set; }
    public virtual UserEntity User { get; init; } = null!;

    public Guid GameId { get; set; }
    public virtual GameEntity Game { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public int Score { get; set; }

    public bool HasFinished { get; set; }
    public bool HasReplay { get; set; }
    public bool IsValid { get; set; }
}

public static class WeeklyChallengeEntryMappingExtensions
{
    public static DystopiaWeeklyChallengeEntryViewModel ToViewModel(this WeeklyChallengeEntryEntity l)
    {
        return new DystopiaWeeklyChallengeEntryViewModel
        {
            PolytopiaUserId = l.UserId,
            PolytopiaGameDataId = l.Game?.Id,
            DateCreated = l.DateCreated,
            Score = l.Score,
            HasFinished = l.HasFinished,
            HasReplay = l.HasReplay,
            IsValid = l.IsValid
        };
    }

    public static List<DystopiaWeeklyChallengeEntryViewModel> ToViewModels(
        this IEnumerable<WeeklyChallengeEntryEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<DystopiaWeeklyChallengeEntryViewModel>();
}
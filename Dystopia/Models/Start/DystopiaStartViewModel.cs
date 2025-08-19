using Dystopia.Models.League;

namespace Dystopia.Models.Start;

public class DystopiaStartViewModel : StartViewModel
{
    public required List<LeagueViewModel> LeagueViewModels { get; set; }
    public required int LeagueId { get; set; }
    public DateTime? LastSeenWeeklyChallengeDate { get; set; }
    public DateTime? LastWeeklyChallengeEntryDate { get; set; }
}
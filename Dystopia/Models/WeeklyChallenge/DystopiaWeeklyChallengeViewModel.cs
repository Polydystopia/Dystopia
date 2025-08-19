using Dystopia.Models.WeeklyChallenge.League;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

namespace Dystopia.Models.WeeklyChallenge;

public class DystopiaWeeklyChallengeViewModel : IServerResponseData
{
    public DateTime CurrentServerTime { get; set; }
    public int Week { get; set; }
    public DystopiaWeeklyChallengeTemplateMetadataViewModel WeeklyChallengeTemplateMetadataViewModel { get; set; }
    public bool HasPersonalData { get; set; }
    public int LeagueId { get; set; }
    public Dictionary<int, List<DystopiaWeeklyChallengeHighscoreViewModel>> WeeklyChallengeHighscoreViewModels { get; set; }
    public List<DystopiaLeagueHighscoreViewModel> LeagueHighscoreViewModels { get; set; }
    public List<DystopiaWeeklyChallengeEntryViewModel> WeeklyChallengeEntryViewModels { get; set; }
    public int Rank { get; set; } // -1 when no rank
    public DystopiaPromotionState PromotionState { get; set; }
}
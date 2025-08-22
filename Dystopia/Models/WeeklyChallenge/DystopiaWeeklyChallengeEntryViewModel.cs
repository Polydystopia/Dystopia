namespace Dystopia.Models.WeeklyChallenge;

public class DystopiaWeeklyChallengeEntryViewModel
{
    public Guid PolytopiaUserId { get; set; }
    public Guid? PolytopiaGameDataId { get; set; }
    public DateTime DateCreated { get; set; }
    public int Score { get; set; }
    public bool HasFinished { get; set; }
    public bool HasReplay { get; set; }
    public bool IsValid { get; set; }
}
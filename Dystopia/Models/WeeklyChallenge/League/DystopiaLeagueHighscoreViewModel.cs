namespace Dystopia.Models.WeeklyChallenge.League;

public class DystopiaLeagueHighscoreViewModel
{
    public int LeagueId { get; set; }
    public int ParticipantCount { get; set; }
    public List<DystopiaWeeklyChallengeHighscoreViewModel> Highscores { get; set; }
}
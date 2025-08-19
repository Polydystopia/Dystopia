namespace Dystopia.Models.WeeklyChallenge.League;

public class LeagueViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LocalizationKey { get; set; }
    public int PrimaryColor { get; set; }
    public int SecondaryColor { get; set; }
    public int TertiaryColor { get; set; }
    public float PromotionRate { get; set; }
    public float DemotionRate { get; set; }
    public bool IsFriendsLeague { get; set; }
    public bool IsEntry { get; set; }
}
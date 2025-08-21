using System.ComponentModel.DataAnnotations;
using Dystopia.Database.User;
using Dystopia.Models.WeeklyChallenge.League;

namespace Dystopia.Database.WeeklyChallenge.League;

public class LeagueEntity
{
    [Key]
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

    public virtual ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}

public static class LobbyGameMappingExtensions
{
    public static DystopiaLeagueViewModel ToViewModel(this LeagueEntity l)
    {
        return new DystopiaLeagueViewModel
        {
            Id = l.Id,
            Name = l.Name,
            LocalizationKey = l.LocalizationKey,
            PrimaryColor = l.PrimaryColor,
            SecondaryColor = l.SecondaryColor,
            TertiaryColor = l.TertiaryColor,
            PromotionRate = l.PromotionRate,
            DemotionRate = l.DemotionRate,
            IsFriendsLeague = l.IsFriendsLeague,
            IsEntry = l.IsEntry
        };
    }
}

public static class LeagueCollectionMappingExtensions
{
    public static List<DystopiaLeagueViewModel> ToViewModels(this IEnumerable<LeagueEntity>? source) =>
        source?.Select(e => e.ToViewModel()).ToList() ?? new List<DystopiaLeagueViewModel>();
}
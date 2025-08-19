using System.ComponentModel.DataAnnotations;
using Dystopia.Models.Skin;
using Polytopia.Data;

namespace Dystopia.Database.WeeklyChallenge;

public class WeeklyChallengeEntity
{
    [Key] public int Id { get; set; }

    public int Week { get; set; }

    public string Name { get; set; }

    public TribeData.Type Tribe { get; set; }
    public DystopiaSkinType SkinType { get; set; }

    public int GameVersion { get; set; }

    public string DiscordLink { get; set; }
}
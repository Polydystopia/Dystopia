using System.Reflection;
using Dystopia.Database;
using Dystopia.Database.News;
using Dystopia.Database.WeeklyChallenge.League;
using Microsoft.EntityFrameworkCore;

namespace Dystopia.Services.Database;

public class DbSeederService(PolydystopiaDbContext context) : IDbSeederService
{
    public async Task SeedAsync()
    {
        await SeedLeaguesAsync();
        await SeedNewsAsync();
    }

    private async Task SeedLeaguesAsync()
    {
        if (await context.Leagues.AnyAsync()) return;

        var leagues = new[]
        {
            new LeagueEntity
            {
                Name = "Entry League",
                LocalizationKey = "league.name.entry",
                PrimaryColor = 0x00B4FE,
                SecondaryColor = 0xA3E0FE,
                TertiaryColor = 0x49BE00,
                PromotionRate = 0.5f,
                DemotionRate = 0.0f,
                IsFriendsLeague = false,
                IsEntry = true
            },
            new LeagueEntity
            {
                Name = "Bronze League",
                LocalizationKey = "league.name.bronze",
                PrimaryColor = 0xC74601,
                SecondaryColor = 0xFDB219,
                TertiaryColor = 0xBC5D00,
                PromotionRate = 0.35f,
                DemotionRate = 0.0f,
                IsFriendsLeague = false,
                IsEntry = false
            },
            new LeagueEntity
            {
                Name = "Silver League",
                LocalizationKey = "league.name.silver",
                PrimaryColor = 0xA9C1C9,
                SecondaryColor = 0x5390A1,
                TertiaryColor = 0x9EC9DA,
                PromotionRate = 0.35f,
                DemotionRate = 0.35f,
                IsFriendsLeague = false,
                IsEntry = false
            },
            new LeagueEntity
            {
                Name = "Gold League",
                LocalizationKey = "league.name.gold",
                PrimaryColor = 0xFEEA79,
                SecondaryColor = 0xCE8D00,
                TertiaryColor = 0xFFA12C,
                PromotionRate = 0.00f,
                DemotionRate = 0.35f,
                IsFriendsLeague = false,
                IsEntry = false
            },

            new LeagueEntity
            {
                Name = "Friends",
                LocalizationKey = "league.name.friends",
                PrimaryColor = 0x606060,
                SecondaryColor = 0x232323,
                TertiaryColor = 0xEBEBEB,
                PromotionRate = 0.0f,
                DemotionRate = 0.0f,
                IsFriendsLeague = true,
                IsEntry = false
            }
        };

        await context.Leagues.AddRangeAsync(leagues);
        await context.SaveChangesAsync();
    }

    private async Task SeedNewsAsync()
    {
        if (await context.News.AnyAsync()) return;

        var news = new[]
        {
            new NewsEntity
            {
                NewsType = NewsType.SystemMessage,
                Body = "Welcome to Polydystopia!\n\nThis project is WIP so you will experience bugs.\n\nDiscord\njuli.gg\nhttps://discord.gg/rtwgWTzxWy\n\n\nMade with <3",
                CreatedAt = DateTime.Parse("01.01.2025"),
                IsActive = true
            },
            new NewsEntity
            {
                NewsType = NewsType.News,
                Body = $"v{Assembly.GetExecutingAssembly().GetName().Version}\nFeel free to report bugs at our discord or Github.",
                Link = "https://discord.gg/rtwgWTzxWy",
                Image = "https://avatars.githubusercontent.com/u/120461041",
                CreatedAt = DateTime.Parse("01.01.2025"),
                IsActive = true
            }
        };

        await context.News.AddRangeAsync(news);
        await context.SaveChangesAsync();
    }
}
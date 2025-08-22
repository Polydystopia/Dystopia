using System.Globalization;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Models.Skin;
using Polytopia.Data;

namespace Dystopia.Services.WeeklyChallenge;

public class WeeklyChallengeSchedulerService(
    IServiceScopeFactory scopeFactory,
    ILogger<WeeklyChallengeSchedulerService> logger)
    : BackgroundService
{
    private readonly Random _random = new();

    private static readonly TribeData.Type[] AvailableTribes = Enum.GetValues<TribeData.Type>()
        .Where(val => val != TribeData.Type.None && val != TribeData.Type.Nature).ToArray();

    private static readonly DystopiaSkinType[] AvailableSkins =
        Enum.GetValues<DystopiaSkinType>().Where(val => val == DystopiaSkinType.Default)
            .ToArray(); // TODO

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureCurrentWeekChallenge();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextSunday = GetNextSundayUtc(now);
            var delay = nextSunday - now;

            logger.LogInformation("Next weekly challenge creation scheduled for {NextSunday} UTC (in {Delay})",
                nextSunday, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                await CreateNextWeekChallenge();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during weekly challenge creation. Service will continue.");
            }
        }
    }

    private async Task EnsureCurrentWeekChallenge()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IWeeklyChallengeRepository>();

            var now = DateTime.UtcNow;
            var currentWeekComposite = GetCompositeWeekNumber(now);
            var existingChallenge = await repository.GetByWeekAsync(currentWeekComposite);

            if (existingChallenge == null)
            {
                logger.LogInformation("No challenge found for current week {Week}. Creating one.",
                    currentWeekComposite);
                await CreateChallengeForWeek(repository, currentWeekComposite);
            }
            else
            {
                logger.LogInformation("Challenge already exists for current week {Week}: {Name}",
                    currentWeekComposite, existingChallenge.Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while ensuring current week challenge exists");
        }
    }

    private async Task CreateNextWeekChallenge()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IWeeklyChallengeRepository>();

            var nextWeekDate = DateTime.UtcNow.AddDays(7);
            var nextWeekComposite = GetCompositeWeekNumber(nextWeekDate);
            var existingChallenge = await repository.GetByWeekAsync(nextWeekComposite);

            if (existingChallenge == null)
            {
                logger.LogInformation("Creating weekly challenge for week {Week}", nextWeekComposite);
                await CreateChallengeForWeek(repository, nextWeekComposite);
            }
            else
            {
                logger.LogInformation("Challenge already exists for week {Week}: {Name}",
                    nextWeekComposite, existingChallenge.Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during next week challenge creation");
        }
    }

    private async Task CreateChallengeForWeek(IWeeklyChallengeRepository repository, int compositeWeek)
    {
        var tribe = AvailableTribes[_random.Next(AvailableTribes.Length)];
        var skin = AvailableSkins[_random.Next(AvailableSkins.Length)];

        var year = compositeWeek / 100;
        var weekOfYear = compositeWeek % 100;

        var challenge = new WeeklyChallengeEntity
        {
            Week = compositeWeek,
            Name = $"Week {weekOfYear}/{year} - {tribe} Challenge",
            Tribe = tribe,
            SkinType = skin,
            GameVersion = 114, //TODO
            DiscordLink = "https://discord.gg/rtwgWTzxWy"
        };

        await repository.CreateAsync(challenge);
        logger.LogInformation(
            "Created weekly challenge for week {CompositeWeek} ({WeekOfYear}/{Year}): {Name} ({Tribe}, {Skin})",
            compositeWeek, weekOfYear, year, challenge.Name, tribe, skin);
    }

    private static DateTime GetNextSundayUtc(DateTime fromDate)
    {
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)fromDate.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && fromDate.TimeOfDay > TimeSpan.Zero)
        {
            daysUntilSunday = 7;
        }

        return fromDate.Date.AddDays(daysUntilSunday);
    }

    public static int GetCompositeWeekNumber(DateTime date)
    {
        var year = ISOWeek.GetYear(date);
        var week = ISOWeek.GetWeekOfYear(date);
        return year * 100 + week;
    }
}
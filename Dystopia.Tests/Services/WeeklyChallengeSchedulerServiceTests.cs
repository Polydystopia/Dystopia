using System.Globalization;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Models.Skin;
using Dystopia.Services.WeeklyChallenge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Polytopia.Data;
using Xunit;

namespace Dystopia.Tests.Services;

public class WeeklyChallengeSchedulerServiceTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IWeeklyChallengeRepository> _repositoryMock;
    private readonly Mock<ILogger<WeeklyChallengeSchedulerService>> _loggerMock;

    public WeeklyChallengeSchedulerServiceTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _repositoryMock = new Mock<IWeeklyChallengeRepository>();
        _loggerMock = new Mock<ILogger<WeeklyChallengeSchedulerService>>();

        // Setup the service scope chain
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWeeklyChallengeRepository)))
            .Returns(_repositoryMock.Object);
    }

    private WeeklyChallengeSchedulerService CreateService()
    {
        return new WeeklyChallengeSchedulerService(_serviceScopeFactoryMock.Object, _loggerMock.Object);
    }

    private WeeklyChallengeEntity CreateWeeklyChallengeEntity(
        int week = 202401,
        string name = "Test Challenge",
        TribeData.Type tribe = TribeData.Type.Xinxi,
        DystopiaSkinType skinType = DystopiaSkinType.Default,
        int gameVersion = 114,
        string discordLink = "https://discord.gg/test")
    {
        return new WeeklyChallengeEntity
        {
            Week = week,
            Name = name,
            Tribe = tribe,
            SkinType = skinType,
            GameVersion = gameVersion,
            DiscordLink = discordLink
        };
    }

    #region AI

    [Fact]
    public void GetCompositeWeekNumber_ReturnsCorrectComposite()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 8); // Week 2 of 2024

        // Act
        var result = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        var expectedWeek = ISOWeek.GetWeekOfYear(testDate);
        var expectedYear = ISOWeek.GetYear(testDate);
        var expected = expectedYear * 100 + expectedWeek;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetCompositeWeekNumber_HandlesYearBoundaryCorrectly()
    {
        // Arrange - January 1, 2024 is week 1 of 2024 in ISO
        var testDate = new DateTime(2024, 1, 1);

        // Act
        var result = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        Assert.Equal(202401, result);
    }

    [Fact]
    public void GetCompositeWeekNumber_HandlesLastWeekOfYear()
    {
        // Arrange - December 30, 2024 should be week 52 of 2024
        var testDate = new DateTime(2024, 12, 30);

        // Act
        var result = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        var expectedWeek = ISOWeek.GetWeekOfYear(testDate);
        var expectedYear = ISOWeek.GetYear(testDate);
        var expected = expectedYear * 100 + expectedWeek;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetCompositeWeekNumber_HandlesLeapYear()
    {
        // Arrange - February 29, 2024 (leap year)
        var testDate = new DateTime(2024, 2, 29);

        // Act
        var result = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        var expectedWeek = ISOWeek.GetWeekOfYear(testDate);
        var expectedYear = ISOWeek.GetYear(testDate);
        var expected = expectedYear * 100 + expectedWeek;
        Assert.Equal(expected, result);
        Assert.True(result > 202400 && result < 202500); // Should be in 2024
    }

    [Fact]
    public async Task EnsureCurrentWeekChallenge_WhenChallengeExists_DoesNotCreate()
    {
        // Arrange
        var service = CreateService();
        var existingChallenge = CreateWeeklyChallengeEntity();
        var currentWeek = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(DateTime.UtcNow);
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(currentWeek))
            .ReturnsAsync(existingChallenge);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start
        await service.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByWeekAsync(currentWeek), Times.Once);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<WeeklyChallengeEntity>()), Times.Never);
    }

    [Fact]
    public async Task EnsureCurrentWeekChallenge_WhenChallengeDoesNotExist_CreatesOne()
    {
        // Arrange
        var service = CreateService();
        var currentWeek = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(DateTime.UtcNow);
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(currentWeek))
            .ReturnsAsync((WeeklyChallengeEntity)null);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<WeeklyChallengeEntity>()))
            .ReturnsAsync((WeeklyChallengeEntity challenge) => challenge);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start
        await service.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByWeekAsync(currentWeek), Times.Once);
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<WeeklyChallengeEntity>(c => 
            c.Week == currentWeek && 
            !string.IsNullOrEmpty(c.Name) &&
            c.GameVersion > 0)), Times.Once);
    }

    [Fact]
    public async Task CreateChallengeForWeek_GeneratesCorrectChallengeName()
    {
        // Arrange
        var service = CreateService();
        var currentYear = DateTime.UtcNow.Year;
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(It.IsAny<int>()))
            .ReturnsAsync((WeeklyChallengeEntity)null);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<WeeklyChallengeEntity>()))
            .ReturnsAsync((WeeklyChallengeEntity challenge) => challenge);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start
        await service.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<WeeklyChallengeEntity>(c => 
            c.Name.Contains("Week") && 
            c.Name.Contains("Challenge") &&
            c.Name.Contains(currentYear.ToString()))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateChallengeForWeek_AssignsRandomTribeAndSkin()
    {
        // Arrange
        var service = CreateService();
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(It.IsAny<int>()))
            .ReturnsAsync((WeeklyChallengeEntity)null);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<WeeklyChallengeEntity>()))
            .ReturnsAsync((WeeklyChallengeEntity challenge) => challenge);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start
        await service.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<WeeklyChallengeEntity>(c => 
            c.Tribe != TribeData.Type.None && 
            c.Tribe != TribeData.Type.Nature &&
            c.SkinType != DystopiaSkinType.None &&
            c.SkinType != DystopiaSkinType.Test)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateChallengeForWeek_SetsCorrectGameVersionAndDiscordLink()
    {
        // Arrange
        var service = CreateService();
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(It.IsAny<int>()))
            .ReturnsAsync((WeeklyChallengeEntity)null);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<WeeklyChallengeEntity>()))
            .ReturnsAsync((WeeklyChallengeEntity challenge) => challenge);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start
        await service.StopAsync(CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.CreateAsync(It.Is<WeeklyChallengeEntity>(c => 
            c.GameVersion == 114 &&
            c.DiscordLink == "https://discord.gg/rtwgWTzxWy")), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Service_HandlesRepositoryException_ContinuesRunning()
    {
        // Arrange
        var service = CreateService();
        
        _repositoryMock.Setup(x => x.GetByWeekAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert - Should not throw
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100); // Allow background task to start and handle exception
        await service.StopAsync(CancellationToken.None);

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void GetCompositeWeekNumber_MultipleCallsWithSameDate_ReturnsSameResult()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15);

        // Act
        var result1 = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);
        var result2 = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetCompositeWeekNumber_ConsecutiveWeeks_IncreasesCorrectly()
    {
        // Arrange
        var week1Date = new DateTime(2024, 6, 3); // Monday
        var week2Date = week1Date.AddDays(7);     // Next Monday

        // Act
        var week1Composite = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(week1Date);
        var week2Composite = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(week2Date);

        // Assert
        Assert.Equal(week1Composite + 1, week2Composite);
    }

    [Fact]
    public void GetCompositeWeekNumber_CrossYearBoundary_HandlesCorrectly()
    {
        // Arrange - Last week of 2023 and first week of 2024
        var lastWeek2023 = new DateTime(2023, 12, 25); // Should be week 52 of 2023
        var firstWeek2024 = new DateTime(2024, 1, 1);  // Should be week 1 of 2024

        // Act
        var lastWeekComposite = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(lastWeek2023);
        var firstWeekComposite = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(firstWeek2024);

        // Assert
        Assert.True(lastWeekComposite < firstWeekComposite); // 202352 < 202401
        Assert.Equal(2023, lastWeekComposite / 100);         // Year portion
        Assert.Equal(2024, firstWeekComposite / 100);        // Year portion
    }

    [Theory]
    [InlineData(2024, 1, 1)]   // New Year's Day
    [InlineData(2024, 6, 15)]  // Middle of year
    [InlineData(2024, 12, 31)] // New Year's Eve
    public void GetCompositeWeekNumber_VariousDates_ProducesValidComposites(int year, int month, int day)
    {
        // Arrange
        var testDate = new DateTime(year, month, day);

        // Act
        var composite = WeeklyChallengeSchedulerService.GetCompositeWeekNumber(testDate);

        // Assert
        var compositeYear = composite / 100;
        var compositeWeek = composite % 100;
        
        Assert.True(compositeYear >= 2024 && compositeYear <= 2025); // Reasonable year range
        Assert.True(compositeWeek >= 1 && compositeWeek <= 53);      // Valid week range
        Assert.True(composite > 202400 && composite < 202600);       // Valid composite range
    }

    #endregion
}
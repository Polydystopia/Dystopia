using Dystopia.Database;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Models.Skin;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Polytopia.Data;
using Xunit;

namespace Dystopia.Tests.Database;

public class WeeklyChallengeRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;

    public WeeklyChallengeRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private WeeklyChallengeRepository CreateRepository() => new(_dbContextMock.Object);

    private WeeklyChallengeEntity CreateWeeklyChallengeEntity(
        int id = 1,
        int week = 1,
        string name = "Test Challenge",
        TribeData.Type tribe = TribeData.Type.Xinxi,
        DystopiaSkinType skinType = DystopiaSkinType.Default,
        int gameVersion = 1,
        string discordLink = "https://discord.gg/test")
    {
        return new WeeklyChallengeEntity
        {
            Id = id,
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
    public async Task GetByIdAsync_ReturnsWeeklyChallenge_WhenExists()
    {
        // Arrange
        var expectedChallenge = CreateWeeklyChallengeEntity();
        var challenges = new List<WeeklyChallengeEntity> { expectedChallenge };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(expectedChallenge);
        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Challenge", result.Name);
    }

    [Fact]
    public async Task GetByWeekAsync_ReturnsWeeklyChallenge_WhenExists()
    {
        // Arrange
        var expectedChallenge = CreateWeeklyChallengeEntity(week: 5);
        var challenges = new List<WeeklyChallengeEntity> { expectedChallenge };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByWeekAsync(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Week);
    }

    [Fact]
    public async Task GetCurrentAsync_ReturnsLatestWeeklyChallenge()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>
        {
            CreateWeeklyChallengeEntity(1, week: 1),
            CreateWeeklyChallengeEntity(2, week: 3),
            CreateWeeklyChallengeEntity(3, week: 2)
        };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetCurrentAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Week); // Highest week number
    }

    [Fact]
    public async Task GetByTribeAsync_ReturnsFilteredChallenges_OrderedByWeekDesc()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>
        {
            CreateWeeklyChallengeEntity(1, week: 1, tribe: TribeData.Type.Xinxi),
            CreateWeeklyChallengeEntity(2, week: 3, tribe: TribeData.Type.Bardur),
            CreateWeeklyChallengeEntity(3, week: 2, tribe: TribeData.Type.Xinxi)
        };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByTribeAsync(TribeData.Type.Xinxi);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(TribeData.Type.Xinxi, c.Tribe));
        Assert.Equal(2, result[0].Week); // Ordered by week descending
        Assert.Equal(1, result[1].Week);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllChallenges_OrderedByWeekDesc()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>
        {
            CreateWeeklyChallengeEntity(1, week: 1),
            CreateWeeklyChallengeEntity(2, week: 3),
            CreateWeeklyChallengeEntity(3, week: 2)
        };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0].Week); // Ordered by week descending
        Assert.Equal(2, result[1].Week);
        Assert.Equal(1, result[2].Week);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLimitedChallenges_WithDefaultLimit()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>();
        for (int i = 1; i <= 15; i++)
        {
            challenges.Add(CreateWeeklyChallengeEntity(i, week: i));
        }
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetRecentAsync();

        // Assert
        Assert.Equal(10, result.Count); // Default limit
        Assert.Equal(15, result[0].Week); // Most recent first
        Assert.Equal(6, result[9].Week); // 10th item
    }

    [Fact]
    public async Task GetRecentAsync_RespectsCustomLimit()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>();
        for (int i = 1; i <= 10; i++)
        {
            challenges.Add(CreateWeeklyChallengeEntity(i, week: i));
        }
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetRecentAsync(5);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal(10, result[0].Week); // Most recent first
        Assert.Equal(6, result[4].Week); // 5th item
    }

    [Fact]
    public async Task CreateAsync_AddsWeeklyChallenge_AndSavesChanges()
    {
        // Arrange
        var newChallenge = CreateWeeklyChallengeEntity();
        var challenges = new List<WeeklyChallengeEntity>();
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().CreateAsync(newChallenge);

        // Assert
        Assert.Equal(newChallenge, result);
        mockDbSet.Verify(x => x.AddAsync(newChallenge, default), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesWeeklyChallenge_AndSavesChanges()
    {
        // Arrange
        var challenge = CreateWeeklyChallengeEntity();
        var mockDbSet = new List<WeeklyChallengeEntity>().AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().UpdateAsync(challenge);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Update(challenge), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RemovesWeeklyChallenge_WhenExists()
    {
        // Arrange
        var challenge = CreateWeeklyChallengeEntity();
        var challenges = new List<WeeklyChallengeEntity> { challenge };
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(challenge);
        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(1);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Remove(challenge), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var challenges = new List<WeeklyChallengeEntity>();
        var mockDbSet = challenges.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallenges).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(999);

        // Assert
        Assert.False(result);
        mockDbSet.Verify(x => x.Remove(It.IsAny<WeeklyChallengeEntity>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    #endregion
}
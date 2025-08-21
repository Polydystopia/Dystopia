using Dystopia.Database;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Database.WeeklyChallenge.League;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Dystopia.Tests.Database;

public class WeeklyChallengeEntryRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;

    public WeeklyChallengeEntryRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private WeeklyChallengeEntryRepository CreateRepository() => new(_dbContextMock.Object);

    private WeeklyChallengeEntryEntity CreateWeeklyChallengeEntryEntity(
        int id = 1,
        int? weeklyChallengeId = null,
        int? leagueId = null,
        Guid? userId = null,
        Guid? gameId = null,
        int score = 1000,
        bool isValid = true,
        bool hasFinished = true,
        bool hasReplay = false)
    {
        return new WeeklyChallengeEntryEntity
        {
            Id = id,
            WeeklyChallengeId = weeklyChallengeId ?? 1,
            LeagueId = leagueId ?? 1,
            UserId = userId ?? Guid.NewGuid(),
            GameId = gameId ?? Guid.NewGuid(),
            DateCreated = DateTime.Now,
            Score = score,
            HasFinished = hasFinished,
            HasReplay = hasReplay,
            IsValid = isValid
        };
    }

    private UserEntity CreateUserEntity(Guid? id = null)
    {
        return new UserEntity
        {
            Id = id ?? Guid.NewGuid(),
            SteamId = "123456789",
            UserName = "TestUser",
            Discriminator = "1234",
            CurrentLeagueId = 1
        };
    }

    private WeeklyChallengeEntity CreateWeeklyChallengeEntity(int? id = null)
    {
        return new WeeklyChallengeEntity
        {
            Id = id ?? 1,
            Week = 1,
            Name = "Test Challenge"
        };
    }

    private LeagueEntity CreateLeagueEntity(int? id = null)
    {
        return new LeagueEntity
        {
            Id = id ?? 1,
            Name = "Test League"
        };
    }

    #region AI

    [Fact]
    public async Task GetByIdAsync_ReturnsEntry_WhenExists()
    {
        // Arrange
        var expectedEntry = CreateWeeklyChallengeEntryEntity();
        var entries = new List<WeeklyChallengeEntryEntity> { expectedEntry };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1000, result.Score);
    }

    [Fact]
    public async Task GetByUserAndChallengeAsync_ReturnsEntry_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = 1;
        var expectedEntry = CreateWeeklyChallengeEntryEntity(userId: userId, weeklyChallengeId: challengeId);
        var entries = new List<WeeklyChallengeEntryEntity> { expectedEntry };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAndChallengeAsync(userId, challengeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(challengeId, result.WeeklyChallengeId);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsUserEntries_OrderedByDateDesc()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entries = new List<WeeklyChallengeEntryEntity>
        {
            CreateWeeklyChallengeEntryEntity(1, userId: userId),
            CreateWeeklyChallengeEntryEntity(2, userId: Guid.NewGuid()),
            CreateWeeklyChallengeEntryEntity(3, userId: userId)
        };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, entry => Assert.Equal(userId, entry.UserId));
    }

    [Fact]
    public async Task GetByChallengeAsync_ReturnsChallengeEntries_OrderedByScoreDesc()
    {
        // Arrange
        var challengeId = 1;
        var entries = new List<WeeklyChallengeEntryEntity>
        {
            CreateWeeklyChallengeEntryEntity(1, weeklyChallengeId: challengeId, score: 1500),
            CreateWeeklyChallengeEntryEntity(2, weeklyChallengeId: challengeId, score: 2000),
            CreateWeeklyChallengeEntryEntity(3, weeklyChallengeId: challengeId, score: 1000)
        };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByChallengeAsync(challengeId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, entry => Assert.Equal(challengeId, entry.WeeklyChallengeId));
        Assert.Equal(2000, result[0].Score); // Higher score first
        Assert.Equal(1500, result[1].Score);
        Assert.Equal(1000, result[2].Score);
    }

    [Fact]
    public async Task GetLeaderboardAsync_ReturnsValidEntries_OrderedByScoreDesc_WithLimit()
    {
        // Arrange
        var challengeId = 1;
        var leagueId = 1;
        var entries = new List<WeeklyChallengeEntryEntity>
        {
            CreateWeeklyChallengeEntryEntity(1, weeklyChallengeId: challengeId, leagueId: leagueId, score: 1500, isValid: true),
            CreateWeeklyChallengeEntryEntity(2, weeklyChallengeId: challengeId, leagueId: leagueId, score: 2000, isValid: false),
            CreateWeeklyChallengeEntryEntity(3, weeklyChallengeId: challengeId, leagueId: leagueId, score: 1000, isValid: true),
            CreateWeeklyChallengeEntryEntity(4, weeklyChallengeId: challengeId, leagueId: 2, score: 3000, isValid: true)
        };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetLeaderboardAsync(challengeId, leagueId, 10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, entry => Assert.True(entry.IsValid));
        Assert.All(result, entry => Assert.Equal(challengeId, entry.WeeklyChallengeId));
        Assert.All(result, entry => Assert.Equal(leagueId, entry.LeagueId));
        Assert.Equal(1500, result[0].Score); // Higher score first
        Assert.Equal(1000, result[1].Score);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_CreatesNewEntry_WhenNotExists()
    {
        // Arrange
        var newEntry = CreateWeeklyChallengeEntryEntity();
        var entries = new List<WeeklyChallengeEntryEntity>();
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().SaveOrUpdateAsync(newEntry);

        // Assert
        Assert.Equal(newEntry, result);
        mockDbSet.Verify(x => x.AddAsync(newEntry, default), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_UpdatesExistingEntry_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = 1;
        var existingEntry = CreateWeeklyChallengeEntryEntity(userId: userId, weeklyChallengeId: challengeId, score: 1000);
        var updatedEntry = CreateWeeklyChallengeEntryEntity(userId: userId, weeklyChallengeId: challengeId, score: 1500);
        
        var entries = new List<WeeklyChallengeEntryEntity> { existingEntry };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().SaveOrUpdateAsync(updatedEntry);

        // Assert
        Assert.Equal(1500, result.Score);
        Assert.Equal(existingEntry, result); // Should return the existing entity
        mockDbSet.Verify(x => x.AddAsync(It.IsAny<WeeklyChallengeEntryEntity>(), default), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetUserRankAsync_ReturnsCorrectRank_WhenUserHasValidEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = 1;
        var leagueId = 1;
        
        var entries = new List<WeeklyChallengeEntryEntity>
        {
            CreateWeeklyChallengeEntryEntity(1, userId: Guid.NewGuid(), weeklyChallengeId: challengeId, leagueId: leagueId, score: 2000, isValid: true),
            CreateWeeklyChallengeEntryEntity(2, userId: userId, weeklyChallengeId: challengeId, leagueId: leagueId, score: 1500, isValid: true),
            CreateWeeklyChallengeEntryEntity(3, userId: Guid.NewGuid(), weeklyChallengeId: challengeId, leagueId: leagueId, score: 1000, isValid: true)
        };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetUserRankAsync(userId, challengeId, leagueId);

        // Assert
        Assert.Equal(2, result); // User should be rank 2 (1 person with higher score)
    }

    [Fact]
    public async Task GetUserRankAsync_ReturnsMinusOne_WhenUserHasNoValidEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challengeId = 1;
        var leagueId = 1;
        
        var entries = new List<WeeklyChallengeEntryEntity>
        {
            CreateWeeklyChallengeEntryEntity(1, userId: userId, weeklyChallengeId: challengeId, leagueId: leagueId, score: 1500, isValid: false)
        };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetUserRankAsync(userId, challengeId, leagueId);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntry_WhenExists()
    {
        // Arrange
        var entry = CreateWeeklyChallengeEntryEntity();
        var entries = new List<WeeklyChallengeEntryEntity> { entry };
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(entry);
        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(1);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Remove(entry), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var entries = new List<WeeklyChallengeEntryEntity>();
        var mockDbSet = entries.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.WeeklyChallengeEntries).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(999);

        // Assert
        Assert.False(result);
        mockDbSet.Verify(x => x.Remove(It.IsAny<WeeklyChallengeEntryEntity>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    #endregion
}
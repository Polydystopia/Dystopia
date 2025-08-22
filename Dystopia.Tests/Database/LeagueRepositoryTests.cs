using Dystopia.Database;
using Dystopia.Database.WeeklyChallenge.League;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Dystopia.Tests.Database;

public class LeagueRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;

    public LeagueRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private LeagueRepository CreateRepository() => new(_dbContextMock.Object);

    private LeagueEntity CreateLeagueEntity(
        int id = 1,
        string name = "Bronze League",
        string localizationKey = "league.bronze",
        bool isFriendsLeague = false,
        bool isEntry = false)
    {
        return new LeagueEntity
        {
            Id = id,
            Name = name,
            LocalizationKey = localizationKey,
            PrimaryColor = 0xFF8B,
            SecondaryColor = 0xFFCF,
            TertiaryColor = 0xFFF4,
            PromotionRate = 0.2f,
            DemotionRate = 0.2f,
            IsFriendsLeague = isFriendsLeague,
            IsEntry = isEntry
        };
    }

    #region AI

    [Fact]
    public async Task GetByIdAsync_ReturnsLeague_WhenExists()
    {
        // Arrange
        var expectedLeague = CreateLeagueEntity();
        var leagues = new List<LeagueEntity> { expectedLeague };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(expectedLeague);
        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Bronze League", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var leagues = new List<LeagueEntity>();
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsLeague_WhenExists()
    {
        // Arrange
        var expectedLeague = CreateLeagueEntity(name: "Silver League");
        var leagues = new List<LeagueEntity> { expectedLeague };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByNameAsync("Silver League");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Silver League", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLeagues_OrderedByName()
    {
        // Arrange
        var leagues = new List<LeagueEntity>
        {
            CreateLeagueEntity(1, "Gold League"),
            CreateLeagueEntity(2, "Bronze League"),
            CreateLeagueEntity(3, "Silver League")
        };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bronze League", result[0].Name);
        Assert.Equal("Gold League", result[1].Name);
        Assert.Equal("Silver League", result[2].Name);
    }

    [Fact]
    public async Task GetFriendsLeagueAsync_ReturnsFriendsLeague_WhenExists()
    {
        // Arrange
        var leagues = new List<LeagueEntity>
        {
            CreateLeagueEntity(1, "Bronze League", isFriendsLeague: false),
            CreateLeagueEntity(2, "Friends League", isFriendsLeague: true),
            CreateLeagueEntity(3, "Silver League", isFriendsLeague: false)
        };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetFriendsLeagueAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Friends League", result.Name);
        Assert.True(result.IsFriendsLeague);
    }

    [Fact]
    public async Task GetCompetitiveLeaguesAsync_ReturnsOnlyCompetitiveLeagues()
    {
        // Arrange
        var leagues = new List<LeagueEntity>
        {
            CreateLeagueEntity(1, "Bronze League", isFriendsLeague: false),
            CreateLeagueEntity(2, "Friends League", isFriendsLeague: true),
            CreateLeagueEntity(3, "Silver League", isFriendsLeague: false)
        };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetCompetitiveLeaguesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, league => Assert.False(league.IsFriendsLeague));
        Assert.Contains(result, l => l.Name == "Bronze League");
        Assert.Contains(result, l => l.Name == "Silver League");
    }

    [Fact]
    public async Task GetEntryLeagueAsync_ReturnsEntryLeague_WhenExists()
    {
        // Arrange
        var leagues = new List<LeagueEntity>
        {
            CreateLeagueEntity(1, "Bronze League", isEntry: false),
            CreateLeagueEntity(2, "Entry League", isEntry: true),
            CreateLeagueEntity(3, "Silver League", isEntry: false)
        };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetEntryLeagueAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Entry League", result.Name);
        Assert.True(result.IsEntry);
    }

    [Fact]
    public async Task CreateAsync_AddsLeague_AndSavesChanges()
    {
        // Arrange
        var newLeague = CreateLeagueEntity();
        var leagues = new List<LeagueEntity>();
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().CreateAsync(newLeague);

        // Assert
        Assert.Equal(newLeague, result);
        mockDbSet.Verify(x => x.AddAsync(newLeague, default), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesLeague_AndSavesChanges()
    {
        // Arrange
        var league = CreateLeagueEntity();
        var mockDbSet = new List<LeagueEntity>().AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().UpdateAsync(league);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Update(league), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RemovesLeague_WhenExists()
    {
        // Arrange
        var league = CreateLeagueEntity();
        var leagues = new List<LeagueEntity> { league };
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(league);
        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(1);

        // Assert
        Assert.True(result);
        mockDbSet.Verify(x => x.Remove(league), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var leagues = new List<LeagueEntity>();
        var mockDbSet = leagues.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Leagues).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().DeleteAsync(999);

        // Assert
        Assert.False(result);
        mockDbSet.Verify(x => x.Remove(It.IsAny<LeagueEntity>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    #endregion
}
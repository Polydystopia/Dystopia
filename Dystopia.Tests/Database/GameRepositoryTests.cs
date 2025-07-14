using Dystopia.Database;
using DystopiaShared;
using MockQueryable.Moq;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Timers;

namespace Dystopia.Tests.Database;

using Dystopia.Database.Game;
using Dystopia.Services.Cache;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Dystopia.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Dystopia.Bridge;

public class GameRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    private readonly Mock<PolydystopiaDbContext> _dbContextMock;
    private readonly Mock<ICacheService<GameViewModel>> _cacheServiceMock = new();
    private readonly IOptions<CacheSettings> _settings = Options.Create(new CacheSettings { GameViewModel = new CacheProfile() {CacheTime = TimeSpan.FromMinutes(30)}});
    private readonly Mock<IDystopiaCastle> _bridgeMock = new();
    public GameRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private PolydystopiaGameRepository CreateRepository() => new(
        _dbContextMock.Object,
        _cacheServiceMock.Object,
        _settings,
        _bridgeMock.Object
    );

    [Fact]
    public async Task GetByIdAsync_ReturnsCachedItem_WithoutDbAccess()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var expectedGame = new GameViewModel { Id = testId };
        _cacheServiceMock.Setup(c => c.TryGet(testId, out expectedGame)).Returns(true);

        // Act
        var result = await CreateRepository().GetByIdAsync(testId);

        // Assert
        Assert.Same(expectedGame, result);
        _dbContextMock.Verify(db => db.Games.FindAsync(testId), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_FetchesFromDb_WhenNotInCache()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var expectedGame = new GameViewModel { Id = testId };
        _cacheServiceMock.Setup(c => c.TryGet(testId, out expectedGame)).Returns(false);
        _dbContextMock.Setup(db => db.Games.FindAsync(testId)).ReturnsAsync(expectedGame);

        // Act
        var result = await CreateRepository().GetByIdAsync(testId);

        // Assert
        Assert.Same(expectedGame, result);
        _dbContextMock.Verify(db => db.Games.FindAsync(testId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AddsToDatabase()
    {
        // Arrange
        var testGame = new GameViewModel();
        var mockDbSet = new Mock<DbSet<GameViewModel>>();
        _dbContextMock.Setup(db => db.Games).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().CreateAsync(testGame);

        // Assert
        mockDbSet.Verify(db => db.AddAsync(testGame, default), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once);
        Assert.Same(testGame, result);
    }

    [Fact]
    public async Task UpdateAsync_UsesCache_WhenShouldCache()
    {
        // Arrange
        var testGame = new GameViewModel { 
            Id = Guid.NewGuid(),
            TimerSettings = new TimerSettings { UseTimebanks = true },
            DateLastCommand = DateTime.UtcNow
        };

        // Act
        var result = await CreateRepository().UpdateAsync(testGame);
        
        // Assert
        _cacheServiceMock.Verify(c => c.Set(
            testGame.Id, 
            testGame, 
            It.IsAny<Action<PolydystopiaDbContext>>()), 
            Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SavesDirectly_WhenNotCaching()
    {
        // Arrange
        var testGame = new GameViewModel { 
            Id = Guid.NewGuid(),
            TimerSettings = new TimerSettings { UseTimebanks = false },
            DateLastCommand = DateTime.UtcNow.AddHours(-10)
        };
        _dbContextMock.Setup(db => db.Games).Returns(new Mock<DbSet<GameViewModel>>().Object);
        // Act
        var result = await CreateRepository().UpdateAsync(testGame);

        // Assert
        _cacheServiceMock.Verify(c => c.Set(It.IsAny<Guid>(), It.IsAny<GameViewModel>(), It.IsAny<Action<PolydystopiaDbContext>>()), Times.Never);
        _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAllGamesByPlayer_ReturnsFilteredResults()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var games = new List<GameViewModel>
        {
            new() { Id = Guid.NewGuid(), CurrentGameStateData = new byte[] { 1 } },
            new() { Id = Guid.NewGuid(), CurrentGameStateData = new byte[] { 2 } }
        };

        var mockDbSet = games.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(db => db.Games).Returns(mockDbSet.Object);
        
        _bridgeMock.SetupSequence(b => b.IsPlayerInGame(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(true)
            .Returns(false);

        // Act
        var result = await CreateRepository().GetAllGamesByPlayer(playerId);

        // Assert
        Assert.Single(result);
    }
}
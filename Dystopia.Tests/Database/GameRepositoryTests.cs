using System.Reflection;
using Dystopia.Database;
using Dystopia.Database.User;
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
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;
    private readonly Mock<ICacheService<GameEntity>> _cacheServiceMock = new();

    private readonly IOptions<CacheSettings> _settings = Options.Create(new CacheSettings
        { GameEntity = new CacheProfile() { CacheTime = TimeSpan.FromMinutes(30) } });

    private readonly Mock<IDystopiaCastle> _bridgeMock = new();

    public GameRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);

        GameCache.InitializeCache(_cacheServiceMock.Object);
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
        var expectedGame = new GameEntity { Id = testId };
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
        var expectedGame = new GameEntity() { Id = testId };
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
        var testGame = new GameEntity();
        var mockDbSet = new Mock<DbSet<GameEntity>>();
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
        var testGame = new GameEntity
        {
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
        var testGame = new GameEntity
        {
            Id = Guid.NewGuid(),
            TimerSettings = new TimerSettings { UseTimebanks = false },
            DateLastCommand = DateTime.UtcNow.AddHours(-10)
        };
        _dbContextMock.Setup(db => db.Games).Returns(new Mock<DbSet<GameEntity>>().Object);
        // Act
        var result = await CreateRepository().UpdateAsync(testGame);

        // Assert
        _cacheServiceMock.Verify(
            c => c.Set(It.IsAny<Guid>(), It.IsAny<GameEntity>(), It.IsAny<Action<PolydystopiaDbContext>>()),
            Times.Never);
        _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAllGamesByPlayer_OnlyDatabaseHits()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
            });

        // Act
        var result = await CreateRepository().GetAllGamesByPlayer(user);

        // Assert
        Assert.Equal(3, result.Count);
    }


    [Fact]
    public async Task GetAllGamesByPlayer_OnlyCacheHits()
    {
        var overlapGuidA = Guid.NewGuid();
        var overlapGuidB = Guid.NewGuid();
        var overlapGuidC = Guid.NewGuid();

        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(overlapGuidA, new GameEntity()),
                CreateGameParticipation(overlapGuidB, new GameEntity()),
                CreateGameParticipation(overlapGuidC, new GameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(new() { Id = overlapGuidA, CurrentGameStateData = new byte[] { 4 } });
                values.Add(new() { Id = overlapGuidB, CurrentGameStateData = new byte[] { 5 } });
                values.Add(new() { Id = overlapGuidC, CurrentGameStateData = new byte[] { 6 } });
            });

        // Act
        var result = await CreateRepository().GetAllGamesByPlayer(user);

        // Assert
        Assert.Equal(3, result.Count);
    }


    [Fact]
    public async Task GetAllGamesByPlayer_CacheAndDbHits()
    {
        var overlapGuidA = Guid.NewGuid();
        var overlapGuidB = Guid.NewGuid();
        var overlapGuidC = Guid.NewGuid();

        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity()),

                CreateGameParticipation(overlapGuidA, new GameEntity()),
                CreateGameParticipation(overlapGuidB, new GameEntity()),
                CreateGameParticipation(overlapGuidC, new GameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(new() { Id = overlapGuidA, CurrentGameStateData = new byte[] { 4 } });
                values.Add(new() { Id = overlapGuidB, CurrentGameStateData = new byte[] { 5 } });
                values.Add(new() { Id = overlapGuidC, CurrentGameStateData = new byte[] { 6 } });
            });

        // Act
        var result = await CreateRepository().GetAllGamesByPlayer(user);

        // Assert
        Assert.Equal(6, result.Count);
    }


    [Fact]
    public async Task GetAllGamesByPlayer_CacheAndDbHitsWithOverlap_ReturnsFilteredResults()
    {
        var overlapGuidA = Guid.NewGuid();
        var overlapGuidB = Guid.NewGuid();
        var overlapGuidC = Guid.NewGuid();

        var overlapGuidD = Guid.NewGuid();
        var overlapGuidE = Guid.NewGuid();

        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(overlapGuidA, new GameEntity()),
                CreateGameParticipation(overlapGuidB, new GameEntity()),
                CreateGameParticipation(overlapGuidC, new GameEntity()),

                CreateGameParticipation(overlapGuidD, new GameEntity()),
                CreateGameParticipation(overlapGuidE, new GameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(new() { Id = overlapGuidA, CurrentGameStateData = new byte[] { 6 } });

                values.Add(new() { Id = overlapGuidD, CurrentGameStateData = new byte[] { 4 } });
                values.Add(new() { Id = overlapGuidE, CurrentGameStateData = new byte[] { 5 } });
            });

        // Act
        var result = await CreateRepository().GetAllGamesByPlayer(user);

        // Assert
        Assert.Equal(5, result.Count);
    }


    private GameParticipatorUserUser CreateGameParticipation(Guid gameId, GameEntity game)
    {
        var participation = new GameParticipatorUserUser
        {
            GameId = gameId
        };

        // Use reflection to set private/internal property
        var gameProperty = typeof(GameParticipatorUserUser)
            .GetProperty("Game", BindingFlags.NonPublic | BindingFlags.Instance);
        gameProperty?.SetValue(participation, game);

        return participation;
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_ReturnsEmptyList_WhenNoEndedGames()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity { State = GameSessionState.Started }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity { State = GameSessionState.Lobby }),
            }
        };

        // Act
        var result = await CreateRepository().GetLastEndedGamesByPlayer(user, 10);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_ReturnsEndedGames_OrderedByDateLastCommand()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(-5);
        var middleDate = DateTime.UtcNow.AddDays(-2);
        var recentDate = DateTime.UtcNow.AddHours(-1);

        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = oldDate 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Started,
                    DateLastCommand = recentDate 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = recentDate 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = middleDate 
                }),
            }
        };

        // Act
        var result = await CreateRepository().GetLastEndedGamesByPlayer(user, 10);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].DateLastCommand >= result[1].DateLastCommand);
        Assert.True(result[1].DateLastCommand >= result[2].DateLastCommand);
        Assert.Equal(recentDate, result[0].DateLastCommand);
        Assert.Equal(middleDate, result[1].DateLastCommand);
        Assert.Equal(oldDate, result[2].DateLastCommand);
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_RespectsLimit()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-1) 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-2) 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-3) 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-4) 
                }),
            }
        };

        // Act
        var result = await CreateRepository().GetLastEndedGamesByPlayer(user, 2);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_ReturnsAllEndedGames_WhenLimitExceedsAvailable()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-1) 
                }),
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-2) 
                }),
            }
        };

        // Act
        var result = await CreateRepository().GetLastEndedGamesByPlayer(user, 10);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_HandlesZeroLimit()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), new GameEntity 
                { 
                    State = GameSessionState.Ended, 
                    DateLastCommand = DateTime.UtcNow.AddDays(-1) 
                }),
            }
        };

        // Act
        var result = await CreateRepository().GetLastEndedGamesByPlayer(user, 0);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLastEndedGamesByPlayer_HandlesNullGameParticipations()
    {
        // Arrange
        var user = new UserEntity()
        {
            GameParticipations = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            CreateRepository().GetLastEndedGamesByPlayer(user, 10));
    }
}
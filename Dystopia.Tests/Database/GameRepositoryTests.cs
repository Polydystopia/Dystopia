using System.Reflection;
using Dystopia.Database;
using Dystopia.Database.Shared;
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

    private GameEntity CreateGameEntity(
        Guid? id = null,
        GameSessionState state = GameSessionState.Unknown,
        DateTime? dateLastCommand = null,
        TimerSettings timerSettings = null,
        byte[] currentGameStateData = null)
    {
        return new GameEntity
        {
            Id = id ?? Guid.Empty,
            LobbyId = default,
            State = state,
            DateLastCommand = dateLastCommand ?? default,
            Type = RoundType.Friendly,
            TimerSettings = timerSettings,
            InitialGameStateData = new byte[] { },
            CurrentGameStateData = currentGameStateData ?? new byte[] { }
        };
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCachedItem_WithoutDbAccess()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var expectedGame = CreateGameEntity(testId);
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
        var expectedGame = CreateGameEntity(testId);
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
        var testGame = CreateGameEntity(null);
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
        var testGame = CreateGameEntity(
            id: Guid.NewGuid(),
            dateLastCommand: DateTime.UtcNow,
            timerSettings: new TimerSettings { UseTimebanks = true });

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
        var testGame = CreateGameEntity(
            id: Guid.NewGuid(),
            dateLastCommand: DateTime.UtcNow.AddHours(-10),
            timerSettings: new TimerSettings { UseTimebanks = false });
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(overlapGuidA, CreateGameEntity()),
                CreateGameParticipation(overlapGuidB, CreateGameEntity()),
                CreateGameParticipation(overlapGuidC, CreateGameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(CreateGameEntity(overlapGuidA));
                values.Add(CreateGameEntity(overlapGuidB));
                values.Add(CreateGameEntity(overlapGuidC));
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity()),

                CreateGameParticipation(overlapGuidA, CreateGameEntity()),
                CreateGameParticipation(overlapGuidB, CreateGameEntity()),
                CreateGameParticipation(overlapGuidC, CreateGameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(CreateGameEntity(overlapGuidA));
                values.Add(CreateGameEntity(overlapGuidB));
                values.Add(CreateGameEntity(overlapGuidC));
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(overlapGuidA, CreateGameEntity()),
                CreateGameParticipation(overlapGuidB, CreateGameEntity()),
                CreateGameParticipation(overlapGuidC, CreateGameEntity()),

                CreateGameParticipation(overlapGuidD, CreateGameEntity()),
                CreateGameParticipation(overlapGuidE, CreateGameEntity()),
            }
        };

        _cacheServiceMock
            .Setup(c => c.TryGetAll(
                It.IsAny<Func<GameEntity, bool>>(),
                out It.Ref<IList<GameEntity>>.IsAny))
            .Callback((Func<GameEntity, bool> predicate, out IList<GameEntity> values) =>
            {
                values = new List<GameEntity>();
                values.Add(CreateGameEntity(overlapGuidA));

                values.Add(CreateGameEntity(overlapGuidD));
                values.Add(CreateGameEntity(overlapGuidE));
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Started)),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Lobby)),
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: oldDate)),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Started,
                    dateLastCommand: recentDate)),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: recentDate)),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: middleDate)),
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-1))),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-2))),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-3))),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-4))),
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-1))),
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-2))),
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
            CurrentLeagueId = 1,
            GameParticipations = new List<GameParticipatorUserUser>()
            {
                CreateGameParticipation(Guid.NewGuid(), CreateGameEntity(
                    state: GameSessionState.Ended,
                    dateLastCommand: DateTime.UtcNow.AddDays(-1))),
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
            CurrentLeagueId = 1,
            GameParticipations = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            CreateRepository().GetLastEndedGamesByPlayer(user, 10));
    }

    [Fact]
    public async Task GetFavoriteGamesByPlayer_ReturnsEmptyList_WhenNoFavoriteGames()
    {
        // Arrange
        var user = new UserEntity()
        {
            CurrentLeagueId = 1,
            FavoriteGames = new List<GameEntity>()
        };

        // Act
        var result = await CreateRepository().GetFavoriteGamesByPlayer(user);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFavoriteGamesByPlayer_ReturnsFavoriteGames_OrderedByDateLastCommand()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddDays(-5);
        var middleDate = DateTime.UtcNow.AddDays(-2);
        var recentDate = DateTime.UtcNow.AddHours(-1);

        var user = new UserEntity()
        {
            CurrentLeagueId = 1,
            FavoriteGames = new List<GameEntity>()
            {
                CreateGameEntity(
                    id: Guid.NewGuid(),
                    dateLastCommand: oldDate),
                CreateGameEntity(
                    id: Guid.NewGuid(),
                    dateLastCommand: recentDate),
                CreateGameEntity(
                    id: Guid.NewGuid(),
                    dateLastCommand: middleDate),
            }
        };

        // Act
        var result = await CreateRepository().GetFavoriteGamesByPlayer(user);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result[0].DateLastCommand >= result[1].DateLastCommand);
        Assert.True(result[1].DateLastCommand >= result[2].DateLastCommand);
        Assert.Equal(recentDate, result[0].DateLastCommand);
        Assert.Equal(middleDate, result[1].DateLastCommand);
        Assert.Equal(oldDate, result[2].DateLastCommand);
    }

    [Fact]
    public async Task GetFavoriteGamesByPlayer_UsesCachedVersions_WhenAvailable()
    {
        // Arrange
        var gameId1 = Guid.NewGuid();
        var gameId2 = Guid.NewGuid();
        var gameId3 = Guid.NewGuid();

        var user = new UserEntity()
        {
            CurrentLeagueId = 1,
            FavoriteGames = new List<GameEntity>()
            {
                CreateGameEntity(
                    id: gameId1,
                    dateLastCommand: DateTime.UtcNow.AddDays(-1),
                    currentGameStateData: new byte[] { 1 }),
                CreateGameEntity(
                    id: gameId2,
                    dateLastCommand: DateTime.UtcNow.AddDays(-2),
                    currentGameStateData: new byte[] { 2 }),
                CreateGameEntity(
                    id: gameId3,
                    dateLastCommand: DateTime.UtcNow.AddDays(-3),
                    currentGameStateData: new byte[] { 3 }),
            }
        };

        // Setup cache to return updated versions for some games
        _cacheServiceMock
            .Setup(c => c.TryGet(gameId1, out It.Ref<GameEntity>.IsAny))
            .Returns((Guid id, out GameEntity game) =>
            {
                game = CreateGameEntity(
                    id: gameId1,
                    dateLastCommand: DateTime.UtcNow.AddHours(-1),
                    currentGameStateData: new byte[] { 10 });
                return true;
            });

        _cacheServiceMock
            .Setup(c => c.TryGet(gameId2, out It.Ref<GameEntity>.IsAny))
            .Returns(false);

        _cacheServiceMock
            .Setup(c => c.TryGet(gameId3, out It.Ref<GameEntity>.IsAny))
            .Returns((Guid id, out GameEntity game) =>
            {
                game = CreateGameEntity(
                    id: gameId3,
                    dateLastCommand: DateTime.UtcNow.AddHours(-2),
                    currentGameStateData: new byte[] { 30 });
                return true;
            });

        // Act
        var result = await CreateRepository().GetFavoriteGamesByPlayer(user);

        // Assert
        Assert.Equal(3, result.Count);

        // Verify cached versions are used where available
        var game1 = result.First(g => g.Id == gameId1);
        var game2 = result.First(g => g.Id == gameId2);
        var game3 = result.First(g => g.Id == gameId3);

        Assert.Equal(10, game1.CurrentGameStateData[0]); // Cached version
        Assert.Equal(2, game2.CurrentGameStateData[0]); // Original version (not cached)
        Assert.Equal(30, game3.CurrentGameStateData[0]); // Cached version
    }

    [Fact]
    public async Task GetFavoriteGamesByPlayer_UsesOriginalGames_WhenCacheIsNull()
    {
        // Arrange
        var originalCache = GameCache.Cache;
        GameCache.InitializeCache(null); // Set cache to null

        var user = new UserEntity()
        {
            CurrentLeagueId = 1,
            FavoriteGames = new List<GameEntity>()
            {
                CreateGameEntity(
                    id: Guid.NewGuid(),
                    dateLastCommand: DateTime.UtcNow.AddDays(-1)),
                CreateGameEntity(
                    id: Guid.NewGuid(),
                    dateLastCommand: DateTime.UtcNow.AddDays(-2)),
            }
        };

        try
        {
            // Act
            var result = await CreateRepository().GetFavoriteGamesByPlayer(user);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Same(user.FavoriteGames.First(), result[0]); // Should be same reference as original
        }
        finally
        {
            // Restore original cache
            GameCache.InitializeCache(originalCache);
        }
    }
}
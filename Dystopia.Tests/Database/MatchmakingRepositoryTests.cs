using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Shared;
using Dystopia.Database.User;

namespace Dystopia.Tests.Database;

using Microsoft.EntityFrameworkCore;
using Moq;
using Dystopia.Database.Matchmaking;
using Dystopia.Database;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MatchmakingRepositoryTests
{
    private readonly PolydystopiaDbContext _context;
    private readonly PolydystopiaMatchmakingRepository _repository;

    public MatchmakingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new PolydystopiaDbContext(options);
        _context.Database.EnsureCreated();
        
        _repository = new PolydystopiaMatchmakingRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllFittingLobbies_ReturnsMatchingLobbiesExcludingPlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            },
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { playerId }, // we want this one to not be in result
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            playerId, 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
        Assert.DoesNotContain(result, l => l.PlayerIds.Contains(playerId));
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenPlayerInLobby_ExcludesLobby()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var testLobbies = new List<MatchmakingEntity>
        {
            new() { PlayerIds = new List<Guid> { playerId }, MaxPlayers = 2 }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            playerId, 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenCrossPlayAllowed_IncludesCrossPlatform()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LobbyEntityId = null,
                LobbyEntity = null,
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 1000,
                TimeLimit = 5,
                Platform = Platform.NintendoSwitchWithMultiplayer, // other platform
                AllowCrossPlay = true, // allow crossplay
                MaxPlayers = 2,
                PlayerIds = new List<Guid>()

            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 1000, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_AddsEntityAndSavesChanges()
    {
        // Arrange
        var entity = new MatchmakingEntity()
        {
            Id = Guid.NewGuid(),
            PlayerIds = new List<Guid>()
        };
        // Act
        var result = await _repository.CreateAsync(entity);

        // Assert
        var dbEntity = await _context.Matchmaking.FindAsync(result.Id);
        Assert.NotNull(dbEntity);
        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntityAndSavesChanges()
    {
        // Arrange
        var entity = new MatchmakingEntity()
        {
            Id = Guid.NewGuid(),
            PlayerIds = new List<Guid>()
        };
        _context.Matchmaking.Add(entity);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.UpdateAsync(entity);

        // Assert
        var updatedEntity = await _context.Matchmaking.FindAsync(entity.Id);
        Assert.Equal(updatedEntity, result);
        Assert.Equal(entity, result);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenExists_RemovesAndReturnsTrue()
    {
        // Arrange
        var entity = new MatchmakingEntity()
        {
            Id = Guid.NewGuid(),
            PlayerIds = new List<Guid>()
        };
        _context.Matchmaking.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteByIdAsync(entity.Id);

        // Assert
        var deletedEntity = await _context.Matchmaking.FindAsync(entity.Id);
        Assert.Null(deletedEntity);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        // Act
        var result = await _repository.DeleteByIdAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    #region AI

    [Fact]
    public async Task GetAllFittingLobbies_WhenMapSizeZero_IgnoresMapSize()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 4, // different map size
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - passing mapSize as 0 should ignore map size filter
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 0, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenMapPresetNone_IgnoresMapPreset()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Lakes, // different map preset
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - passing MapPreset.None should ignore map preset filter
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.None, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenGameModeNone_IgnoresGameMode()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Perfection, // different game mode
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - passing GameMode.None should ignore game mode filter
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.None, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenScoreLimitZero_IgnoresScoreLimit()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 100, // different score limit
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - passing scoreLimit as 0 should ignore score limit filter
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 0, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenLobbyFull_ExcludesLobby()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }, // lobby is full
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenCrossPlayDisallowed_ExcludesDifferentPlatforms()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.NintendoSwitchWithMultiplayer, // different platform
                AllowCrossPlay = true, // lobby allows crossplay
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - player doesn't allow crossplay
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, false);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenLobbyCrossPlayDisallowed_ExcludesDifferentPlatforms()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.NintendoSwitchWithMultiplayer, // different platform
                AllowCrossPlay = false, // lobby doesn't allow crossplay
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act - player allows crossplay but lobby doesn't
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenVersionMismatch_ExcludesLobby()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 2, // different version
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFittingLobbies_WhenTimeLimitMismatch_ExcludesLobby()
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 10, // different time limit
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(MapPreset.Archipelago)]
    [InlineData(MapPreset.Continents)]
    [InlineData(MapPreset.Lakes)]
    public async Task GetAllFittingLobbies_WithDifferentMapPresets_FiltersCorrectly(MapPreset mapPreset)
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = mapPreset,
                GameMode = GameMode.Domination,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, mapPreset, GameMode.Domination, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
        Assert.Equal(mapPreset, result.First().MapPreset);
    }

    [Theory]
    [InlineData(GameMode.Perfection)]
    [InlineData(GameMode.Might)]
    [InlineData(GameMode.Glory)]
    public async Task GetAllFittingLobbies_WithDifferentGameModes_FiltersCorrectly(GameMode gameMode)
    {
        // Arrange
        var testLobbies = new List<MatchmakingEntity>
        {
            new() {
                Id = Guid.NewGuid(),
                PlayerIds = new List<Guid> { Guid.NewGuid() },
                Version = 1,
                MapSize = 2,
                MapPreset = MapPreset.Dryland,
                GameMode = gameMode,
                ScoreLimit = 10,
                TimeLimit = 5,
                Platform = Platform.Steam,
                AllowCrossPlay = true,
                MaxPlayers = 2
            }
        };

        _context.Matchmaking.AddRange(testLobbies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFittingLobbies(
            Guid.NewGuid(), 1, 2, MapPreset.Dryland, gameMode, 10, 5, Platform.Steam, true);

        // Assert
        Assert.Single(result);
        Assert.Equal(gameMode, result.First().GameMode);
    }

    [Fact]
    public void MatchmakingEntity_Constructor_WithNullParticipators_ShouldThrowArgumentNullException()
    {
        // Arrange
        var lobbyEntity = new LobbyEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            InviteLink = "",
            Bots = new List<int>(),
            Participators = null!,
            DateCreated = default,
            MapPreset = MapPreset.None,
            MapSize = 0,
            GameMode = GameMode.None,
            State = GameSessionState.Unknown,
            Type = RoundType.Friendly,
            DisabledTribes = null // This will cause NullReferenceException
        };

        // Act & Assert - This should fail because constructor accesses Participators without null check
        Assert.Throws<ArgumentNullException>(() =>
            new MatchmakingEntity(lobbyEntity, 1, 2, MapPreset.Dryland, GameMode.Domination, 10, 5, Platform.Steam, true, 2));
    }

    [Fact]
    public void LobbyEntity_GetInvitationStateForPlayer_NonExistentPlayer_ShouldReturnUnknown()
    {
        // Arrange
        var lobbyEntity = new LobbyEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            InviteLink = "",
            Bots = new List<int>(),
            Participators = new List<LobbyParticipatorUserEntity>
            {
                new()
                {
                    UserId = Guid.NewGuid(),
                    InvitationState = PlayerInvitationState.Accepted
                }
            },
            DateCreated = default,
            MapPreset = MapPreset.None,
            MapSize = 0,
            GameMode = GameMode.None,
            State = GameSessionState.Unknown,
            Type = RoundType.Friendly,
            DisabledTribes = null
        };

        var nonExistentPlayerId = Guid.NewGuid();

        //Act
        var invitationState = lobbyEntity.GetInvitationStateForPlayer(nonExistentPlayerId);

        //Assert
        Assert.Equal(PlayerInvitationState.Unknown, invitationState);
    }

    #endregion
}

// Required for async query support
internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) =>
        new TestAsyncEnumerable<T>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => _inner.Execute(expression)!;

    public TResult Execute<TResult>(Expression expression) =>
        _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default) =>
        Task.FromResult(Execute<TResult>(expression)) is TResult result
            ? result
            : throw new InvalidCastException();
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() =>
        ValueTask.FromResult(_inner.MoveNext());
}

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

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

public class MatchMakingRepositoryTests
{
    private readonly PolydystopiaDbContext _context;
    private readonly PolydystopiaMatchmakingRepository _repository;

    public MatchMakingRepositoryTests()
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

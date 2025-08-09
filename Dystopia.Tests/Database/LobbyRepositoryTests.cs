using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dystopia.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using Xunit;
using Dystopia.Database.Lobby;
using Dystopia.Database.User;
using PolytopiaBackendBase.Game;

namespace Dystopia.Tests.Database;

public class LobbyRepositoryTests
{
    private readonly Mock<PolydystopiaDbContext> _mockContext;
    private readonly PolydystopiaLobbyRepository _repository;
    private readonly Mock<DbSet<LobbyEntity>> _mockLobbiesSet;

    public LobbyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockContext = new Mock<PolydystopiaDbContext>(options);
        _mockLobbiesSet = new Mock<DbSet<LobbyEntity>>();
        _mockContext.Setup(m => m.Lobbies).Returns(_mockLobbiesSet.Object);
        _repository = new PolydystopiaLobbyRepository(_mockContext.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsLobby_WhenLobbyExists()
    {
        var lobbyId = Guid.NewGuid();
        var lobbyEntity = new LobbyEntity { Id = lobbyId };
        
        _mockLobbiesSet.Setup(x => x.FindAsync(lobbyId))
                      .ReturnsAsync(lobbyEntity);

        var result = await _repository.GetByIdAsync(lobbyId);

        Assert.NotNull(result);
        Assert.Equal(lobbyId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenLobbyDoesNotExist()
    {
        var lobbyId = Guid.NewGuid();
        
        _mockLobbiesSet.Setup(x => x.FindAsync(lobbyId))
                      .ReturnsAsync((LobbyEntity?)null);

        var result = await _repository.GetByIdAsync(lobbyId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsLobbyToContext_AndSavesChanges()
    {
        var lobbyEntity = new LobbyEntity { Id = Guid.NewGuid() };

        _mockLobbiesSet.Setup(x => x.AddAsync(lobbyEntity, default))
                      .Returns(new ValueTask<EntityEntry<LobbyEntity>>());
        _mockContext.Setup(x => x.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        var result = await _repository.CreateAsync(lobbyEntity);

        Assert.Equal(lobbyEntity, result);
        _mockLobbiesSet.Verify(x => x.AddAsync(lobbyEntity, default), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesLobbyInContext_AndSetsDateModified()
    {
        var lobbyEntity = new LobbyEntity { Id = Guid.NewGuid() };
        var originalDateModified = lobbyEntity.DateModified;

        _mockLobbiesSet.Setup(x => x.Update(lobbyEntity));
        _mockContext.Setup(x => x.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        var result = await _repository.UpdateAsync(lobbyEntity);

        Assert.Equal(lobbyEntity, result);
        Assert.NotEqual(originalDateModified, lobbyEntity.DateModified);
        Assert.True((DateTime.UtcNow - lobbyEntity.DateModified!.Value).TotalSeconds < 5);
        _mockLobbiesSet.Verify(x => x.Update(lobbyEntity), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenLobbyDoesNotExist()
    {
        var lobbyId = Guid.NewGuid();
        
        _mockLobbiesSet.Setup(x => x.FindAsync(lobbyId))
                      .ReturnsAsync((LobbyEntity?)null);

        var result = await _repository.DeleteAsync(lobbyId);

        Assert.False(result);
        _mockLobbiesSet.Verify(x => x.Remove(It.IsAny<LobbyEntity>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenLobbyExists()
    {
        var lobbyId = Guid.NewGuid();
        var lobbyEntity = new LobbyEntity { Id = lobbyId };
        
        _mockLobbiesSet.Setup(x => x.FindAsync(lobbyId))
                      .ReturnsAsync(lobbyEntity);
        _mockLobbiesSet.Setup(x => x.Remove(lobbyEntity));
        _mockContext.Setup(x => x.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        var result = await _repository.DeleteAsync(lobbyId);

        Assert.True(result);
        _mockLobbiesSet.Verify(x => x.Remove(lobbyEntity), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAllLobbiesByPlayer_ReturnsPlayerLobbies()
    {
        var playerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();
        
        var playerParticipator = new LobbyParticipatorUserEntity { UserId = playerId };
        var otherParticipator = new LobbyParticipatorUserEntity { UserId = otherPlayerId };
        
        var playerLobby = new LobbyEntity 
        { 
            Id = Guid.NewGuid(),
            Participators = new List<LobbyParticipatorUserEntity> { playerParticipator }
        };
        
        var otherLobby = new LobbyEntity 
        { 
            Id = Guid.NewGuid(),
            Participators = new List<LobbyParticipatorUserEntity> { otherParticipator }
        };
        
        var mixedLobby = new LobbyEntity 
        { 
            Id = Guid.NewGuid(),
            Participators = new List<LobbyParticipatorUserEntity> { playerParticipator, otherParticipator }
        };

        var lobbies = new List<LobbyEntity> { playerLobby, otherLobby, mixedLobby };
        var mockLobbiesQueryable = lobbies.AsQueryable().BuildMockDbSet();
        
        _mockContext.Setup(x => x.Lobbies).Returns(mockLobbiesQueryable.Object);

        var result = await _repository.GetAllLobbiesByPlayer(playerId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(playerLobby, result);
        Assert.Contains(mixedLobby, result);
        Assert.DoesNotContain(otherLobby, result);
    }

    [Fact]
    public async Task GetAllLobbiesByPlayer_ReturnsEmptyList_WhenNoLobbiesFound()
    {
        var playerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();
        
        var otherParticipator = new LobbyParticipatorUserEntity { UserId = otherPlayerId };
        var otherLobby = new LobbyEntity 
        { 
            Id = Guid.NewGuid(),
            Participators = new List<LobbyParticipatorUserEntity> { otherParticipator }
        };

        var lobbies = new List<LobbyEntity> { otherLobby };
        var mockLobbiesQueryable = lobbies.AsQueryable().BuildMockDbSet();
        
        _mockContext.Setup(x => x.Lobbies).Returns(mockLobbiesQueryable.Object);

        var result = await _repository.GetAllLobbiesByPlayer(playerId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

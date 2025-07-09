using System.Diagnostics;
using PolytopiaBackendBase.Game;

namespace Dystopia.Tests.Database;

using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dystopia.Database.Friendship;
using Dystopia.Database.User;
using Dystopia.Database;
using PolytopiaBackendBase.Auth;

public class FriendshipTests
{
    private readonly PolydystopiaDbContext _mockContext;
    private readonly FriendshipRepository _repository;
    public FriendshipTests()
    {
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockContext = new PolydystopiaDbContext(options);
        
        _repository = new FriendshipRepository(_mockContext);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_NoRelationship_ReturnsNone()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Act
        var result = await _repository.GetFriendshipStatusAsync(user1, user2);

        // Assert
        Assert.Equal(FriendshipStatus.None, result);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_RequestSent_ReturnsCorrectStatus()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var friendship = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.SentRequest };
        _mockContext.Add(friendship);
        await _mockContext.SaveChangesAsync();

        // Act
        var result1 = await _repository.GetFriendshipStatusAsync(user1, user2);
        var result2 = await _repository.GetFriendshipStatusAsync(user2, user1);

        // Assert
        Assert.Equal(FriendshipStatus.SentRequest, result1);
        Assert.Equal(FriendshipStatus.ReceivedRequest, result2);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_NewFriendship_AddsToDatabase()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Accepted);
        // Assert
        var addedFriendship = await _mockContext.Friends
            .FirstOrDefaultAsync(f => f.UserId1 == user1 && f.UserId2 == user2);
        Assert.NotNull(addedFriendship);
        Assert.Equal(user1, addedFriendship!.UserId1);
        Assert.Equal(user2, addedFriendship.UserId2);
        Assert.Equal(FriendshipStatus.Accepted, addedFriendship.Status);
        Assert.True(result);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_UpdateExisting_UpdatesStatus()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var existing = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.SentRequest };
        _mockContext.Add(existing);
        await _mockContext.SaveChangesAsync();
        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Accepted);

        // Assert
        Assert.Equal(FriendshipStatus.Accepted, existing.Status);
        Assert.True(result);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_ReturnsCombinedFriends()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var friend1 = new UserEntity { PolytopiaId = Guid.NewGuid(), Alias = "Friend1" };
        var friend2 = new UserEntity { PolytopiaId = Guid.NewGuid(), Alias = "Friend2" };

        var friends = new List<FriendshipEntity>
        {
            new() { UserId1 = userId, UserId2 = friend1.PolytopiaId },
            new() { UserId1 = userId, UserId2 = friend2.PolytopiaId },
            new() { UserId1 = friend1.PolytopiaId, UserId2 = friend2.PolytopiaId }, // this should not be in results
        };
        await _mockContext.AddRangeAsync(friend1, friend2);
        await _mockContext.AddRangeAsync(friends);
        await _mockContext.SaveChangesAsync();
        // Act
        var result = await _repository.GetFriendsForUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        // Assert.Contains(result, f => f.User.UserName == "Friend1");
        // Assert.Contains(result, f => f.User.UserName == "Friend2"); TODO use reflection
    }

    [Fact]
    public async Task DeleteFriendshipAsync_ExistingFriendship_RemovesFromDatabase()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var friendship = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.Accepted };
        await _mockContext.AddAsync(friendship);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteFriendshipAsync(user1, user2);

        // Assert
        Assert.Empty(_mockContext.Friends);
        Assert.True(result);
    }
}

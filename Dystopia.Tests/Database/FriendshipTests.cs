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
        var friendship = new FriendshipEntity
            { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.SentRequest };
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
        var friend1 = new UserEntity()
        {
            Id = Guid.NewGuid(), UserName = "Friend1", SteamId = "1", Discriminator = "1111",
            AvatarStateData = new byte[0], GameVersions = new List<ClientGameVersionViewModel>()
        };
        var friend2 = new UserEntity
        {
            Id = Guid.NewGuid(), UserName = "Friend2", SteamId = "1", Discriminator = "1111",
            AvatarStateData = new byte[0], GameVersions = new List<ClientGameVersionViewModel>()
        };

        var friends = new List<FriendshipEntity>
        {
            new() { UserId1 = userId, UserId2 = friend1.Id },
            new() { UserId1 = userId, UserId2 = friend2.Id },
            new() { UserId1 = friend1.Id, UserId2 = friend2.Id }, // this should not be in results
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

    #region AI

    [Fact]
    public async Task DeleteFriendshipAsync_NonExistentFriendship_ReturnsFalse()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteFriendshipAsync(user1, user2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteFriendshipAsync_ReversedUserOrder_RemovesFromDatabase()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var friendship = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.Accepted };
        await _mockContext.AddAsync(friendship);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteFriendshipAsync(user2, user1);

        // Assert
        Assert.Empty(_mockContext.Friends);
        Assert.True(result);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_AcceptedFriendship_ReturnsAccepted()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var friendship = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.Accepted };
        _mockContext.Add(friendship);
        await _mockContext.SaveChangesAsync();

        // Act
        var result1 = await _repository.GetFriendshipStatusAsync(user1, user2);
        var result2 = await _repository.GetFriendshipStatusAsync(user2, user1);

        // Assert
        Assert.Equal(FriendshipStatus.Accepted, result1);
        Assert.Equal(FriendshipStatus.Accepted, result2);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_SameUser_ReturnsNone()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetFriendshipStatusAsync(userId, userId);

        // Assert
        Assert.Equal(FriendshipStatus.None, result);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_RejectedStatus_DeletesFriendship()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var existing = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.SentRequest };
        _mockContext.Add(existing);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Rejected);

        // Assert
        Assert.Empty(_mockContext.Friends);
        Assert.True(result);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_NoneStatus_DeletesFriendship()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var existing = new FriendshipEntity { UserId1 = user1, UserId2 = user2, Status = FriendshipStatus.Accepted };
        _mockContext.Add(existing);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.None);

        // Assert
        Assert.Empty(_mockContext.Friends);
        Assert.True(result);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_RequestStoredInReverseOrder_ReturnsCorrectStatus()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var friendship = new FriendshipEntity { UserId1 = user2, UserId2 = user1, Status = FriendshipStatus.SentRequest };
        _mockContext.Add(friendship);
        await _mockContext.SaveChangesAsync();

        // Act
        var result1 = await _repository.GetFriendshipStatusAsync(user1, user2);
        var result2 = await _repository.GetFriendshipStatusAsync(user2, user1);

        // Assert
        Assert.Equal(FriendshipStatus.ReceivedRequest, result1);
        Assert.Equal(FriendshipStatus.SentRequest, result2);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_UpdateExistingReverseOrder_UpdatesCorrectly()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var existing = new FriendshipEntity { UserId1 = user2, UserId2 = user1, Status = FriendshipStatus.SentRequest };
        _mockContext.Add(existing);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Accepted);

        // Assert
        Assert.Equal(FriendshipStatus.Accepted, existing.Status);
        Assert.True(result);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_MultipleFriendsWithDifferentStatuses_ReturnsOnlyAcceptedFriends()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var friend1 = new UserEntity()
        {
            Id = Guid.NewGuid(), UserName = "AcceptedFriend", SteamId = "1", Discriminator = "1111",
            AvatarStateData = new byte[0], GameVersions = new List<ClientGameVersionViewModel>()
        };
        var friend2 = new UserEntity
        {
            Id = Guid.NewGuid(), UserName = "RequestSentUser", SteamId = "2", Discriminator = "2222",
            AvatarStateData = new byte[0], GameVersions = new List<ClientGameVersionViewModel>()
        };

        var friendships = new List<FriendshipEntity>
        {
            new() { UserId1 = userId, UserId2 = friend1.Id, Status = FriendshipStatus.Accepted },
            new() { UserId1 = userId, UserId2 = friend2.Id, Status = FriendshipStatus.SentRequest }
        };
        await _mockContext.AddRangeAsync(friend1, friend2);
        await _mockContext.AddRangeAsync(friendships);
        await _mockContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetFriendsForUserAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        var acceptedFriend = result.FirstOrDefault(f => f.User.UserName == "AcceptedFriend");
        var requestFriend = result.FirstOrDefault(f => f.User.UserName == "RequestSentUser");

        Assert.NotNull(acceptedFriend);
        Assert.NotNull(requestFriend);
        Assert.Equal(FriendshipStatus.Accepted, acceptedFriend.Status);
        Assert.Equal(FriendshipStatus.SentRequest, requestFriend.Status);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetFriendsForUserAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFriendshipStatusAsync_EmptyGuidUsers_ReturnsNone()
    {
        // Arrange & Act
        var result = await _repository.GetFriendshipStatusAsync(Guid.Empty, Guid.Empty);

        // Assert
        Assert.Equal(FriendshipStatus.None, result);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_EmptyGuidUsers_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _repository.SetFriendshipStatusAsync(Guid.Empty, Guid.Empty, FriendshipStatus.SentRequest);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteFriendshipAsync_EmptyGuidUsers_ReturnsFalse()
    {
        // Arrange & Act
        var result = await _repository.DeleteFriendshipAsync(Guid.Empty, Guid.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFriendsForUserAsync_EmptyGuidUser_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = await _repository.GetFriendsForUserAsync(Guid.Empty);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_OneEmptyGuid_ReturnsFalse()
    {
        // Arrange
        var validUserId = Guid.NewGuid();

        // Act & Assert
        var result1 = await _repository.SetFriendshipStatusAsync(Guid.Empty, validUserId, FriendshipStatus.SentRequest);
        var result2 = await _repository.SetFriendshipStatusAsync(validUserId, Guid.Empty, FriendshipStatus.SentRequest);

        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_NewFriendship_SetsCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.SentRequest);
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(result);
        var friendship = await _mockContext.Friends
            .FirstOrDefaultAsync(f => f.UserId1 == user1 && f.UserId2 == user2);

        Assert.NotNull(friendship);
        Assert.True(friendship!.CreatedAt > beforeCreation && friendship.CreatedAt < afterCreation);
        Assert.True(friendship.UpdatedAt > beforeCreation && friendship.UpdatedAt < afterCreation);
        Assert.Equal(friendship.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), friendship.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_UpdateExisting_UpdatesOnlyUpdatedTimestamp()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var originalCreatedAt = DateTime.UtcNow.AddHours(-1);
        var existing = new FriendshipEntity
        {
            UserId1 = user1,
            UserId2 = user2,
            Status = FriendshipStatus.SentRequest,
            CreatedAt = originalCreatedAt,
            UpdatedAt = originalCreatedAt
        };
        _mockContext.Add(existing);
        await _mockContext.SaveChangesAsync();

        await Task.Delay(50);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Accepted);
        var afterUpdate = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(result);
        Assert.Equal(originalCreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), existing.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.True(existing.UpdatedAt > beforeUpdate && existing.UpdatedAt < afterUpdate);
        Assert.True(existing.UpdatedAt > existing.CreatedAt);
    }

    [Fact]
    public async Task SetFriendshipStatusAsync_MultipleUpdates_UpdatesTimestampEachTime()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Create initial friendship
        await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.SentRequest);
        var friendship = await _mockContext.Friends.FirstAsync(f => f.UserId1 == user1 && f.UserId2 == user2);
        var firstUpdateTime = friendship.UpdatedAt;

        await Task.Delay(50);

        // Act - First update
        await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.Accepted);
        var secondUpdateTime = friendship.UpdatedAt;

        await Task.Delay(50);

        // Act - Second update
        await _repository.SetFriendshipStatusAsync(user1, user2, FriendshipStatus.SentRequest);
        var thirdUpdateTime = friendship.UpdatedAt;

        // Assert
        Assert.True(secondUpdateTime > firstUpdateTime);
        Assert.True(thirdUpdateTime > secondUpdateTime);
    }

    #endregion
}
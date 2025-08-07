using Microsoft.EntityFrameworkCore;
using Dystopia.Database.User;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Friendship;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public FriendshipRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FriendshipStatus> GetFriendshipStatusAsync(Guid user1Id, Guid user2Id)
    {
        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId1 == user1Id && f.UserId2 == user2Id) || (f.UserId1 == user2Id && f.UserId2 == user1Id));

        if (friendship?.Status == FriendshipStatus.SentRequest)
        {
            return friendship.UserId1 == user1Id ? FriendshipStatus.SentRequest : FriendshipStatus.ReceivedRequest;
        }

        return friendship?.Status ?? FriendshipStatus.None;
    }

    public async Task<bool> SetFriendshipStatusAsync(Guid user1Id, Guid user2Id, FriendshipStatus status)
    {
        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId1 == user1Id && f.UserId2 == user2Id) || (f.UserId1 == user2Id && f.UserId2 == user1Id));

        if (friendship == null)
        {
            friendship = new FriendshipEntity
            {
                UserId1 = user1Id,
                UserId2 = user2Id,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _dbContext.Friends.Add(friendship);
        }
        else if (status is FriendshipStatus.None or FriendshipStatus.Rejected)
        {
            return await DeleteFriendshipAsync(user1Id, user2Id);
        }
        else
        {
            friendship.Status = status;
            friendship.UpdatedAt = DateTime.UtcNow;
        }

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<List<(UserEntity User, FriendshipStatus Status)>> GetFriendsForUserAsync(Guid userId)
    {
        var friendUsers = await _dbContext.Users
            .Where(u => _dbContext.Friends
                .Any(f =>
                    (f.UserId1 == userId && f.UserId2 == u.Id) ||
                    (f.UserId2 == userId && f.UserId1 == u.Id)
                )
            )
            .ToListAsync();

        var friends = new List<(UserEntity User, FriendshipStatus Status)>();
        foreach (var friend in friendUsers)
        {
            friends.Add((friend, await GetFriendshipStatusAsync(userId, friend.Id)));
        }

        return friends;
    }

    public async Task<bool> DeleteFriendshipAsync(Guid user1Id, Guid user2Id)
    {
        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId1 == user1Id && f.UserId2 == user2Id) || (f.UserId1 == user2Id && f.UserId2 == user1Id));

        if (friendship != null)
        {
            _dbContext.Friends.Remove(friendship);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        return false;
    }
}
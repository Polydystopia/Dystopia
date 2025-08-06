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

    public async Task<List<PolytopiaFriendViewModel>> GetFriendsForUserAsync(Guid userId)
    {
        var friendships1 = await _dbContext.Friends
            .Where(f => f.UserId1 == userId)
            .Select(f => f.UserId2)
            .ToListAsync();

        var friendships2 = await _dbContext.Friends
            .Where(f => f.UserId2 == userId)
            .Select(f => f.UserId1)
            .ToListAsync();

        var friendIds = friendships1.Concat(friendships2).ToList();

        var friendUsers = await _dbContext.Users
            .Where(u => friendIds.Contains(u.Id))
            .ToListAsync();

        var friendViewModels = new List<PolytopiaFriendViewModel>();
        foreach (var user in friendUsers)
        {
            var friend = new PolytopiaFriendViewModel();
            friend.User = user.ToViewModel();
            friend.FriendshipStatus = await GetFriendshipStatusAsync(userId, user.Id);

            friendViewModels.Add(friend);
        }

        return friendViewModels;
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
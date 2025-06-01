using Microsoft.EntityFrameworkCore;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Friendship;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly PolydystopiaDbContext _dbContext;

    public FriendshipRepository(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FriendshipStatus> GetFriendshipStatusAsync(Guid user1Id, Guid user2Id)
    {
        var forUser = user1Id;

        if (user1Id.CompareTo(user2Id) > 0)
            (user1Id, user2Id) = (user2Id, user1Id);

        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f => f.UserId1 == user1Id && f.UserId2 == user2Id);

        if (friendship?.Status == FriendshipStatus.SentRequest)
        {
            // Since we can only project SentRequest or ReceivedRequest, we need to check who called this method and set the status accordingly
            friendship.Status = user1Id == forUser ? FriendshipStatus.SentRequest : FriendshipStatus.ReceivedRequest;
        }

        return friendship?.Status ?? FriendshipStatus.None;
    }

    public async Task<bool> SetFriendshipStatusAsync(Guid user1Id, Guid user2Id, FriendshipStatus status)
    {
        if (user1Id.CompareTo(user2Id) > 0)
            (user1Id, user2Id) = (user2Id, user1Id);

        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f => f.UserId1 == user1Id && f.UserId2 == user2Id);

        if (friendship == null)
        {
            friendship = new FriendshipEntity
            {
                UserId1 = user1Id,
                UserId2 = user2Id,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Friends.Add(friendship);
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
            .Where(u => friendIds.Contains(u.PolytopiaId))
            .ToListAsync();

        var friendViewModels = new List<PolytopiaFriendViewModel>();
        foreach (var user in friendUsers)
        {
            var friend = new PolytopiaFriendViewModel();
            friend.User = PolydystopiaUserRepository.AddMissingData(user);
            friend.FriendshipStatus = await GetFriendshipStatusAsync(userId, user.PolytopiaId);
            
            friendViewModels.Add(friend);
        }
        
        return friendViewModels;
    }

    public async Task<bool> DeleteFriendshipAsync(Guid user1Id, Guid user2Id)
    {
        if (user1Id.CompareTo(user2Id) > 0)
            (user1Id, user2Id) = (user2Id, user1Id);

        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f => f.UserId1 == user1Id && f.UserId2 == user2Id);

        if (friendship != null)
        {
            _dbContext.Friends.Remove(friendship);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        return false;
    }
}
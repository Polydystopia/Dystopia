using Microsoft.EntityFrameworkCore;
using Dystopia.Database.User;
using Dystopia.Patches;
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
    { // TODO inspect usages
        var friendship = await _dbContext.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId1 == user1Id && f.UserId2 == user2Id) || (f.UserId1 == user2Id && f.UserId2 == user1Id));

        if (friendship?.Status == FriendshipStatus.SentRequest)
        {
            return friendship.UserId1 == user1Id ? FriendshipStatus.SentRequest : FriendshipStatus.ReceivedRequest;
        }

        return friendship?.Status ?? FriendshipStatus.None;
    }

    public static FriendshipEntity ReverseFriendship(FriendshipEntity friendshipEntity)
    {
        return new FriendshipEntity
        {
            UserId1 = friendshipEntity.UserId2,
            User1 = friendshipEntity.User2,
            UserId2 = friendshipEntity.UserId1,
            User2 = friendshipEntity.User1,
            Status = friendshipEntity.Status switch
            {
                FriendshipStatus.None => FriendshipStatus.None,
                FriendshipStatus.SentRequest => FriendshipStatus.ReceivedRequest,
                FriendshipStatus.ReceivedRequest => FriendshipStatus.SentRequest,
                FriendshipStatus.Accepted => FriendshipStatus.Accepted,
                FriendshipStatus.Rejected => FriendshipStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException()
            },
            CreatedAt = friendshipEntity.CreatedAt,
            UpdatedAt = friendshipEntity.UpdatedAt,
        };
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

    public async Task<IEnumerable<FriendshipEntity>> GetFriendsForUserAsync(Guid userId)
    {
        return await _dbContext.Users
            .Where(u => u.PolytopiaId == userId)
            .Include(u => u.Friends1)
            .Include(u => u.Friends2)
            .FirstAsync()
            .Then(u => u.Friends1.Concat(u.Friends2.Select(entity => ReverseFriendship(entity))));
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
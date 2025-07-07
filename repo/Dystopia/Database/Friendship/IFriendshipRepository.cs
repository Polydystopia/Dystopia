using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Friendship;

public interface IFriendshipRepository
{
    Task<FriendshipStatus> GetFriendshipStatusAsync(Guid user1Id, Guid user2Id);
    Task<bool> SetFriendshipStatusAsync(Guid user1Id, Guid user2Id, FriendshipStatus status);
    Task<List<PolytopiaFriendViewModel>> GetFriendsForUserAsync(Guid userId);
    Task<bool> DeleteFriendshipAsync(Guid user1Id, Guid user2Id);
}

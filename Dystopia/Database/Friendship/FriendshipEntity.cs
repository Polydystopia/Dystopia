using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Friendship;

[PrimaryKey(nameof(UserId1), nameof(UserId2))]
public class FriendshipEntity
{
    public required Guid UserId1 { get; init; }
    public UserEntity User1 { get; init; } = null!;
    public required Guid UserId2 { get; init; }
    public UserEntity User2 { get; init; } = null!;
    public required FriendshipStatus Status { get; set; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; set; }
}

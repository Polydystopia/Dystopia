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
    public Guid UserId1 { get; set; }
    public Guid UserId2 { get; set; }
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual UserEntity User1 { get; set; }
    public virtual UserEntity User2 { get; set; }
}

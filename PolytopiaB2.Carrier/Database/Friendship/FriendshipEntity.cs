using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Friendship;

[PrimaryKey(nameof(UserId1), nameof(UserId2))]
public class FriendshipEntity
{
    public Guid UserId1 { get; set; } // Always the lower ID of the two
    public Guid UserId2 { get; set; } // Always the higher ID of the two
    public FriendshipStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual PolytopiaUserViewModel User1 { get; set; }
    public virtual PolytopiaUserViewModel User2 { get; set; }
}

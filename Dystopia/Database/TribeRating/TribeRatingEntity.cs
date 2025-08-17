using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using Polytopia.Data;

namespace Dystopia.Database.TribeRating;

[PrimaryKey(nameof(UserId), nameof(Tribe))]
public class TribeRatingEntity
{
    public required TribeData.Type Tribe { get; set; }
    public uint? Score { get; set; }
    public uint? Rating { get; set; }

    public required Guid UserId { get; set; }
    public virtual UserEntity User { get; init; } = null!;
}
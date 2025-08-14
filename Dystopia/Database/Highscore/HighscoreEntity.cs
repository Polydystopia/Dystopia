using System.ComponentModel.DataAnnotations;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using Polytopia.Data;

namespace Dystopia.Database.Highscore;

[PrimaryKey(nameof(UserId), nameof(Tribe))]
public class HighscoreEntity
{
    public required TribeData.Type Tribe { get; set; }
    public required uint Score { get; set; }

    public required Guid UserId { get; set; }
    public virtual UserEntity User { get; init; } = null!;

    public required byte[]? InitialGameStateData { get; init; }
    public required byte[]? FinalGameStateData { get; set; }
}
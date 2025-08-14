using System.ComponentModel.DataAnnotations;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;

namespace Dystopia.Database.Highscore;

[PrimaryKey(nameof(UserId), nameof(Tribe))]
public class HighscoreEntity
{
    public required int Tribe { get; set; }
    public required int Score { get; set; }

    public required Guid UserId { get; set; }
    public virtual UserEntity User { get; init; } = null!;

    public required byte[]? InitialGameStateData { get; init; }
    public required byte[]? FinalGameStateData { get; set; }
}
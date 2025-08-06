using Dystopia.Database.Game;
using Dystopia.Database.User;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Replay;

public class UserFavoriteGame
{
    public Guid UserId { get; set; }
    public virtual UserEntity User { get; set; }

    public Guid GameId { get; set; }
    public virtual GameEntity Game { get; set; }

    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
}
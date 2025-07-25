using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Replay;

public class UserFavouriteGame
{
    public Guid UserId { get; set; }
    public PolytopiaUserViewModel User { get; set; }

    public Guid GameId { get; set; }
    public GameViewModel Game { get; set; }

    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
}
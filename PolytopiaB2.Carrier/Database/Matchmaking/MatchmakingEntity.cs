using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Database.Matchmaking;

public class MatchmakingEntity
{
    public Guid Id { get; set; }

    public Guid? LobbyGameViewModelId { get; set; }
    public LobbyGameViewModel? LobbyGameViewModel { get; set; }

    public int Version { get; set; }
    public int MapSize { get; set; }
    public MapPreset MapPreset { get; set; }
    public GameMode GameMode { get; set; }
    public int ScoreLimit { get; set; }
    public int TimeLimit { get; set; }
    public Platform Platform { get; set; }
    public bool AllowCrossPlay { get; set; }

    public int MaxPlayers { get; set; }

    public List<Guid> PlayerIds { get; set; }
}
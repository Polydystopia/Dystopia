using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Matchmaking;

public class MatchMakingFilter
{

    public Guid PlayerId { get; init; }
    public int Version { get; init; }
    public int MapSize { get; init; }
    public MapPreset MapPreset { get; init; }
    public GameMode GameMode { get; init; }
    public int ScoreLimit { get; init; }
    public int TimeLimit { get; init; }
    public Platform Platform { get; init; }
    public bool AllowCrossPlay { get; init; }
}
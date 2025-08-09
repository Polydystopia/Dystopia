using Dystopia.Database.Lobby;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Matchmaking;

public class MatchmakingEntity
{
    public MatchmakingEntity()
    {

    }

    public MatchmakingEntity(LobbyEntity lobbyEntity, int version, int mapSize, MapPreset mapPreset, GameMode gameMode, int scoreLimit, int timeLimit, Platform platform, bool allowCrossPlay, int maxPlayers)
    {
        Id = Guid.NewGuid();
        LobbyEntity = lobbyEntity;
        LobbyEntityId = lobbyEntity.Id;
        Version = version;
        MapSize = mapSize;
        MapPreset = mapPreset;
        GameMode = gameMode;
        ScoreLimit = scoreLimit;
        TimeLimit = timeLimit;
        Platform = platform;
        AllowCrossPlay = allowCrossPlay;
        MaxPlayers = maxPlayers;
        PlayerIds = lobbyEntity.Participators.Select(p => p.UserId).ToList();
    }

    public Guid Id { get; set; }

    public Guid? LobbyEntityId { get; set; }
    public virtual LobbyEntity? LobbyEntity { get; set; }

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
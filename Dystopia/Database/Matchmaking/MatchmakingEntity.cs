using Dystopia.Database.Lobby;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Matchmaking;

public class MatchmakingEntity
{
    public Guid Id { get; init; }

    public required Guid? LobbyGameViewModelId { get; init; }
    public LobbyEntity? LobbyGameViewModel { get; init; } = null!;

    public required int Version { get; init; }
    public required int MapSize { get; init; }
    public required MapPreset MapPreset { get; init; }
    public required GameMode GameMode { get; init; }
    public required int ScoreLimit { get; init; }
    public required int TimeLimit { get; init; }
    public required Platform Platform { get; init; }
    public required bool AllowCrossPlay { get; init; }

    public required int MaxPlayers { get; init; }

    public required List<Guid> PlayerIds { get; init; }
}
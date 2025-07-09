using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Game;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.Lobby;

public class LobbyEntity
{
    [Key]
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public Guid? StartedGameId { get; set; } // TODO make nullable
    public GameEntity? StartedGame { get; init; } = null!;
    
    public required MapPreset MapPreset { get; init; }
    
    public required Guid OwnerId { get; set; }
    
    public required int TimeLimit { get; init; }

    public required int ScoreLimit { get; init; }
    
    public Guid? MatchmakingGameId { get; init; }
    public MatchmakingEntity? MatchmakingGame { get; init; } = null!;
    
    public DateTime? StartTime { get; set; }
    public required List<LobbyPlayerEntity> Participators { get; init; } = null!; // TODO one-to-many relationship
    public List<int> DisabledTribes { get; set; }
    
    public required List<int> Bots { get; set; } = null!;
    public int MapSize { get; init; }
    public GameMode GameMode { get; init; }

    public LobbyGameViewModel ToLobbyGameViewModel(LobbyUpdatedReason updatedReason)
    {
        return new LobbyGameViewModel
        {
            Id = Id,
            UpdatedReason = updatedReason,
            DateCreated = DateTime.Now,
            DateModified = DateTime.Now,
            Name = Name,
            MapPreset = MapPreset,
            MapSize = MapSize,
            OpponentCount = (short)(Participators.Count - 1),
            GameMode = GameMode,
            OwnerId = OwnerId,
            DisabledTribes = DisabledTribes,
            StartedGameId = StartedGameId,
            IsPersistent = true,
            IsSharable = true,
            TimeLimit = TimeLimit,
            ScoreLimit = ScoreLimit,
            InviteLink = "https://virus.info", //TODO,
            MatchmakingGameId = 0, // TODO,
            ChallengermodeGameId = null,
            StartTime = StartTime,
            GameContext = null,
            Participators = Participators.Select(p => (ParticipatorViewModel)p).ToList(),
            Bots = Bots,
        };
    }
    
}

public static class LobbyGameViewModelExtensions
{
    public static LobbyEntity ToLobbyEntity(this LobbyGameViewModel model)
    {
        return new LobbyEntity
        {
            Id = model.Id,
            Name = model.Name,
            StartedGameId = model.StartedGameId,
            MapPreset = model.MapPreset,
            OwnerId = model.OwnerId,
            TimeLimit = model.TimeLimit,
            ScoreLimit = model.ScoreLimit,
            MatchmakingGameId = model.MatchmakingGameId != 0 ? Guid.Parse(model.MatchmakingGameId.ToString()) : null, // TODO: This is a hack, fix it
            StartTime = model.StartTime,
            Participators = model.Participators.Select(p => (LobbyPlayerEntity)p).ToList(),
            DisabledTribes = model.DisabledTribes,
            Bots = model.Bots,
            MapSize = model.MapSize,
            GameMode = model.GameMode,
        };
    }
}
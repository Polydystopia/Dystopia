using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Lobby;
using Dystopia.Database.User;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace Dystopia.Database.Game;

public class GameEntity
{
    [Key] public Guid Id { get; init; }

    public Guid LobbyId { get; init; }
    public virtual LobbyEntity Lobby { get; init; } = null!; //TODO

    public Guid? OwnerId { get; init; }
    public virtual UserEntity Owner { get; set; } = null!;

    public DateTime? DateCreated { get; init; }
    public DateTime? DateLastCommand { get; set; }

    public GameSessionState State { get; set; }
    public string GameSettings { get; init; } = null!; // TODO make it ownsone in builder TODO make custom entity

    public byte[]? InitialGameStateData { get; init; }

    public byte[]? CurrentGameStateData { get; set; }

    public TimerSettings TimerSettings { get; init; } = null!;

    public DateTime? DateCurrentTurnDeadline { get; init; }

    public virtual ICollection<GameParticipatorUserUser> Participators { get; set; }
        = new List<GameParticipatorUserUser>();

    public Guid? ExternalTournamentId { get; init; }

    public Guid? ExternalMatchId { get; init; }
}

public static class GameMappingExtensions
{
    public static GameViewModel ToViewModel(this GameEntity e)
    {
        return new GameViewModel
        {
            Id = e.Id,
            OwnerId = e.OwnerId,
            DateCreated = e.DateCreated,
            DateLastCommand = e.DateLastCommand,
            State = e.State,
            GameSettingsJson = e.GameSettings,
            InitialGameStateData = e.InitialGameStateData,
            CurrentGameStateData = e.CurrentGameStateData,
            TimerSettings = e.TimerSettings,
            DateCurrentTurnDeadline = e.DateCurrentTurnDeadline,
            GameContext = new GameContext()
                { ExternalMatchId = e.ExternalMatchId, ExternalTournamentId = e.ExternalTournamentId }
        };
    }
}
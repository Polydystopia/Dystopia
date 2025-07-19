using System.ComponentModel.DataAnnotations;
using Dystopia.Database.Lobby;
using Dystopia.Database.User;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Timers;

namespace Dystopia.Database.Game;

public class GameEntity
{
    [Key]
    public Guid Id { get; init; }

    public LobbyEntity Lobby { get; init; } = null!;
    public Guid? OwnerId { get; init; }
    public UserEntity Owner { get; init; } = null!;

    public DateTime? DateCreated { get; init; }
    public DateTime? DateLastCommand { get; set; }
    public string GameSettings { get; init; } = null!; // TODO make it ownsone in builder TODO make custom entity

    public byte[]? InitialGameStateData { get; init; }

    public byte[]? CurrentGameStateData { get; set; }

    public TimerSettings TimerSettings { get; init; } = null!;

    public DateTime? DateCurrentTurnDeadline { get; init;}

    public static explicit operator GameViewModel(GameEntity v)
    {
        return new GameViewModel
        {
            Id = v.Id,
            OwnerId = v.OwnerId,
            DateCreated = v.DateCreated,
            DateLastCommand = v.DateLastCommand,
            State = GameSessionState.Started,
            GameSettingsJson = v.GameSettings,
            InitialGameStateData = v.InitialGameStateData,
            CurrentGameStateData = v.CurrentGameStateData,
            TimerSettings = v.TimerSettings,
            DateCurrentTurnDeadline = v.DateCurrentTurnDeadline,
        };
    }

    public static explicit operator GameEntity(GameViewModel v)
    {
        return new GameEntity
        {
            Id = v.Id,
            OwnerId = v.OwnerId,
            DateCreated = v.DateCreated,
            DateLastCommand = v.DateLastCommand,
            GameSettings = v.GameSettingsJson,
            InitialGameStateData = v.InitialGameStateData,
            CurrentGameStateData = v.CurrentGameStateData,
            TimerSettings = v.TimerSettings,
            DateCurrentTurnDeadline = v.DateCurrentTurnDeadline,
        };
    }
}
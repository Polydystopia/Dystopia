using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Game;

public interface IPolydystopiaGameManager
{
    Task<bool> CreateGame(LobbyEntity lobby);

    Task<bool> Resign(ResignBindingModel model, Guid senderId);

    Task<bool> SendCommand(SendCommandBindingModel commandBindingModel, Guid senderId);

    GameSummaryViewModel GetGameSummaryViewModelByGameViewModel(GameViewModel game);
}
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Game;

public interface IPolydystopiaGameManager
{
    Task<bool> CreateGame(LobbyGameViewModel lobby);

    Task<bool> Resign(ResignBindingModel model, Guid senderId);

    Task<bool> SendCommand(SendCommandBindingModel commandBindingModel, Guid senderId);

    GameSummaryViewModel GetGameSummaryViewModelByGameViewModel(GameViewModel game);
}
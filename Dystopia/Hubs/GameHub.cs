using Dystopia.Game;
using Dystopia.Patches;
using Newtonsoft.Json;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<GameListingViewModel>> GetGameListingsV3()
    {
        var response = new GameListingViewModel();
        response.gameSummaries = new List<GameSummaryViewModel>();
        response.matchmakingGameSummaries = new List<MatchmakingGameSummaryViewModel>(); //TODO

        var myGames = await _gameRepository.GetAllGamesByPlayer(UserGuid);
        foreach (var game in myGames)
        {
            response.gameSummaries.Add(PolydystopiaGameManager.GetGameSummaryViewModelByGameViewModel(game));
        }

        return new ServerResponse<GameListingViewModel>(response);
    }

    public async Task<ServerResponse<ResponseViewModel>> Resign(ResignBindingModel model)
    {
        var res = await PolydystopiaGameManager.Resign(model, _gameRepository, UserGuid);

        if (!res)
        {
            _logger.LogWarning("Resign by {playerId} in game {gameId} failed", UserId, model.GameId);
        }

        return res ? new ServerResponse<ResponseViewModel>(new ResponseViewModel()) : new ServerResponse<ResponseViewModel>();
    }

    public async Task<ServerResponse<ResponseViewModel>> SendCommand(SendCommandBindingModel model)
    {
        var res = await PolydystopiaGameManager.SendCommand(model, _gameRepository, UserGuid);

        return res ? new ServerResponse<ResponseViewModel>(new ResponseViewModel()) : new ServerResponse<ResponseViewModel>();
    }

    public async Task<ServerResponse<ResponseViewModel>> SetParticipationDone(
        SetParticipationDoneBindingModel model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<GameSummaryViewModel>> GetRecentGames(
        RecentGamesBindingModel model)
    {
        var list = new List<GameSummaryViewModel>();
        return new ServerResponseList<GameSummaryViewModel>(list);
    }

    public async Task<ServerResponseList<GameSummaryViewModel>> GetSavedGames()
    {
        var list = new List<GameSummaryViewModel>();
        return new ServerResponseList<GameSummaryViewModel>(list);
    }
}
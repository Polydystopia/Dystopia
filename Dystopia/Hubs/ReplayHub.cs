using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponseList<GameSummaryViewModel>> GetRecentGames(
        RecentGamesBindingModel model)
    {
        var summaries = new List<GameSummaryViewModel>();

        var games = await _gameRepository.GetLastEndedGamesByPlayer(_userGuid, model.Limit);

        foreach (var gameViewModel in games)
        {
            var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(gameViewModel);

            summaries.Add(summary);
        }

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }

    public async Task<ServerResponse<ResponseViewModel>> SaveGame(SaveGameBindingModel model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<GameSummaryViewModel>> GetSavedGames()
    {
        var summaries = new List<GameSummaryViewModel>();

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }
}
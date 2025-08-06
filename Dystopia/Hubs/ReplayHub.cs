using Dystopia.Database.Game;
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
            var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(gameViewModel.ToViewModel());

            summaries.Add(summary);
        }

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }

    public async Task<ServerResponse<ResponseViewModel>> SaveGame(SaveGameBindingModel model)
    {
        if (model.Save)
        {
            await _gameRepository.AddFavoriteAsync(_userGuid, model.GameId);
        }
        else
        {
            await _gameRepository.RemoveFavoriteAsync(_userGuid, model.GameId);
        }

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<GameSummaryViewModel>> GetSavedGames()
    {
        var games = await _gameRepository.GetFavoriteGamesByPlayer(_userGuid);

        var summaries = new List<GameSummaryViewModel>();

        foreach (var game in games)
        {
            var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(game.ToViewModel());

            summaries.Add(summary);
        }

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }
}
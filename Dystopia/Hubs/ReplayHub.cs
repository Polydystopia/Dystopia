using Dystopia.Database.Game;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;

namespace Dystopia.Hubs;

public partial class DystopiaHub
{
    public async Task<ServerResponseList<GameSummaryViewModel>> GetRecentGames(
        RecentGamesBindingModel model)
    {
        var user = await _userRepository.GetByIdAsync(_userGuid);

        if (user == null)
        {
            _logger.LogWarning("Get recent games failed: User not found. UserId={userId}", _userGuid);

            return new ServerResponseList<GameSummaryViewModel>(ErrorCode.UserNotFound, "User not found.");
        }

        var games = await _gameRepository.GetLastEndedGamesByPlayer(user, model.Limit);

        var summaries = new List<GameSummaryViewModel>();
        foreach (var gameViewModel in games)
        {
            var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(gameViewModel.ToViewModel());

            summaries.Add(summary);
        }

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }

    public async Task<ServerResponse<ResponseViewModel>> SaveGame(SaveGameBindingModel model)
    {
        var user = await _userRepository.GetByIdAsync(_userGuid);
        var game = await _gameRepository.GetByIdAsync(model.GameId);

        if (user == null)
        {
            _logger.LogWarning("Save as favorite failed: User not found. UserId={userId}", _userGuid);

            return new ServerResponse<ResponseViewModel>(ErrorCode.UserNotFound, "User not found.");
        }

        if (game == null)
        {
            _logger.LogWarning("Save as favorite failed: Game not found. GameId={gameId}", model.GameId);

            return new ServerResponse<ResponseViewModel>(ErrorCode.GameNotFound, "Game not found.");
        }

        if (model.Save)
        {
            await _userRepository.AddFavoriteAsync(user, game);
        }
        else
        {
            await _userRepository.RemoveFavoriteAsync(user, game);
        }

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<GameSummaryViewModel>> GetSavedGames()
    {
        var user = await _userRepository.GetByIdAsync(_userGuid);

        if (user == null)
        {
            _logger.LogWarning("Get saved games of user failed: User not found. UserId={userId}", _userGuid);

            return new ServerResponseList<GameSummaryViewModel>(ErrorCode.UserNotFound, "User not found.");
        }

        var games = await _gameRepository.GetFavoriteGamesByPlayer(user);

        var summaries = new List<GameSummaryViewModel>();

        foreach (var game in games)
        {
            var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(game.ToViewModel());

            summaries.Add(summary);
        }

        return new ServerResponseList<GameSummaryViewModel>(summaries);
    }
}
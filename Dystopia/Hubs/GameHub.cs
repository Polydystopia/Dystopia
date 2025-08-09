using Dystopia.Database.Game;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<GameListingViewModel>> GetGameListingsV3()
    {
        var response = new GameListingViewModel();
        response.gameSummaries = new List<GameSummaryViewModel>();
        response.matchmakingGameSummaries = new List<MatchmakingGameSummaryViewModel>(); //TODO

        var user = await _userRepository.GetByIdAsync(_userGuid);
        if (user == null)
        {
            _logger.LogWarning("User {userId} not found.", _userGuid);
            return new ServerResponse<GameListingViewModel>(ErrorCode.UserNotFound, "User not found.");
        }

        var myGames = await _gameRepository.GetAllGamesByPlayer(user);
        foreach (var game in myGames)
        {
            if (game.State != GameSessionState.Started) continue;

            response.gameSummaries.Add(_gameManager.GetGameSummaryViewModelByGameViewModel(game.ToViewModel()));
        }

        return new ServerResponse<GameListingViewModel>(response);
    }

    public async Task<ServerResponse<ResponseViewModel>> Resign(ResignBindingModel model)
    {
        var res = await _gameManager.Resign(model, _userGuid);

        if (!res)
        {
            _logger.LogWarning("Resign by {playerId} in game {gameId} failed", _userId, model.GameId);
        }

        return res ? new ServerResponse<ResponseViewModel>(new ResponseViewModel()) : new ServerResponse<ResponseViewModel>();
    }

    public async Task<ServerResponse<ResponseViewModel>> SendCommand(SendCommandBindingModel model)
    {
        var res = await _gameManager.SendCommand(model, _userGuid);

        return res ? new ServerResponse<ResponseViewModel>(new ResponseViewModel()) : new ServerResponse<ResponseViewModel>();
    }

    public async Task<ServerResponse<ResponseViewModel>> SetParticipationDone(
        SetParticipationDoneBindingModel model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
}
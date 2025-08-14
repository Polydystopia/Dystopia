using Dystopia.Database.Game;
using Microsoft.AspNetCore.Mvc;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

namespace Dystopia.Controllers;

[ApiController]
[Route("api/game")]
public class GameController(IPolydystopiaGameRepository gameRepository, ILogger<GameController> logger)
    : ControllerBase
{
    [Route("upload_numsingleplayergames")]
    public ServerResponse<ResponseViewModel> UploadNumSingleplayerGames([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("upload_triberating")]
    public ServerResponse<ResponseViewModel> UploadTribeRating([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("get_triberating")]
    public ServerResponse<ResponseViewModel> GetTribeRating([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("join_game")]
    public async Task<ServerResponse<GameViewModel>> JoinGame([FromBody] JoinGameBindingModel model)
    {
        var gameEntity = await gameRepository.GetByIdAsync(model.GameId);

        if (gameEntity == null)
        {
            return new ServerResponse<GameViewModel>() { Success = false };
        }

        return new ServerResponse<GameViewModel>(gameEntity.ToViewModel());
    }

    [Route("upload_highscores")]
    public async Task<ServerResponse<ResponseViewModel>> UploadHighscores(
        [FromBody] UploadHighscoresBindingModel model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("spectate_game")]
    public async Task<ServerResponse<GameViewModel>> SpectateGame([FromBody] SpectateGameBindingModel model)
    {
        var gameEntity = await gameRepository.GetByIdAsync(model.GameId);

        if (gameEntity == null)
        {
            return new ServerResponse<GameViewModel>()
                { Success = false, ErrorCode = ErrorCode.GameNotFound, ErrorMessage = "Game not found." };
        }

        return new ServerResponse<GameViewModel>(gameEntity.ToViewModel());
    }
}
using System.Globalization;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Database.WeeklyChallenge;
using Dystopia.Managers.Highscore;
using Dystopia.Models.Util;
using Dystopia.Models.WeeklyChallenge;
using Dystopia.Models.WeeklyChallenge.League;
using Dystopia.Services.WeeklyChallenge;
using Microsoft.AspNetCore.Mvc;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

namespace Dystopia.Controllers;

[ApiController]
[Route("api/game")]
public class GameController(
    IPolydystopiaGameRepository gameRepository,
    IPolydystopiaUserRepository userRepository,
    IDystopiaHighscoreManager highscoreManager,
    IWeeklyChallengeRepository weeklyChallengeRepository,
    ILogger<GameController> logger)
    : ControllerBase
{
    private string _userId => HttpContext.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private Guid _userGuid => Guid.Parse(_userId);

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
        var user = await userRepository.GetByIdAsync(_userGuid);

        if (user == null) return new ServerResponse<ResponseViewModel>(ErrorCode.UserNotFound, "User not found.");

        var success = highscoreManager.ProcessHighscore(model, user);

        if (success)
        {
            return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
        }
        else
        {
            return new ServerResponse<ResponseViewModel>(ErrorCode.InvalidUserCommand, "Invalid highscore.");
        }
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

    [Route("get_weekly_challenge_data")]
    public async Task<ServerResponse<DystopiaWeeklyChallengeViewModel>> GetWeeklyChallengeData(
        [FromBody] DystopiaWeeklyChallengeBindingModel model)
    {
        var user = await userRepository.GetByIdAsync(_userGuid);
        if (user == null) return new ServerResponse<DystopiaWeeklyChallengeViewModel>(ErrorCode.UserNotFound, "User not found.");

        var currentChallenge = await weeklyChallengeRepository.GetByWeekAsync(WeeklyChallengeSchedulerService.GetCompositeWeekNumber(model.Date));
        if(currentChallenge == null) return new ServerResponse<DystopiaWeeklyChallengeViewModel>(DystopiaErrorCode.GameNotFound.Map(), "WeeklyChallenge not found.");

        var ownChallengeEntry = user.WeeklyChallengeEntries.FirstOrDefault(w => w.Id == currentChallenge.Id);

        var weeklyChallengeViewModel = new DystopiaWeeklyChallengeViewModel()
        {
            CurrentServerTime = DateTime.Now,
            Week = ISOWeek.GetWeekOfYear(DateTime.Today),
            WeeklyChallengeTemplateMetadataViewModel = new DystopiaWeeklyChallengeTemplateMetadataViewModel()
            {
                Name = currentChallenge.Name,
                TribeType = (byte)currentChallenge.Tribe,
                TribeSkin = (short)currentChallenge.SkinType,
                GameVersion = currentChallenge.GameVersion,
                DiscordLink = currentChallenge.DiscordLink,
            },
            HasPersonalData = true,//ownChallengeEntry != null,
            LeagueId = currentChallenge.Id,
            WeeklyChallengeHighscoreViewModels = new Dictionary<int, List<DystopiaWeeklyChallengeHighscoreViewModel>>()
            {

            },
            LeagueHighscoreViewModels = new List<DystopiaLeagueHighscoreViewModel>()
            {

            },
            WeeklyChallengeEntryViewModels = new List<DystopiaWeeklyChallengeEntryViewModel>()
            {

            },
            Rank = -1,
            PromotionState = DystopiaPromotionState.None,
        };

        return new ServerResponse<DystopiaWeeklyChallengeViewModel>(weeklyChallengeViewModel);
    }
}
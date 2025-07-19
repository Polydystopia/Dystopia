using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Dystopia.Database.Game;
using Dystopia.Database.User;
using Dystopia.Services.News;
using Dystopia.Services.Steam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Polytopia.Data;
using Dystopia.Database;
using Dystopia.Models;
using Dystopia.Patches;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode.Matchmaking;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Notifications;
using SteamKit2;
using SteamTicketDecrypt.Console;
using Steamworks;

namespace Dystopia.Controllers;

[ApiController]
public class PolytopiaController : ControllerBase
{
    private readonly IPolydystopiaGameRepository _gameRepository;

    private readonly INewsService _newsService;

    private readonly ILogger<PolytopiaController> _logger;

    public PolytopiaController(IPolydystopiaGameRepository gameRepository,
        INewsService newsService, ILogger<PolytopiaController> logger)
    {
        _gameRepository = gameRepository;
        _newsService = newsService;
        _logger = logger;
    }

    [Route("api/start/get_versioning")]
    public async Task<ServerResponse<VersioningViewModel>> GetVersioning([FromBody] VersioningBindingModel bindingModel)
    {
        var versioningViewModel = new VersioningViewModel();

        versioningViewModel.SystemMessage = await _newsService.GetSystemMessage();

        versioningViewModel.VersionEnabledStatuses = new List<VersionEnabledStatus>() //TODO Find out what these do
        {
            new() { Enabled = true, Message = null, Feature = VersionedFeature.App },
            new() { Enabled = true, Message = null, Feature = VersionedFeature.Network },
            new() { Enabled = true, Message = null, Feature = VersionedFeature.NewMultiplayer }
        };

        var response = new ServerResponse<VersioningViewModel>(versioningViewModel);

        return response;
    }

    [Route("api/game/upload_numsingleplayergames")]
    public ServerResponse<ResponseViewModel> UploadNumSingleplayerGames([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("api/game/upload_triberating")]
    public ServerResponse<ResponseViewModel> UploadTribeRating([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("api/start/get_start_viewmodel")]
    public ServerResponse<StartViewModel> GetStartViewModel([FromBody] object model) //TODO
    {
        return new ServerResponse<StartViewModel>(new StartViewModel()
        {
            ActionableGamesCount = 0,
            UnseenNewsItemCount = 1
        });
    }

    [Route("api/news/get_news")]
    public async Task<ServerResponse<NewsObject>> GetNews([FromBody] object startDate) //TODO respect startDate
    {
        var news = new NewsObject();

        news.News = await _newsService.GetNews();

        return new ServerResponse<NewsObject>(news);
    }

    [Route("api/game/get_triberating")]
    public ServerResponse<ResponseViewModel> GetTribeRating([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("api/game/join_game")]
    public async Task<ServerResponse<GameViewModel>> JoinGame([FromBody] JoinGameBindingModel model)
    {
        var gameViewModel = await _gameRepository.GetByIdAsync(model.GameId);

        if (gameViewModel == null)
        {
            return new ServerResponse<GameViewModel>() { Success = false };
        }

        return new ServerResponse<GameViewModel>((GameViewModel)gameViewModel);
    }

    [Route("api/cm/list_matchmaking ")]
    public async Task<ServerResponseList<TournamentMatchmakingQueueViewModel>> ListMatchmakingQueues() //TODO
    {
        var tournamentMatchmakingQueues = new List<TournamentMatchmakingQueueViewModel>();

        return new ServerResponseList<TournamentMatchmakingQueueViewModel>(tournamentMatchmakingQueues);
    }
}
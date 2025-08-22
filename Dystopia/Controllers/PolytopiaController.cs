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
using Dystopia.Database.WeeklyChallenge.League;
using Dystopia.Models;
using Dystopia.Models.Start;
using Dystopia.Models.Versioning;
using Dystopia.Models.WeeklyChallenge.League;
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
    private readonly ILeagueRepository _leagueRepository;
    private readonly IPolydystopiaUserRepository _userRepository;

    private readonly INewsService _newsService;

    private readonly ILogger<PolytopiaController> _logger;

    private string _userId => HttpContext.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private Guid _userGuid => Guid.Parse(_userId);

    public PolytopiaController(IPolydystopiaGameRepository gameRepository, ILeagueRepository leagueRepository,
        IPolydystopiaUserRepository userRepository,
        INewsService newsService, ILogger<PolytopiaController> logger)
    {
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _leagueRepository = leagueRepository;
        _newsService = newsService;
        _logger = logger;
    }

    [Route("api/start/get_versioning")]
    public async Task<ServerResponse<DystopiaVersioningViewModel>> GetVersioning(
        [FromBody] VersioningBindingModel bindingModel)
    {
        var versioningViewModel = new DystopiaVersioningViewModel();

        versioningViewModel.SystemMessage =
            await _newsService.GetSystemMessage() +
            $"\n\n{Guid.NewGuid()}"; // We need to add a random value to the end since the client caches the system message by value and does not show an already cached one

        versioningViewModel.VersionEnabledStatuses = new List<DystopiaVersionEnabledStatus>()
        {
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.App },
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.Network },
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.NewMultiplayer },
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.NewMatchmaking },
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.Highscores },
            new() { Enabled = true, Message = null, Feature = DystopiaVersionedFeature.WeeklyChallenge },
        };

        var response = new ServerResponse<DystopiaVersioningViewModel>(versioningViewModel);

        return response;
    }

    [Route("api/start/get_start_viewmodel")]
    public async Task<ServerResponse<DystopiaStartViewModel>> GetStartViewModel([FromBody] object model) //TODO
    {
        var leagues = await _leagueRepository.GetAllAsync();

        var user = await _userRepository.GetByIdAsync(_userGuid);
        if (user == null) return new ServerResponse<DystopiaStartViewModel>(ErrorCode.UserNotFound, "User not found.");

        return new ServerResponse<DystopiaStartViewModel>(new DystopiaStartViewModel()
        {
            ActionableGamesCount = 0,
            UnseenNewsItemCount = 1,
            LeagueId = user.CurrentLeagueId,
            LeagueViewModels = leagues.ToViewModels(),
            LastSeenWeeklyChallengeDate = DateTime.MinValue, //TODO
            LastWeeklyChallengeEntryDate = DateTime.MinValue, //TODO
        });
    }

    [Route("api/news/get_news")]
    public async Task<ServerResponse<NewsObject>> GetNews([FromBody] object startDate) //TODO respect startDate
    {
        var news = new NewsObject();

        news.News = (await _newsService.GetNews()).ToList();

        return new ServerResponse<NewsObject>(news);
    }

    [Route("api/cm/list_matchmaking ")]
    public async Task<ServerResponseList<TournamentMatchmakingQueueViewModel>> ListMatchmakingQueues() //TODO
    {
        var tournamentMatchmakingQueues = new List<TournamentMatchmakingQueueViewModel>();

        return new ServerResponseList<TournamentMatchmakingQueueViewModel>(tournamentMatchmakingQueues);
    }
}
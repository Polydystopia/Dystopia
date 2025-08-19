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
    public async Task<ServerResponse<DystopiaVersioningViewModel>> GetVersioning([FromBody] VersioningBindingModel bindingModel)
    {
        var versioningViewModel = new DystopiaVersioningViewModel();

        versioningViewModel.SystemMessage = await _newsService.GetSystemMessage();

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
    public ServerResponse<DystopiaStartViewModel> GetStartViewModel([FromBody] object model) //TODO
    {
        return new ServerResponse<DystopiaStartViewModel>(new DystopiaStartViewModel()
        {
            ActionableGamesCount = 0,
            UnseenNewsItemCount = 1,
            LeagueId = 1,
            LeagueViewModels = new List<LeagueViewModel>()
            {
                new()
                {
                    Id = 1,
                    Name = "Entry League",
                    LocalizationKey = "league.name.entry",
                    PrimaryColor = 46334,
                    SecondaryColor = 10739966,
                    TertiaryColor = 4832768,
                    PromotionRate = 0.5f,
                    DemotionRate = 0,
                    IsEntry = true,
                    IsFriendsLeague = false,
                },
                new()
                {
                    Id = 2,
                    Name = "Friends",
                    LocalizationKey = "league.name.friends",
                    PrimaryColor = 6316128,
                    SecondaryColor = 2302755,
                    TertiaryColor = 15461355,
                    PromotionRate = 0,
                    DemotionRate = 0,
                    IsEntry = false,
                    IsFriendsLeague = true,
                }
            },
            LastSeenWeeklyChallengeDate = DateTime.MinValue,
            LastWeeklyChallengeEntryDate = DateTime.MinValue,
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
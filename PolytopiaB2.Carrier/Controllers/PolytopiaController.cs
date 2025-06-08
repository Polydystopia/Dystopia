using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Database;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaB2.Carrier.Models;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Notifications;
using SteamTicketDecrypt.Console;
using Steamworks;

namespace PolytopiaB2.Carrier.Controllers;

[ApiController]
public class PolytopiaController : ControllerBase
{
    private readonly IPolydystopiaUserRepository _userRepository;
    private readonly IPolydystopiaGameRepository _gameRepository;

    public PolytopiaController(IPolydystopiaUserRepository userRepository, IPolydystopiaGameRepository gameRepository)
    {
        _userRepository = userRepository;
        _gameRepository = gameRepository;
    }

    [Route("api/start/get_versioning")]
    public ServerResponse<VersioningViewModel> GetVersioning([FromBody] VersioningBindingModel bindingModel)
    {
        var versioningViewModel = new VersioningViewModel();
        versioningViewModel.SystemMessage = "Custom server WIP\nPlease report any bugs to the developers at\nhttps://github.com/Polydystopia/curly-octo-waffle\n\ndiscord: juli.gg\nmail: polydystopia@juli.gg\n\nHave fun!\n" + Guid.NewGuid();

        versioningViewModel.VersionEnabledStatuses = new List<VersionEnabledStatus>() //TODO Find out what these do
        {
            new() { Enabled = true, Message = null, Feature = VersionedFeature.App },
            new() { Enabled = true, Message = null, Feature = VersionedFeature.Network },
            new() { Enabled = true, Message = null, Feature = VersionedFeature.NewMultiplayer }
        };

        var response = new ServerResponse<VersioningViewModel>(versioningViewModel);

        return response;
    }

    [Route("api/auth/login_steam")]
    public async Task<IActionResult> LoginSteam([FromBody] SteamLoginBindingModel? model)
    {
        if (model?.SteamAuthTicket?.Data == null)
        {
            return BadRequest("Invalid auth ticket data");
        }

        var parsedSteamTicket = AppTicketParser.ParseAppTicket(model.SteamAuthTicket.Data);

        var username = "Player " + Guid.NewGuid(); //TODO: get username from steam

        var userFromDb = await _userRepository.GetBySteamIdAsync(parsedSteamTicket.SteamID, username);
        
        var token = new PolytopiaToken();

        var claims = new List<Claim>
        {
            new("nameid", userFromDb.PolytopiaId.ToString()),
            new("unique_name", userFromDb.GetUniqueNameInternal()),
            new("AspNet.Identity.SecurityStamp", "PCSD6HQ3RTGJDIWAT4BBJY3IFW5ARY3J"), //TODO: what is this?
            new("steam", userFromDb.SteamId)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("higul0u9pgwojaingwagvupöjoahg8wag890zuahgvbuaagau9j")); //TODO: key
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var now = DateTime.UtcNow;
        var expiryDate = now.AddDays(1);

        var jwtToken = new JwtSecurityToken(
            issuer: "Issuer",
            audience: "Audience",
            claims: claims,
            notBefore: now,
            expires: expiryDate,
            signingCredentials: creds
        );
        
        var tokenHandler = new JwtSecurityTokenHandler();
        
        token.JwtToken = tokenHandler.WriteToken(jwtToken);
        token.ExpiresAt = expiryDate;
        
        token.User = userFromDb;

        var json = JsonConvert.SerializeObject(new ServerResponse<PolytopiaToken>(token));

        return Content(json, "application/json");
    }

    [Route("api/auth/steam_notifications")]
    public ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse> ActivateSteamNotifications(
        [FromBody] SteamNotificationsModels.RequestSteamNotificationsBindingModel model) //TODO
    {
        return new ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse>(
            new SteamNotificationsModels.RequestSteamNotificationsResponse()
            {
                Was_Created = false,
                Allow_Notifications = true
            });
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
    public ServerResponse<NewsObject> GetNews([FromBody] object startDate) //TODO
    {
        var news = new NewsObject();
        news.News = new List<NewsItem>();

        var item = new NewsItem();
        item.Id = 2;
        var specificDate2 = new DateTime(2025, 6, 8, 0, 0, 0, DateTimeKind.Utc);
        item.Date = new DateTimeOffset(specificDate2).ToUnixTimeSeconds();

        item.Body = "Custom server WIP\nPlease report any bugs to the developers at\nhttps://github.com/Polydystopia/curly-octo-waffle\n\ndiscord: juli.gg\nmail: polydystopia@juli.gg\n\nHave fun!";
        item.Link = "https://github.com/Polydystopia/curly-octo-waffle";

        item.Image = @"https://avatars.githubusercontent.com/u/120461041";

        news.News.Add(item);

        return new ServerResponse<NewsObject>(news);
    }

    [Route("api/game/get_triberating")]
    public ServerResponse<ResponseViewModel> GetTribeRating([FromBody] object model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    //[Route("api/auth/steam_notifications")]
    //public ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse> SteamNotifications(
    //    [FromBody] SteamNotificationsModels.RequestSteamNotificationsBindingModel model)
    //{
    //    return new ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse>(
    //        new SteamNotificationsModels.RequestSteamNotificationsResponse()
    //        {
    //            Was_Created = true,
    //            Allow_Notifications = true
    //        });
    //}

    [Route("api/game/join_game")]
    public async Task<ServerResponse<GameViewModel>> JoinGame([FromBody] JoinGameBindingModel model)
    {
        var gameViewModel = await _gameRepository.GetByIdAsync(model.GameId);

        if (gameViewModel == null)
        {
            return new ServerResponse<GameViewModel>() {Success = false};
        }
        
        return new ServerResponse<GameViewModel>(gameViewModel);
    }

    [Route("/api/auth/whoami")]
    public async Task<ServerResponse<PolytopiaToken>> WhoAmI() //TODO
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }
}
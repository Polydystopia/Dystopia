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
        //versioningViewModel.SystemMessage = "Private server by Paranoia";

        versioningViewModel.VersionEnabledStatuses = new List<VersionEnabledStatus>()
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

        var userFromDb = await _userRepository.GetBySteamIdAsync(parsedSteamTicket.SteamID);
        
        var token = new PolytopiaToken();

        var claims = new List<Claim>
        {
            new("nameid", userFromDb.PolytopiaId.ToString()),
            new("unique_name", userFromDb.SteamId), //TODO
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
        [FromBody] SteamNotificationsModels.RequestSteamNotificationsBindingModel model)
    {
        return new ServerResponse<SteamNotificationsModels.RequestSteamNotificationsResponse>(
            new SteamNotificationsModels.RequestSteamNotificationsResponse()
            {
                Was_Created = false,
                Allow_Notifications = true
            });
    }

    [Route("api/game/upload_numsingleplayergames")]
    public ServerResponse<ResponseViewModel> UploadNumSingleplayerGames([FromBody] object model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("api/game/upload_triberating")]
    public ServerResponse<ResponseViewModel> UploadTribeRating([FromBody] object model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    [Route("api/start/get_start_viewmodel")]
    public ServerResponse<StartViewModel> GetStartViewModel([FromBody] object model)
    {
        return new ServerResponse<StartViewModel>(new StartViewModel()
        {
            ActionableGamesCount = 69,
            UnseenNewsItemCount = 111
        });
    }

    [Route("api/game/get_triberating")]
    public ServerResponse<ResponseViewModel> GetTribeRating([FromBody] object model)
    {
        //TODO
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

    public static HotseatClient client;

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
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dystopia.Database.User;
using Dystopia.Services.Steam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Notifications;

namespace Dystopia.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IPolydystopiaUserRepository _userRepository;

    private readonly ISteamService _steamService;

    private readonly ILogger<AuthController> _logger;

    public AuthController(IPolydystopiaUserRepository userRepository, ISteamService steamService, ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _steamService = steamService;
        _logger = logger;
    }

    [Route("login_steam")]
    public async Task<IActionResult> LoginSteam([FromBody] SteamLoginBindingModel? model)
    {
        if (model?.SteamAuthTicket?.Data == null)
        {
            _logger.LogInformation(
                "Steam login attempt from IP address {RemoteIpAddress} failed. Invalid auth ticket data",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return BadRequest("Invalid auth ticket data");
        }

        var parsedSteamTicket = _steamService.ParseTicket(model.SteamAuthTicket.Data, model.DeviceId);
        if (parsedSteamTicket == null)
        {
            _logger.LogInformation(
                "Steam login attempt from IP address {RemoteIpAddress} failed. Invalid auth ticket data",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
            return Forbid("Invalid auth ticket data");
        }

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); //TODO: Hack use DI later
        var isDevEnv = string.Equals(env, Environments.Development, StringComparison.OrdinalIgnoreCase);

        var username = !isDevEnv ? await _steamService.GetSteamUsernameAsync(parsedSteamTicket.SteamID) : model.DeviceId;
        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Could not get steam username for steamId {steamId}", parsedSteamTicket.SteamID);;
            return BadRequest("Username parse error.");
        }

        var userFromDb = await _userRepository.GetBySteamIdAsync(parsedSteamTicket.SteamID, username);

        _logger.LogInformation("Steam login attempt from IP address {RemoteIpAddress}. User {UserName}",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            userFromDb.GetUniqueNameInternal() ?? "Unknown");

        var token = new PolytopiaToken();

        var claims = new List<Claim>
        {
            new("nameid", userFromDb.PolytopiaId.ToString()),
            new("unique_name", userFromDb.GetUniqueNameInternal()),
            new("AspNet.Identity.SecurityStamp", "PCSD6HQ3RTGJDIWAT4BBJY3IFW5ARY3J"), //TODO: what is this?
            new("steam", userFromDb.SteamId)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("higul0u9pgwojaingwagvupöjoahg8wag890zuahgvbuaagau9j")); //TODO: key
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

    public async Task<ServerResponse<PolytopiaToken>> LoginGooglePlay(
        LoginGooglePlayBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginNintendoServiceAccount(
        LoginNintendoServiceAccountBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginTesla(LoginTeslaBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginIos(LoginGameCenterBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginIosV2(LoginGameCenterV2BindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginFake(LoginFakeBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    public async Task<ServerResponse<PolytopiaToken>> LoginDebugLegacyUser(LoginFakeBindingModel model)
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }

    [Route("steam_notifications")]
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

    [Route("whoami")]
    public async Task<ServerResponse<PolytopiaToken>> WhoAmI() //TODO
    {
        return new ServerResponse<PolytopiaToken>(new PolytopiaToken());
    }
}
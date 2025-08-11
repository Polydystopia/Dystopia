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
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Notifications;

namespace Dystopia.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IPolydystopiaUserRepository _userRepository;

    private readonly ISteamService _steamService;

    private readonly ILogger<AuthController> _logger;

    public AuthController(IPolydystopiaUserRepository userRepository, ISteamService steamService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _steamService = steamService;
        _logger = logger;
    }

    private PolytopiaToken CreateToken(UserEntity userFromDb)
    {
        var token = new PolytopiaToken();

        var claims = new List<Claim>
        {
            new("nameid", userFromDb.Id.ToString()),
            new("unique_name", userFromDb.Alias),
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

        token.User = userFromDb.ToViewModel();

        return token;
    }

    private async Task UpdateClientVersions(LoginBaseBindingModel model, Platform platform, UserEntity userFromDb)
    {
        if(model.GameVersion == null) return;

        if (!userFromDb.GameVersions.Any(g => g.DeviceId == model.DeviceId && g.GameVersion == model.GameVersion))
        {
            userFromDb.GameVersions.Add(new ClientGameVersionViewModel()
            {
                DeviceId = model.DeviceId,
                GameVersion = (int)model.GameVersion,
                Platform = platform
            });

            await _userRepository.UpdateAsync(userFromDb);
        }
    }

    [Route("login_steam")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginSteam([FromBody] SteamLoginBindingModel? model)
    {
        if (model?.SteamAuthTicket?.Data == null)
        {
            _logger.LogInformation(
                "Steam login attempt failed. Invalid auth ticket data");
            return BadRequest("Invalid auth ticket data");
        }

        var parsedSteamTicket = _steamService.ParseTicket(model.SteamAuthTicket.Data, model.DeviceId);
        if (parsedSteamTicket == null)
        {
            _logger.LogInformation(
                "Steam login attempt failed. Invalid auth ticket data");
            return Forbid("Invalid auth ticket data");
        }

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); //TODO: Hack use DI later

        var isDevEnv = string.Equals(env, Environments.Development, StringComparison.OrdinalIgnoreCase);

        var username = !isDevEnv
            ? await _steamService.GetSteamUsernameAsync(parsedSteamTicket.SteamID)
            : PolyUsernameGenerator.GetGeneratedUsername(model.DeviceId);
        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Could not get steam username for steamId {steamId}", parsedSteamTicket.SteamID);
            ;
            return BadRequest("Username parse error.");
            // shouldn't we generate a random username in this case?
        }

        var userFromDb = await _userRepository.GetBySteamIdAsync(parsedSteamTicket.SteamID, username);

        await UpdateClientVersions(model, Platform.Steam, userFromDb);

        var token = CreateToken(userFromDb);

        var json = JsonConvert.SerializeObject(new ServerResponse<PolytopiaToken>(token));

        return Content(json, "application/json");
    }


    [Route("login_google_play")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginGooglePlay(
        LoginGooglePlayBindingModel model)
    {
        //TODO we need to find a way to properly handle model.AuthCode. Maybe it will not be possible to use the original one and we have to patch the game app. For now we will use the deviceId as username.
        var fakeGooglePlaySteamAppTicket = _steamService.ParseTicket(new[] { Byte.MaxValue, }, model.DeviceId);

        var userFromDb = await _userRepository.GetBySteamIdAsync(fakeGooglePlaySteamAppTicket.SteamID,
            PolyUsernameGenerator.GetGeneratedUsername(model.DeviceId));

        await UpdateClientVersions(model, Platform.Steam, userFromDb);

        var token = CreateToken(userFromDb);

        var json = JsonConvert.SerializeObject(new ServerResponse<PolytopiaToken>(token));

        return Content(json, "application/json");
    }

    [Route("login_nintendo_service_account")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginNintendoServiceAccount(
        LoginNintendoServiceAccountBindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is not implemented yet. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            body
        );
    }

    [Route("login_tesla")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginTesla(LoginTeslaBindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is not implemented yet. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            body
        );
    }

    [Route("login_ios")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginIos(LoginGameCenterBindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is not implemented yet. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status501NotImplemented,
            body
        );
    }

    [Route("login_ios_v2")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginIosV2(LoginGameCenterV2BindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is disabled. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status405MethodNotAllowed,
            body
        );
    }

    [Route("login_fake")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginFake(LoginFakeBindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is disabled. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status405MethodNotAllowed,
            body
        );
    }

    [Route("login_debug_legacy")]
    public async Task<ActionResult<ServerResponse<PolytopiaToken>>> LoginDebugLegacyUser(LoginFakeBindingModel model)
    {
        var body = new ServerResponse<PolytopiaToken>(
            errorCode: ErrorCode.LoginFailed,
            errorMessage: "This login method is disabled. Please use another.",
            innerMessage: null
        );

        return StatusCode(
            StatusCodes.Status405MethodNotAllowed,
            body
        );
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
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Database;
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

    public PolytopiaController(IPolydystopiaUserRepository userRepository)
    {
        _userRepository = userRepository;
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
    public ServerResponse<GameViewModel> JoinGame([FromBody] JoinGameBindingModel model)
    {
        PolytopiaDataManager.provider = new MyProvider();

        client = new HotseatClient();

        var settings = new GameSettings();
        settings.players = new Dictionary<Guid, PlayerData>();

        var playerA = new PlayerData();
        playerA.type = PlayerData.Type.Local;
        playerA.state = PlayerData.State.Accepted;
        playerA.knownTribe = true;
        playerA.tribe = TribeData.Type.Aquarion;
        playerA.tribeMix = TribeData.Type.None;
        playerA.botDifficulty = GameSettings.Difficulties.Normal;
        playerA.skinType = SkinType.Default;
        playerA.defaultName = "Paranoia";
        playerA.profile.id = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        playerA.profile.SetName("Paranoia");
        settings.players.Add(Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"), playerA);

        var playerB = new PlayerData();
        playerB.type = PlayerData.Type.Bot;
        playerB.state = PlayerData.State.Accepted;
        playerB.knownTribe = true;
        playerB.tribeMix = TribeData.Type.Aimo;
        playerB.botDifficulty = GameSettings.Difficulties.Normal;
        playerB.skinType = SkinType.Default;
        playerB.defaultName = "PlayerB";
        playerA.profile.SetName("PlayerB");
        playerB.profile.id = Guid.Parse("bbbbbbbb-281c-464c-a8e7-6a79f4496360");
        settings.players.Add(Guid.Parse("bbbbbbbb-281c-464c-a8e7-6a79f4496360"), playerB);

        var players = new List<PlayerState>();

        //var playerStateA = new PlayerState();
        //playerStateA.tribe = TribeData.Type.Aquarion;
        //playerStateA.tribeMix = TribeData.Type.None;
        //playerStateA.AccountId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        //playerStateA.UserName = "Paranoia";
        //players.Add(playerStateA);

        //var playerStateB = new PlayerState();
        //playerStateB.tribe = TribeData.Type.Aquarion;
        //playerStateB.tribeMix = TribeData.Type.None;
        //playerStateB.AccountId = Guid.Parse("bbbbbbbb-281c-464c-a8e7-6a79f4496360");
        //playerStateB.UserName = "PlayerB";
        //players.Add(playerStateB);

        var result = client.CreateSession(settings, players);


        var x = result.Result;


        var gameViewModel = new GameViewModel();
        gameViewModel.Id = model.GameId;
        gameViewModel.OwnerId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        gameViewModel.DateCreated = DateTime.Now;
        gameViewModel.DateLastCommand = DateTime.Now;
        gameViewModel.State = GameSessionState.Started;

        gameViewModel.GameSettingsJson = JsonConvert.SerializeObject(settings);

        gameViewModel.InitialGameStateData =
            SerializationHelpers.ToByteArray<GameState>(client.GameState, client.GameState.Version);
        gameViewModel.CurrentGameStateData =
            SerializationHelpers.ToByteArray<GameState>(client.GameState, client.GameState.Version);

        return new ServerResponse<GameViewModel>(gameViewModel);
    }
}
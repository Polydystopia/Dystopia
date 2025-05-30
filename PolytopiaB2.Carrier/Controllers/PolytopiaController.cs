using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polytopia.Data;
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
    public IActionResult LoginSteam([FromBody] SteamLoginBindingModel? model)
    {
        if (model?.SteamAuthTicket?.Data == null)
        {
            return BadRequest("Invalid auth ticket data");
        }
        
        var parsedSteamTicket = AppTicketParser.ParseAppTicket(model.SteamAuthTicket.Data);
        
        var token = new PolytopiaToken();
        
        //TODO
        token.JwtToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI1OTdmMzMyYi0yODFjLTQ2NGMtYThlNy02YTc5ZjQ0OTYzNjAiLCJ1bmlxdWVfbmFtZSI6IlBhcmFub2lhIHM2OSMwNDUxIiwiQXNwTmV0LklkZW50aXR5LlNlY3VyaXR5U3RhbXAiOiJQQ1NENkhRM1JUR0pESVdBVDRCQkpZM0lGVzVBUlkzSiIsInN0ZWFtIjoiNzY1NjExOTgxOTczODMyMDIiLCJuYmYiOjE3MDg4NTY3MjYsImV4cCI6MTkwODg5MjcyNiwiaWF0IjoxNzA4ODU2NzI2LCJpc3MiOiJJc3N1ZXIiLCJhdWQiOiJBdWRpZW5jZSJ9.JPq-VUNUhlxLxgAB1igjp2wmhlwxW4DF5X5Jz4M-HyA";
        token.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1908892726).DateTime;
        //TODO
        
        var user = new NewPolytopiaUserViewModel();
        user.PolytopiaId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        user.UserName = "Paranoia123";
        user.Alias = "Paranoia123";
        user.FriendCode = "ppdesmgfxi";
        user.AllowsFriendRequests = true;
        user.SteamId = "76561198197383202";
        user.NumFriends = 0;
        user.Elo = 6969;
        user.Victories = new Dictionary<string, int>();
        user.Defeats = new Dictionary<string, int>();
        user.NumGames = 0;
        user.NumMultiplayergames = 0;
        user.MultiplayerRating = 6666;
        user.AvatarStateData =
            Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA");
        user.UserMigrated = true;
        user.GameVersions = new List<ClientGameVersionViewModel>()
        {
            new ClientGameVersionViewModel()
            {
                Platform = Platform.Steam,
                DeviceId = "4c24759ff9d1d0c6e8bb28c7afc178b4752eca0d",
                GameVersion = 112
            }
        };
        user.LastLoginDate = DateTime.Parse("0001-01-01T00:00:00");
        user.UnlockedTribes = new List<int>();
        user.UnlockedSkins = new List<int>();
        user.CmUserData = null;

        token.User = user;

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
        
        gameViewModel.InitialGameStateData = SerializationHelpers.ToByteArray<GameState>(client.GameState, client.GameState.Version);
        gameViewModel.CurrentGameStateData = SerializationHelpers.ToByteArray<GameState>(client.GameState, client.GameState.Version);
        
        return new ServerResponse<GameViewModel>(gameViewModel);
    }
}

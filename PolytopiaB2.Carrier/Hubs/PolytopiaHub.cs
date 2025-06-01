using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Controllers;
using PolytopiaB2.Carrier.Database;
using PolytopiaB2.Carrier.Database.Friendship;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaB2.Carrier.Hubs;

public class PolytopiaHub : Hub
{
    private readonly IPolydystopiaUserRepository _userRepository;
    private readonly IFriendshipRepository _friendRepository;

    private string _userId => Context.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private string _username => Context.User?.FindFirst("unique_name")?.Value ?? string.Empty;
    private string _steamId => Context.User?.FindFirst("steam")?.Value ?? string.Empty;

    public PolytopiaHub(IPolydystopiaUserRepository userRepository, IFriendshipRepository friendRepository)
    {
        _userRepository = userRepository;
        _friendRepository = friendRepository;
    }

    public override async Task OnConnectedAsync()
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{_userId}");

        await base.OnConnectedAsync();
    }

    public ServerResponse<ResponseViewModel> SubscribeToParticipatingGameSummaries()
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> SubscribeToFriends()
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> UpdateAvatar(AvatarBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }
    
    public async Task<ServerResponseList<PolytopiaFriendViewModel>> SearchUsers(
        SearchUsersBindingModel model)
    {
        var response = new ServerResponseList<PolytopiaFriendViewModel>(new List<PolytopiaFriendViewModel>());

        var foundUsers = await _userRepository.GetAllByNameStartsWith(model.SearchString);

        foreach (var foundUser in foundUsers)
        {
            if (foundUser.PolytopiaId.ToString() == _userId) continue;

            var friendViewModel = new PolytopiaFriendViewModel();
            friendViewModel.User = foundUser;

            friendViewModel.FriendshipStatus = await _friendRepository
                .GetFriendshipStatusAsync(Guid.Parse(_userId), foundUser.PolytopiaId);

            response.Data.Add(friendViewModel);
        }

        return response;
    }
    
    public ServerResponse<PlayersStatusesResponse> GetFriendsStatuses()
    {
        var response = new PlayersStatusesResponse() { Statuses = new Dictionary<string, PlayerStatus>() };
        return new ServerResponse<PlayersStatusesResponse>(response);
    }

    public async Task<ServerResponseList<PolytopiaFriendViewModel>> GetFriends()
    {
        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));

        return new ServerResponseList<PolytopiaFriendViewModel>(myFriends);
    }

    public async Task<ServerResponse<ResponseViewModel>> AcceptFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus == FriendshipStatus.ReceivedRequest)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.Accepted);
            
            Clients.Group($"user-{model.FriendUserId}").SendAsync("OnFriendRequestAccepted", Guid.Parse(_userId)); //TODO: Also when user is offline
        }
        
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
    
    public async Task<ServerResponse<ResponseViewModel>> SendFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus == FriendshipStatus.None)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.SentRequest);
            
            await Clients.Group($"user-{model.FriendUserId}").SendAsync("OnFriendRequestReceived", Guid.Parse(_userId)); //TODO: Also when user is offline
        }

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<ResponseViewModel>> RemoveFriend(FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus == FriendshipStatus.Accepted)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.None);
        }
        else if(currentStatus == FriendshipStatus.ReceivedRequest)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.None); //TODO: Set rejected
        }
        
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
    
    public ServerResponse<ResponseViewModel> UploadNumSingleplayerGames(UploadNumSingleplayerGamesBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> UploadTribeRating(UploadTribeRatingBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<TribeRatingsViewModel> GetTribeRatings()
    {
        var response = new TribeRatingsViewModel();
        response.PolytopiaUserId = Guid.Parse(_userId);
        response.Ratings = new Dictionary<int, TribeRatingViewModel>();
        return new ServerResponse<TribeRatingsViewModel>(response);
    }

    public ServerResponse<PlayersStatusesResponse> LeaveAllMatchmakingGames()
    {
        var response = new PlayersStatusesResponse() { Statuses = new Dictionary<string, PlayerStatus>() };
        return new ServerResponse<PlayersStatusesResponse>(response);
    }

    public ServerResponse<GetLobbyInvitationsViewModel> GetLobbiesInvitations()
    {
        var response = new GetLobbyInvitationsViewModel() { Lobbies = new List<LobbyGameViewModel>() };
        return new ServerResponse<GetLobbyInvitationsViewModel>(response);
    }

    public ServerResponse<GameListingViewModel> GetGameListingsV3()
    {
        var response = new GameListingViewModel();
        response.gameSummaries = new List<GameSummaryViewModel>();
        response.matchmakingGameSummaries = new List<MatchmakingGameSummaryViewModel>();


        var gameSummaryViewModel = new GameSummaryViewModel();

        gameSummaryViewModel.GameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        gameSummaryViewModel.State = GameSessionState.Started;
        gameSummaryViewModel.DateCreated = DateTime.Now;
        gameSummaryViewModel.TimeLimit = 360;
        gameSummaryViewModel.DateLastCommand = DateTime.Now;
        gameSummaryViewModel.DateLastEndTurn = DateTime.Now;

        gameSummaryViewModel.Participators = new List<ParticipatorViewModel>();

        var participator = new ParticipatorViewModel()
        {
            UserId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"),
            Name = "Paranoia",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Invited
        };

        gameSummaryViewModel.Participators.Add(participator);

        response.gameSummaries.Add(gameSummaryViewModel);


        return new ServerResponse<GameListingViewModel>(response);
    }

    public ServerResponse<LobbyGameViewModel> CreateLobby(CreateLobbyBindingModel model)
    {
        var response = new LobbyGameViewModel();
        response.Id = Guid.NewGuid();
        response.UpdatedReason = LobbyUpdatedReason.Created;
        response.DateCreated = DateTime.Now;
        response.DateModified = DateTime.Now;
        response.Name = "Test Lobby";
        response.MapPreset = MapPreset.WaterWorld;
        response.MapSize = 30;
        response.OpponentCount = 1;
        response.GameMode = GameMode.Custom;
        response.OwnerId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        response.DisabledTribes = new List<int>();
        response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.IsPersistent = true;
        response.IsSharable = true;
        response.TimeLimit = 0;
        response.ScoreLimit = 0;
        response.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360";
        response.MatchmakingGameId = 421241;
        response.ChallengermodeGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.StartTime = DateTime.Now;
        response.GameContext = new GameContext()
        {
            ExternalMatchId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360"),
            ExternalTournamentId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360")
        };
        response.Participators = new List<ParticipatorViewModel>();
        response.Participators.Add(new ParticipatorViewModel()
        {
            UserId = Guid.Parse("bbbbbbbb-281c-464c-a8e7-6a79f4496360"),
            Name = "PlayerB",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Accepted
        });


        response.Bots = new List<int>();
        response.Bots.Add(3);

        return new ServerResponse<LobbyGameViewModel>(response);
    }

    public ServerResponse<BoolResponseViewModel> SubscribeToLobby(SubscribeToLobbyBindingModel model)
    {
        var response = new BoolResponseViewModel();
        response.Result = true;

        return new ServerResponse<BoolResponseViewModel>(response);
    }

    public ServerResponse<LobbyGameViewModel> RespondToLobbyInvitation(RespondToLobbyInvitation model)
    {
        var response = new LobbyGameViewModel();
        response.Id = Guid.NewGuid();
        response.UpdatedReason = LobbyUpdatedReason.Created;
        response.DateCreated = DateTime.Now;
        response.DateModified = DateTime.Now;
        response.Name = "Test Lobby";
        response.MapPreset = MapPreset.WaterWorld;
        response.MapSize = 30;
        response.OpponentCount = 1;
        response.GameMode = GameMode.Custom;
        response.OwnerId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        response.DisabledTribes = new List<int>();
        response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.IsPersistent = true;
        response.IsSharable = true;
        response.TimeLimit = 0;
        response.ScoreLimit = 0;
        response.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360";
        response.MatchmakingGameId = 421241;
        response.ChallengermodeGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.StartTime = DateTime.Now;
        response.GameContext = new GameContext()
        {
            ExternalMatchId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360"),
            ExternalTournamentId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360")
        };
        response.Participators = new List<ParticipatorViewModel>();
        response.Participators.Add(new ParticipatorViewModel()
        {
            UserId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"),
            Name = "Paranoia",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Accepted
        });


        response.Bots = new List<int>();
        response.Bots.Add(3);

        return new ServerResponse<LobbyGameViewModel>(response);
    }

    public ServerResponse<LobbyGameViewModel> StartLobbyGame(StartLobbyBindingModel model)
    {
        var response = new LobbyGameViewModel();
        response.Id = Guid.NewGuid();
        response.UpdatedReason = LobbyUpdatedReason.Created;
        response.DateCreated = DateTime.Now;
        response.DateModified = DateTime.Now;
        response.Name = "Test Lobby";
        response.MapPreset = MapPreset.WaterWorld;
        response.MapSize = 30;
        response.OpponentCount = 1;
        response.GameMode = GameMode.Custom;
        response.OwnerId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c");
        response.DisabledTribes = new List<int>();
        response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.IsPersistent = true;
        response.IsSharable = true;
        response.TimeLimit = 0;
        response.ScoreLimit = 0;
        response.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360";
        response.MatchmakingGameId = 421241;
        response.ChallengermodeGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.StartTime = DateTime.Now;
        response.GameContext = new GameContext()
        {
            ExternalMatchId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360"),
            ExternalTournamentId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360")
        };
        response.Participators = new List<ParticipatorViewModel>();
        response.Participators.Add(new ParticipatorViewModel()
        {
            UserId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"),
            Name = "Paranoia",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Accepted
        });


        response.Bots = new List<int>();
        response.Bots.Add(3);

        return new ServerResponse<LobbyGameViewModel>(response);
    }


    public ServerResponse<ResponseViewModel> SubscribeToGame(SubscribeToGameBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<GameSummaryViewModel> GetGameSummaryViewModelByIdAsync(Guid gameId)
    {
        var gameSummaryViewModel = new GameSummaryViewModel();

        gameSummaryViewModel.GameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        gameSummaryViewModel.State = GameSessionState.Started;

        gameSummaryViewModel.Participators = new List<ParticipatorViewModel>();

        var participator = new ParticipatorViewModel()
        {
            UserId = Guid.Parse("bbbbbbbb-281c-464c-a8e7-6a79f4496360"),
            Name = "PlayerB",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Accepted
        };

        gameSummaryViewModel.Participators.Add(participator);
        gameSummaryViewModel.Participators.Add(participator);

        return new ServerResponse<GameSummaryViewModel>(gameSummaryViewModel);
    }

    public ServerResponse<MatchmakingSubmissionViewModel> SubmitMatchmakingRequest(
        SubmitMatchmakingBindingModel model)
    {
        var participator = new ParticipatorViewModel()
        {
            UserId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"),
            Name = "Paranoia",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Accepted
        };


        var matchmakingSubmissionViewModel = new MatchmakingSubmissionViewModel();
        matchmakingSubmissionViewModel.GameName = "lol";
        matchmakingSubmissionViewModel.IsWaitingForOpponents = true;

        var matchmakingGameSummaryViewModel = new MatchmakingGameSummaryViewModel();
        matchmakingGameSummaryViewModel.Id = 124152;
        matchmakingGameSummaryViewModel.GameMode = GameMode.Custom;
        matchmakingGameSummaryViewModel.MapPreset = MapPreset.WaterWorld;
        matchmakingGameSummaryViewModel.MapSize = 30;
        matchmakingGameSummaryViewModel.OpponentCount = 1;
        matchmakingGameSummaryViewModel.Name = "loltroll";
        matchmakingGameSummaryViewModel.WithPickedTribe = true;

        matchmakingGameSummaryViewModel.Participators = new List<ParticipatorViewModel>();
        matchmakingGameSummaryViewModel.Participators.Add(participator);

        matchmakingSubmissionViewModel.MatchmakingGameSummaryViewModel = matchmakingGameSummaryViewModel;

        return new ServerResponse<MatchmakingSubmissionViewModel>(matchmakingSubmissionViewModel);
    }

    public ServerResponse<ResponseViewModel> SendCommand(SendCommandBindingModel model)
    {
        var client = PolytopiaController.client;

        var succ1 = CommandBase.FromByteArray(model.Command.SerializedData, out var cmd, out var version);

        client.SendCommand(cmd).Wait();

        var arr = new CommandArrayViewModel();
        arr.GameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        arr.Commands = new List<PolytopiaCommandViewModel>()
        {
            model.Command
        };

        SendCommandToOthers(Guid.Empty, arr);

        if (cmd is EndTurnCommand)
        {
            client.GameState.EndPlayerTurn();

            CommandBase command = new CommandBase();

            while (command is not EndTurnCommand)
            {
                if (!CommandTriggerUtils.TryGetTriggerCommand(client.GameState, out command))
                {
                    command = AI.GetMove(client.GameState, client.GameState.PlayerStates[1]);

                    if (command is StayCommand)
                    {
                        command = AI.EndCommand(client.GameState, client.GameState.PlayerStates[1]);
                    }
                }

                if (command.IsValid(client.GameState))
                {
                    var arr2 = new CommandArrayViewModel();
                    arr2.GameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
                    arr2.Commands = new List<PolytopiaCommandViewModel>()
                    {
                        new(CommandBase.ToByteArray(command, 104))
                    };
                    SendCommandToAll(Guid.Empty, arr2);
                }
            }
        }

        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public async Task SendCommandToOthers(Guid userId, CommandArrayViewModel commandArray)
    {
        await Clients.Others.SendAsync("OnCommand", commandArray);

        var gameSummaryViewModel = new GameSummaryViewModel();

        gameSummaryViewModel.GameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        gameSummaryViewModel.State = GameSessionState.Started;
        gameSummaryViewModel.DateCreated = DateTime.Now;
        gameSummaryViewModel.TimeLimit = 360;
        gameSummaryViewModel.DateLastCommand = DateTime.Now;
        gameSummaryViewModel.DateLastEndTurn = DateTime.Now;

        gameSummaryViewModel.Participators = new List<ParticipatorViewModel>();

        var participator = new ParticipatorViewModel()
        {
            UserId = Guid.Parse("d078d324-62f1-4d86-b603-5449986ace5c"),
            Name = "Paranoia",
            NumberOfFriends = 0,
            NumberOfMultiplayerGames = 0,
            GameVersion = new List<ClientGameVersionViewModel>(),
            MultiplayerRating = 0,
            SelectedTribe = 1,
            SelectedTribeSkin = 1,
            AvatarStateData =
                Convert.FromBase64String("YgAAACgAAAAMAAAAAAAAABEAAAAAAAAAHgAAAAAAAAAfAAAAAAAAADIAAAC4SusA"),
            InvitationState = PlayerInvitationState.Invited
        };

        gameSummaryViewModel.Participators.Add(participator);

        //gameSummaryViewModel.GameSummaryData
        //gameSummaryViewModel.GameSummaryData
        //gameSummaryViewModel.GameSummaryData

        await SendGameSummaryUpdatedToAll(gameSummaryViewModel, StateUpdateReason.ValidCommand);
    }

    public async Task SendCommandToAll(Guid userId, CommandArrayViewModel commandArray)
    {
        await Clients.All.SendAsync("OnCommand", commandArray);
    }

    public async Task SendGameSummaryUpdatedToAll(
        GameSummaryViewModel model,
        StateUpdateReason pushReason)
    {
        await Clients.All.SendAsync("OnGameSummaryUpdated", model, pushReason);
    }
}
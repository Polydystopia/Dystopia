using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Controllers;
using PolytopiaB2.Carrier.Database;
using PolytopiaB2.Carrier.Database.Friendship;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Database.Lobby;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaB2.Carrier.Game;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub : Hub
{
    private readonly IPolydystopiaUserRepository _userRepository;
    private readonly IFriendshipRepository _friendRepository;
    private readonly IPolydystopiaLobbyRepository _lobbyRepository;
    private readonly IPolydystopiaGameRepository _gameRepository;

    private string _userId => Context.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private string _username => Context.User?.FindFirst("unique_name")?.Value ?? string.Empty;
    private string _steamId => Context.User?.FindFirst("steam")?.Value ?? string.Empty;

    private Guid _userGuid => Guid.Parse(_userId);

    public PolytopiaHub(IPolydystopiaUserRepository userRepository, IFriendshipRepository friendRepository,
        IPolydystopiaLobbyRepository lobbyRepository, IPolydystopiaGameRepository gameRepository)
    {
        _userRepository = userRepository;
        _friendRepository = friendRepository;
        _lobbyRepository = lobbyRepository;
        _gameRepository = gameRepository;
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

    public ServerResponse<ResponseViewModel> UpdateAvatar(AvatarBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
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


    }
}
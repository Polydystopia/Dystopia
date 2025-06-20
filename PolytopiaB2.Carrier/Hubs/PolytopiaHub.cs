using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaB2.Carrier.Controllers;
using PolytopiaB2.Carrier.Database;
using PolytopiaB2.Carrier.Database.Friendship;
using PolytopiaB2.Carrier.Database.Game;
using PolytopiaB2.Carrier.Database.Lobby;
using PolytopiaB2.Carrier.Database.Matchmaking;
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
    private readonly IPolydystopiaMatchmakingRepository _matchmakingRepository;

    private string _userId => Context.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private string _username => Context.User?.FindFirst("unique_name")?.Value ?? string.Empty;
    private string _steamId => Context.User?.FindFirst("steam")?.Value ?? string.Empty;

    private Guid _userGuid => Guid.Parse(_userId);

    private readonly ILogger<PolytopiaHub> _logger;

    public PolytopiaHub(IPolydystopiaUserRepository userRepository, IFriendshipRepository friendRepository,
        IPolydystopiaLobbyRepository lobbyRepository, IPolydystopiaGameRepository gameRepository,
        IPolydystopiaMatchmakingRepository matchmakingRepository, ILogger<PolytopiaHub> logger)
    {
        _userRepository = userRepository;
        _friendRepository = friendRepository;
        _lobbyRepository = lobbyRepository;
        _gameRepository = gameRepository;
        _matchmakingRepository = matchmakingRepository;

        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (string.IsNullOrEmpty(_userId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{_userId}");

        SubscribeToFriends();
        SubscribeToParticipatingGameSummaries();

        OnlinePlayers[_userGuid] = Clients.Caller;

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        OnlinePlayers.TryRemove(_userGuid, out _);
        FriendSubscribers.TryRemove(_userGuid, out _);

        foreach (var gameSubscription in GameSubscribers.Values)
        {
            lock (gameSubscription)
            {
                gameSubscription.RemoveAll(x => x.id == _userGuid);
            }
        }

        foreach (var lobbySubscription in LobbySubscribers.Values)
        {
            lock (lobbySubscription)
            {
                lobbySubscription.RemoveAll(x => x.id == _userGuid);
            }
        }

        var emptyGameKeys = GameSubscribers.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
        foreach (var key in emptyGameKeys)
        {
            GameSubscribers.TryRemove(key, out _);
        }

        var emptyLobbyKeys = LobbySubscribers.Where(kvp => kvp.Value.Count == 0).Select(kvp => kvp.Key).ToList();
        foreach (var key in emptyLobbyKeys)
        {
            LobbySubscribers.TryRemove(key, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public ServerResponse<TribeRatingsViewModel> GetTribeRatings() //TODO
    {
        var response = new TribeRatingsViewModel();
        response.PolytopiaUserId = Guid.Parse(_userId);
        response.Ratings = new Dictionary<int, TribeRatingViewModel>();
        return new ServerResponse<TribeRatingsViewModel>(response);
    }

    public ServerResponse<GameSummaryViewModel> GetGameSummaryViewModelByIdAsync(Guid gameId) //TODO
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
}
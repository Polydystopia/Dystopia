using Dystopia.Database.Friendship;
using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using Microsoft.AspNetCore.SignalR;
using Dystopia.Managers.Game;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Hubs;

public partial class PolytopiaHub : Hub
{
    private readonly IPolydystopiaUserRepository _userRepository;
    private readonly IFriendshipRepository _friendRepository;
    private readonly IPolydystopiaLobbyRepository _lobbyRepository;
    private readonly IPolydystopiaGameRepository _gameRepository;
    private readonly IPolydystopiaMatchmakingRepository _matchmakingRepository;

    private readonly IPolydystopiaGameManager _gameManager;

    private string _userId => Context.User?.FindFirst("nameid")?.Value ?? string.Empty;
    private string _username => Context.User?.FindFirst("unique_name")?.Value ?? string.Empty;
    private string _steamId => Context.User?.FindFirst("steam")?.Value ?? string.Empty;

    private Guid _userGuid => Guid.Parse(_userId);

    private readonly ILogger<PolytopiaHub> _logger;

    public PolytopiaHub(IPolydystopiaUserRepository userRepository, IFriendshipRepository friendRepository,
        IPolydystopiaLobbyRepository lobbyRepository, IPolydystopiaGameRepository gameRepository,
        IPolydystopiaMatchmakingRepository matchmakingRepository, IPolydystopiaGameManager gameManager, ILogger<PolytopiaHub> logger)
    {
        _userRepository = userRepository;
        _friendRepository = friendRepository;
        _lobbyRepository = lobbyRepository;
        _gameRepository = gameRepository;
        _matchmakingRepository = matchmakingRepository;

        _gameManager = gameManager;

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

    public async Task<ServerResponse<GameSummaryViewModel>> GetGameSummaryViewModelByIdAsync(Guid gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);

        if(game == null) return new ServerResponse<GameSummaryViewModel>(ErrorCode.GameNotFound, "Game not found");

        var summary = _gameManager.GetGameSummaryViewModelByGameViewModel(game.ToViewModel());

        return new ServerResponse<GameSummaryViewModel>(summary);
    }
}
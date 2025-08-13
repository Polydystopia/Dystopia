using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Challengermode.Matchmaking;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.Enums;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Hubs;

public interface IDystopiaHubClient
{
    public Task OnNotify(string message);

    public Task OnCommand(CommandArrayViewModel model);
    public Task OnPlayerResigned(PlayerResignedViewModel playerResignedViewModel);
    public Task OnPlayerSkipped(PlayerSkippedViewModel playerSkippedViewModel);

    public Task OnGameStateUpdated(GameStateViewModel model, StateUpdateReason pushReason);
    public Task OnGameSummaryUpdated(GameSummaryViewModel model, StateUpdateReason pushReason);
    public Task OnMatchmakingGameUpdated(long gameId, MatchmakingUpdateReason pushReason);

    public Task OnInvitation(Guid gameId);
    public Task OnLobbyInvitation(LobbyGameViewModel lobby);
    public Task OnLobbyUpdated(LobbyGameViewModel lobby);
    public Task OnGameReadyToStart(Guid gameId);
    public Task OnGameDeleted(Guid gameId, GameDeleteReason deleteReason);

    public Task OnActionableGamesUpdated(int count);
    public Task OnPlayerStatusUpdated(Guid playerId, PlayerStatus status);
    public Task OnUserUpdated(PolytopiaUserViewModel user);

    public Task OnFriendsUpdated(List<PolytopiaFriendViewModel> friends);
    public Task OnFriendRequestReceived(Guid friendId);
    public Task OnFriendRequestAccepted(Guid friendId);

    public Task OnTournamentUpdated(TournamentViewModel tournament, TournamentUpdateReason reason);
    public Task OnTournamentPersonalUpdated(TournamentPersonalUpdatedViewModel viewModel);

    public Task OnTournamentMatchmakingUpdated(TournamentMatchmakingQueueViewModel queue,
        TournamentMatchmakingUpdateReason reason);
}
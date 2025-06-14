using PolytopiaB2.Carrier.Database.Matchmaking;
using PolytopiaB2.Carrier.Game.Matchmaking;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<MatchmakingSubmissionViewModel>> SubmitMatchmakingRequest(
        SubmitMatchmakingBindingModel model)
    {
        var matchmakingSubmissionViewModel = await PolydystopiaMatchmakingManager.QueuePlayer(_userGuid, model, _matchmakingRepository, _userRepository);

        return new ServerResponse<MatchmakingSubmissionViewModel>(matchmakingSubmissionViewModel);
    }

    public ServerResponse<PlayersStatusesResponse> LeaveAllMatchmakingGames() //TODO
    {
        var response = new PlayersStatusesResponse() { Statuses = new Dictionary<string, PlayerStatus>() };
        return new ServerResponse<PlayersStatusesResponse>(response);
    }
}
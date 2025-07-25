﻿using Dystopia.Database.Matchmaking;
using Dystopia.Managers.Matchmaking;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<MatchmakingSubmissionViewModel>> SubmitMatchmakingRequest(
        SubmitMatchmakingBindingModel model)
    {
        var matchmakingSubmissionViewModel = await PolydystopiaMatchmakingManager.QueuePlayer(_userGuid, model, Clients.Caller, _matchmakingRepository, _userRepository, _lobbyRepository);

        return new ServerResponse<MatchmakingSubmissionViewModel>(matchmakingSubmissionViewModel);
    }

    public ServerResponse<PlayersStatusesResponse> LeaveAllMatchmakingGames() //TODO
    {
        var response = new PlayersStatusesResponse() { Statuses = new Dictionary<string, PlayerStatus>() };
        return new ServerResponse<PlayersStatusesResponse>(response);
    }
}
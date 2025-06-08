using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Game;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game.BindingModels;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public ServerResponse<ResponseViewModel> SubscribeToParticipatingGameSummaries() //TODO
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> SubscribeToFriends() //TODO
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<BoolResponseViewModel> SubscribeToLobby(SubscribeToLobbyBindingModel model) //TODO
    {
        var response = new BoolResponseViewModel();
        response.Result = true;

        return new ServerResponse<BoolResponseViewModel>(response);
    }

    public ServerResponse<ResponseViewModel> SubscribeToGame(SubscribeToGameBindingModel model)
    {
        var subList = PolydystopiaGameManager.GameSubscribers;
        if (!subList.ContainsKey(model.GameId))
        {
            subList.Add(model.GameId, new List<(Guid id, IClientProxy proxy)>());
        }

        var myId = _userGuid;
        var myProxy = Clients.Caller;
        var el = subList[model.GameId];

        var existingIndex = el.FindIndex(x => x.id == myId);
        if (existingIndex >= 0)
        {
            el.RemoveAt(existingIndex);
        }

        el.Add((myId, myProxy));

        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public async Task<ServerResponse<PlayersStatusesResponse>> SubscribeToGameParticipantsStatuses(
        SubscribeToGameParticipantsStatusesBindingModel model) //TODO
    {
        var response = new PlayersStatusesResponse();
        response.Statuses = new Dictionary<string, PlayerStatus>();

        return new ServerResponse<PlayersStatusesResponse>(response);
    }
}
using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Game;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game.BindingModels;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public static readonly Dictionary<Guid, IClientProxy> OnlinePlayers = new();

    public static readonly Dictionary<Guid, List<(Guid id, IClientProxy proxy)>> GameSubscribers = new();
    public static readonly Dictionary<Guid, List<(Guid id, IClientProxy proxy)>> LobbySubscribers = new();

    public static readonly Dictionary<Guid, IClientProxy> FriendSubscribers = new();

    private void Subscribe(Dictionary<Guid, List<(Guid id, IClientProxy proxy)>> subList, Guid gameId)
    {
        if (!subList.ContainsKey(gameId))
        {
            subList.Add(gameId, new List<(Guid id, IClientProxy proxy)>());
        }

        var myId = _userGuid;
        var myProxy = Clients.Caller;
        var el = subList[gameId];

        var existingIndex = el.FindIndex(x => x.id == myId);
        if (existingIndex >= 0)
        {
            el.RemoveAt(existingIndex);
        }

        el.Add((myId, myProxy));
    }

    public ServerResponse<ResponseViewModel> SubscribeToParticipatingGameSummaries() //TODO
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }


    public ServerResponse<ResponseViewModel> SubscribeToFriends()
    {
        var myId = _userGuid;
        var myProxy = Clients.Caller;

        FriendSubscribers[myId] = myProxy;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public ServerResponse<BoolResponseViewModel> SubscribeToLobby(SubscribeToLobbyBindingModel model) //TODO
    {
        var response = new BoolResponseViewModel();
        response.Result = true;

        return new ServerResponse<BoolResponseViewModel>(response);
    }

    public ServerResponse<ResponseViewModel> SubscribeToGame(SubscribeToGameBindingModel model)
    {
        var subList = GameSubscribers;

        Subscribe(subList, model.GameId);;

        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public async Task<ServerResponse<ResponseViewModel>> UnsubscribeToGame(
        UnsubscribeToGameBindingModel model)
    {
        if (GameSubscribers.TryGetValue(model.GameId, out var subscribers))
        {
            subscribers.RemoveAll(x => x.id == _userGuid);
        }

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<PlayersStatusesResponse>> SubscribeToGameParticipantsStatuses(
        SubscribeToGameParticipantsStatusesBindingModel model) //TODO
    {
        var response = new PlayersStatusesResponse();
        response.Statuses = new Dictionary<string, PlayerStatus>();

        return new ServerResponse<PlayersStatusesResponse>(response);
    }
}
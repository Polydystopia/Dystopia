using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public ServerResponse<PlayersStatusesResponse> GetFriendsStatuses() //TODO
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

            Clients.Group($"user-{model.FriendUserId}")
                .SendAsync("OnFriendRequestAccepted", Guid.Parse(_userId)); //TODO: Also when user is offline
        }

        var isRequestSenderOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var proxy);
        if (isRequestSenderOnline) await proxy?.SendAsync("OnFriendRequestAccepted", model.FriendUserId)!;

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

            await Clients.Group($"user-{model.FriendUserId}")
                .SendAsync("OnFriendRequestReceived", Guid.Parse(_userId)); //TODO: Also when user is offline
        }

        var isRequestReceiverOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var proxy);
        if (isRequestReceiverOnline) await proxy?.SendAsync("OnFriendRequestReceived", model.FriendUserId)!;

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
        else if (currentStatus == FriendshipStatus.ReceivedRequest)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.None); //TODO: Set rejected
        }

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
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
}
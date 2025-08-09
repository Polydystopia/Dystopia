using Dystopia.Database.Friendship;
using Dystopia.Database.User;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<PlayersStatusesResponse>> GetFriendsStatuses()
    {
        var statuses = new Dictionary<string, PlayerStatus>();

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));

        foreach (var friend in myFriends)
        {
            bool foundInGame = false;

            foreach (var game in GameSubscribers)
            {
                foreach (var playerInGame in game.Value)
                {
                    if (playerInGame.id == friend.User.Id)
                    {
                        statuses[friend.User.Id.ToString()] = new PlayerStatus()
                            { GameId = game.Key, PlayerOnlineStatus = PlayerOnlineStatus.PlayingGame };
                        foundInGame = true;
                        break;
                    }
                }

                if (foundInGame) break;
            }

            if (OnlinePlayers.ContainsKey(friend.User.Id))
            {
                statuses[friend.User.Id.ToString()] = new PlayerStatus()
                    { PlayerOnlineStatus = PlayerOnlineStatus.Online };
            }
            else
            {
                statuses[friend.User.Id.ToString()] = new PlayerStatus()
                    { PlayerOnlineStatus = PlayerOnlineStatus.Offline };
            }
        }

        return new ServerResponse<PlayersStatusesResponse>(new PlayersStatusesResponse() { Statuses = statuses });
    }

    public async Task<ServerResponseList<PolytopiaFriendViewModel>> GetFriends()
    {
        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));

        return new ServerResponseList<PolytopiaFriendViewModel>(myFriends.ToFriendViewModels());
    }

    public async Task<ServerResponse<ResponseViewModel>> AcceptFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus != FriendshipStatus.ReceivedRequest)
            return new ServerResponse<ResponseViewModel>(new ResponseViewModel());

        await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
            FriendshipStatus.Accepted);

        var isRequestSenderOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestSenderOnline)
        {
            await friendProxy?.SendAsync("OnFriendRequestAccepted", Guid.Parse(_userId))!;

            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends.ToFriendViewModels())!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends.ToFriendViewModels())!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<ResponseViewModel>> SendFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus != FriendshipStatus.None)
            return new ServerResponse<ResponseViewModel>(new ResponseViewModel());

        await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
            FriendshipStatus.SentRequest);

        var isRequestReceiverOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestReceiverOnline)
        {
            await friendProxy?.SendAsync("OnFriendRequestReceived", Guid.Parse(_userId))!;

            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends.ToFriendViewModels())!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends.ToFriendViewModels())!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<ResponseViewModel>> RemoveFriend(FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId);

        if (currentStatus
            is FriendshipStatus.Accepted
            or FriendshipStatus.SentRequest
            or FriendshipStatus.ReceivedRequest)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(_userId), model.FriendUserId,
                FriendshipStatus.None);
        }

        var isRequestReceiverOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestReceiverOnline)
        {
            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends.ToFriendViewModels())!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(_userId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends.ToFriendViewModels())!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<PolytopiaFriendViewModel>> SearchUsers(
        SearchUsersBindingModel model)
    {
        var response = new ServerResponseList<PolytopiaFriendViewModel>(new List<PolytopiaFriendViewModel>());

        var foundUsers = await _userRepository.GetAllByNameStartsWith(model.SearchString);

        foreach (var foundUser in foundUsers)
        {
            if (foundUser.Id.ToString() == _userId) continue;

            var friendViewModel = new PolytopiaFriendViewModel();
            friendViewModel.User = foundUser.ToViewModel();

            friendViewModel.FriendshipStatus = await _friendRepository
                .GetFriendshipStatusAsync(Guid.Parse(_userId), foundUser.Id);

            response.Data.Add(friendViewModel);
        }

        return response;
    }
}
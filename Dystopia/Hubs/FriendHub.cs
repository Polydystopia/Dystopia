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
        
        foreach (var friend in await _friendRepository.GetFriendsForUserAsync(UserGuid))
        {
            bool foundInGame = false;

            foreach (var game in GameSubscribers)
            {
                foreach (var playerInGame in game.Value)
                {
                    if (playerInGame.id != friend.UserId1) continue;
                    statuses[friend.UserId1.ToString()] = new PlayerStatus()
                        { GameId = game.Key, PlayerOnlineStatus = PlayerOnlineStatus.PlayingGame };
                    foundInGame = true;
                    break;
                }
                if (foundInGame) break;
            }

            if (OnlinePlayers.ContainsKey(friend.UserId1))
            {
                statuses[friend.UserId1.ToString()] = new PlayerStatus()
                    { PlayerOnlineStatus = PlayerOnlineStatus.Online };
            }
            else
            {
                statuses[friend.UserId1.ToString()] = new PlayerStatus()
                    { PlayerOnlineStatus = PlayerOnlineStatus.Offline };
            }
        }

        return new ServerResponse<PlayersStatusesResponse>(new PlayersStatusesResponse() { Statuses = statuses });
    }

    public async Task<ServerResponseList<PolytopiaFriendViewModel>> GetFriends()
    {
        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(UserId));

        return new ServerResponseList<PolytopiaFriendViewModel>(myFriends.Select(f => (PolytopiaFriendViewModel)f).ToList());
    }

    public async Task<ServerResponse<ResponseViewModel>> AcceptFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId);

        if (currentStatus != FriendshipStatus.ReceivedRequest)
            return new ServerResponse<ResponseViewModel>(new ResponseViewModel());

        await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId,
            FriendshipStatus.Accepted);

        var isRequestSenderOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestSenderOnline)
        {
            await friendProxy?.SendAsync("OnFriendRequestAccepted", Guid.Parse(UserId))!;

            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends)!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(UserId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends)!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<ResponseViewModel>> SendFriendRequest(
        FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId);

        if (currentStatus != FriendshipStatus.None)
            return new ServerResponse<ResponseViewModel>(new ResponseViewModel());

        await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId,
            FriendshipStatus.SentRequest);

        var isRequestReceiverOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestReceiverOnline)
        {
            await friendProxy?.SendAsync("OnFriendRequestReceived", Guid.Parse(UserId))!;

            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends)!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(UserId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends)!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponse<ResponseViewModel>> RemoveFriend(FriendRequestBindingModel model)
    {
        var currentStatus = await _friendRepository.GetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId);

        if (currentStatus
            is FriendshipStatus.Accepted
            or FriendshipStatus.SentRequest
            or FriendshipStatus.ReceivedRequest)
        {
            await _friendRepository.SetFriendshipStatusAsync(Guid.Parse(UserId), model.FriendUserId,
                FriendshipStatus.None);
        }

        var isRequestReceiverOnline = FriendSubscribers.TryGetValue(model.FriendUserId, out var friendProxy);
        if (isRequestReceiverOnline)
        {
            var friendFriends = await _friendRepository.GetFriendsForUserAsync(model.FriendUserId);
            await friendProxy?.SendAsync("OnFriendsUpdated", friendFriends)!;
        }

        var myFriends = await _friendRepository.GetFriendsForUserAsync(Guid.Parse(UserId));
        await Clients.Caller.SendAsync("OnFriendsUpdated", myFriends)!;

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }

    public async Task<ServerResponseList<PolytopiaFriendViewModel>> SearchUsers(
        SearchUsersBindingModel model)
    {
        var response = new ServerResponseList<PolytopiaFriendViewModel>(new List<PolytopiaFriendViewModel>());

        var foundUsers = await _userRepository.GetAllByNameStartsWith(model.SearchString);

        foreach (var foundUser in foundUsers)
        {
            if (foundUser.PolytopiaId.ToString() == UserId) continue;

            var friendViewModel = new PolytopiaFriendViewModel();
            friendViewModel.User = (PolytopiaUserViewModel)foundUser;

            friendViewModel.FriendshipStatus = await _friendRepository
                .GetFriendshipStatusAsync(Guid.Parse(UserId), foundUser.PolytopiaId);

            response.Data.Add(friendViewModel);
        }

        return response;
    }
}
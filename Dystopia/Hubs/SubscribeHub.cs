﻿using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game.BindingModels;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public static readonly ConcurrentDictionary<Guid, IClientProxy> OnlinePlayers = new();

    public static readonly ConcurrentDictionary<Guid, List<(Guid id, IClientProxy proxy)>> GameSubscribers = new();

    public static readonly ConcurrentDictionary<Guid, List<(Guid id, IClientProxy proxy)>> GameSummariesSubscribers =
        new();

    public static readonly ConcurrentDictionary<Guid, List<(Guid id, IClientProxy proxy)>> LobbySubscribers = new();

    public static readonly ConcurrentDictionary<Guid, IClientProxy> FriendSubscribers = new();

    private void Subscribe(ConcurrentDictionary<Guid, List<(Guid id, IClientProxy proxy)>> subList, Guid gameId)
    {
        var myId = _userGuid;
        var myProxy = Clients.Caller;

        Subscribe(subList, gameId, myId, myProxy);
    }

    private void Subscribe(ConcurrentDictionary<Guid, List<(Guid id, IClientProxy proxy)>> subList, Guid gameId, Guid userId, IClientProxy userProxy)
    {
        var el = subList.GetOrAdd(gameId, _ => new List<(Guid id, IClientProxy proxy)>());

        lock (el)
        {
            var existingIndex = el.FindIndex(x => x.id == userId);
            if (existingIndex >= 0)
            {
                el.RemoveAt(existingIndex);
            }

            el.Add((userId, userProxy));
        }
    }

    public async Task<ServerResponse<ResponseViewModel>> SubscribeToParticipatingGameSummaries()
    {
        var gameSummariesSubList = GameSummariesSubscribers;

        var allPlayerGames = await _gameRepository.GetAllGamesByPlayer(_userGuid);

        foreach (var playerGame in allPlayerGames)
        {
            Subscribe(gameSummariesSubList, playerGame.Id);
        }

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
        var subList = LobbySubscribers;

        foreach (var lobbyId in model.LobbyIds)
        {
            Subscribe(subList, lobbyId);
        }

        var responseViewModel = new BoolResponseViewModel() { Result = true };
        return new ServerResponse<BoolResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> SubscribeToGame(SubscribeToGameBindingModel model)
    {
        var subList = GameSubscribers;

        Subscribe(subList, model.GameId);

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

    public async Task<ServerResponse<ResponseViewModel>> SubscribeToTournaments(
        SubscribeToTournamentsBindingModel model) //TODO
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
}
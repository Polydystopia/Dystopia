using Dystopia.Database.Game;
using Dystopia.Database.Lobby;
using Dystopia.Managers.Lobby;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<GetLobbyInvitationsViewModel>> GetLobbiesInvitations()
    {
        var myLobbies = await _lobbyRepository.GetAllLobbiesByPlayer(_userGuid);

        //TODO: HACK!! Since I do not want to use two devices all the time. Change later.
        //foreach (var lobbyGameViewModel in myLobbies)
        //{
        //    foreach (var participatorViewModel in lobbyGameViewModel.Participators)
        //    {
        //        participatorViewModel.InvitationState = PlayerInvitationState.Accepted;
        //    }
        //}

        var response = new GetLobbyInvitationsViewModel() { Lobbies = myLobbies.ToViewModels() };
        return new ServerResponse<GetLobbyInvitationsViewModel>(response);
    }

    public async Task<ServerResponse<LobbyGameViewModel>> CreateLobby(CreateLobbyBindingModel model)
    {
        var ownUser = await _userRepository.GetByIdAsync(_userGuid);
        if (ownUser == null)
            return new ServerResponse<LobbyGameViewModel>()
                { Success = false, ErrorCode = ErrorCode.UserNotFound, ErrorMessage = "Own user not found." };

        var createdLobby = PolydystopiaLobbyManager.CreateLobby(model, ownUser);

        await _lobbyRepository.CreateAsync(createdLobby.ToEntity());

        return new ServerResponse<LobbyGameViewModel>(createdLobby);
    }

    public async Task<ServerResponse<BoolResponseViewModel>> ModifyPlayersInLobby(
        ModifyPlayersInLobbyBindingModel model)
    {
        var lobby = await _lobbyRepository.GetByIdAsync(model.LobbyId);

        if (lobby == null)
        {
            return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = false });
        }

        if (model.Bots != null)
        {
            lobby.Bots = model.Bots;
        }

        if (model.RemovePlayers != null)
        {
            lobby.Participators.RemoveAll(p => model.RemovePlayers.Contains(p.UserId));
        }

        if (model.InvitePlayers != null)
        {
            foreach (var invitedPlayerGuid in model.InvitePlayers)
            {
                if (lobby.Participators.Any(p => p.UserId == invitedPlayerGuid)) continue;

                var invitePlayer =
                    await _userRepository.GetByIdAsync(invitedPlayerGuid); //TODO: Use normal id. Not steam

                if (invitePlayer == null) continue;

                var participator = new ParticipatorViewModel()
                {
                    UserId = invitedPlayerGuid,
                    Name = invitePlayer.GetUniqueNameInternal(),
                    NumberOfFriends = invitePlayer.NumFriends ?? 0,
                    NumberOfMultiplayerGames = invitePlayer.NumMultiplayergames ?? 0,
                    GameVersion = invitePlayer.GameVersions,
                    MultiplayerRating = invitePlayer.MultiplayerRating ?? 0,
                    SelectedTribe = 0, //?
                    SelectedTribeSkin = 0, //?
                    AvatarStateData = invitePlayer.AvatarStateData,
                    InvitationState = PlayerInvitationState.Invited
                };

                lobby.Participators.Add(participator);

                if (OnlinePlayers.TryGetValue(invitedPlayerGuid, out var onlineFriendProxy))
                {
                    await onlineFriendProxy.SendAsync("OnLobbyInvitation", lobby.ToViewModel());
                }
            }
        }

        await _lobbyRepository.UpdateAsync(lobby);

        foreach (var lobbySubscribers in LobbySubscribers[lobby.Id])
        {
            await lobbySubscribers.proxy.SendAsync("OnLobbyUpdated", lobby.ToViewModel());
        }

        return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
    }

    public async Task<ServerResponse<BoolResponseViewModel>> LeaveLobby(Guid lobbyId)
    {
        var lobby = await _lobbyRepository.GetByIdAsync(lobbyId);

        if (lobby != null && lobby.OwnerId == _userGuid)
        {
            if (lobby.MatchmakingGameId != null && lobby.Participators.Count > 1)
            {
                lobby.OwnerId = lobby.Participators.FirstOrDefault(p => p.UserId != _userGuid)!.UserId;
            }
            else
            {
                _logger.LogInformation("User {ownUserId} left lobby {lobbyId}", _userGuid, lobbyId);
                _logger.LogInformation("Deleting lobby {lobbyId} since no remaining players", lobbyId);

                await _lobbyRepository.DeleteAsync(lobbyId);

                lobby.Participators.Clear();
                foreach (var lobbySubscribers in LobbySubscribers[lobby.Id])
                {
                    await lobbySubscribers.proxy.SendAsync("OnLobbyUpdated", lobby.ToViewModel());
                }

                return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
            }
        }

        if (lobby != null && lobby.Participators.Any(p => p.UserId == _userGuid))
        {
            _logger.LogInformation("User {ownUserId} left lobby {lobbyId}", _userGuid, lobbyId);

            lobby.Participators.RemoveAll(p => p.UserId == _userGuid);

            await _lobbyRepository.UpdateAsync(lobby);

            foreach (var lobbySubscribers in LobbySubscribers[lobby.Id])
            {
                await lobbySubscribers.proxy.SendAsync("OnLobbyUpdated", lobby.ToViewModel());
            }

            return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
        }

        _logger.LogWarning("User {ownUserId} wanted to leave lobby {lobbyId} where he is no member", _userGuid,
            lobbyId);

        return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = false })
            { Success = false, ErrorCode = ErrorCode.UserNotFound, ErrorMessage = "User is not in lobby." };
    }

    public async Task<ServerResponse<LobbyGameViewModel>> RespondToLobbyInvitation(RespondToLobbyInvitation model)
    {
        var lobby = await _lobbyRepository.GetByIdAsync(model.LobbyId);

        if (lobby == null)
            return new ServerResponse<LobbyGameViewModel>()
                { Success = false, ErrorCode = ErrorCode.UserNotFound, ErrorMessage = "User is not invited to game." };

        var status = lobby.GetInvitationStateForPlayer(_userGuid);

        if (status != PlayerInvitationState.Invited)
            return new ServerResponse<LobbyGameViewModel>()
                { Success = false, ErrorCode = ErrorCode.PlayerNotFound, ErrorMessage = "Player is not invited" };

        foreach (var participatorViewModel in lobby.Participators)
        {
            if (participatorViewModel.UserId == _userGuid)
            {
                if (model.Accepted)
                {
                    participatorViewModel.InvitationState = PlayerInvitationState.Accepted;
                    participatorViewModel.SelectedTribe = model.TribeId;
                    participatorViewModel.SelectedTribeSkin = model.TribeSkinId;
                }
                else
                {
                    participatorViewModel.InvitationState = PlayerInvitationState.Declined;
                }

                await _lobbyRepository.UpdateAsync(lobby);

                break;
            }
        }

        foreach (var lobbySubscribers in LobbySubscribers[lobby.Id])
        {
            await lobbySubscribers.proxy.SendAsync("OnLobbyUpdated", lobby.ToViewModel());
        }

        return new ServerResponse<LobbyGameViewModel>(lobby.ToViewModel());
    }

    public async Task<ServerResponse<LobbyGameViewModel>> StartLobbyGame(StartLobbyBindingModel model)
    {
        var lobby = await _lobbyRepository.GetByIdAsync(model.LobbyId);

        if (lobby == null)
        {
            return new ServerResponse<LobbyGameViewModel>()
                { Success = false, ErrorCode = ErrorCode.GameNotFound, ErrorMessage = "Lobby not found" };
        }

        _logger.LogInformation("Starting game {lobbyId}", lobby.Id);
        var result = await _gameManager.CreateGame(lobby.ToViewModel());

        lobby.StartTime = DateTime.Now;
        lobby.StartedGameId = lobby.Id;

        if (lobby.MatchmakingGameId != null)
        {
            _logger.LogInformation("Deleting matchmaking {lobbyId} because game started", lobby.Id);
            await _matchmakingRepository.DeleteByIdAsync(lobby.Id);
        }

        if (result)
        {
            _logger.LogInformation("Deleting lobby {lobbyId} because game started", lobby.Id);
            var lobbyDeleted = await _lobbyRepository.DeleteAsync(model.LobbyId);

            var game = await _gameRepository.GetByIdAsync(lobby.Id);
            foreach (var lobbySubscriber in LobbySubscribers[lobby.Id])
            {
                lobby.Participators.Clear();
                await lobbySubscriber.proxy.SendAsync("OnLobbyUpdated", lobby.ToViewModel());

                await lobbySubscriber.proxy.SendAsync("OnGameSummaryUpdated",
                    _gameManager.GetGameSummaryViewModelByGameViewModel(game.ToViewModel()), StateUpdateReason.ValidStartGame);

                Subscribe(GameSummariesSubscribers, game.Id, lobbySubscriber.id, lobbySubscriber.proxy);
            }

            return new ServerResponse<LobbyGameViewModel>(lobby.ToViewModel()) { Success = lobbyDeleted };
        }

        return new ServerResponse<LobbyGameViewModel>(lobby.ToViewModel())
            { Success = false, ErrorCode = ErrorCode.StartGameFailed, ErrorMessage = "Could not create game" };
    }
}
using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Game;
using PolytopiaB2.Carrier.Game.Lobby;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaB2.Carrier.Hubs;

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

        var response = new GetLobbyInvitationsViewModel() { Lobbies = myLobbies };
        return new ServerResponse<GetLobbyInvitationsViewModel>(response);
    }

    public async Task<ServerResponse<LobbyGameViewModel>> CreateLobby(CreateLobbyBindingModel model)
    {
        var ownUser = await _userRepository.GetByIdAsync(_userGuid);
        if (ownUser == null)
            return new ServerResponse<LobbyGameViewModel>()
                { Success = false, ErrorCode = ErrorCode.UserNotFound, ErrorMessage = "Own user not found." };

        var response = PolydystopiaLobbyManager.CreateLobby(model, ownUser);

        await _lobbyRepository.CreateAsync(response);

        return new ServerResponse<LobbyGameViewModel>(response);
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
            }
        }

        await _lobbyRepository.UpdateAsync(lobby, LobbyUpdatedReason.PlayersInvited);

        await Clients.Caller.SendAsync("OnLobbyUpdated", lobby); //TODO: Maybe send to all?

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

                return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
            }
        }

        if (lobby != null && lobby.Participators.Any(p => p.UserId == _userGuid))
        {
            _logger.LogInformation("User {ownUserId} left lobby {lobbyId}", _userGuid, lobbyId);
            ;

            lobby.Participators.RemoveAll(p => p.UserId == _userGuid);

            await _lobbyRepository.UpdateAsync(lobby, LobbyUpdatedReason.PlayerLeftByRequest);

            //await Clients.Caller.SendAsync("OnLobbyUpdated", lobby); //TODO: Maybe send to all? Or all except caller?

            return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
        }

        _logger.LogWarning("User {ownUserId} wanted to leave lobby {lobbyId} where he is no member", _userGuid,
            lobbyId);
        ;

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

                await _lobbyRepository.UpdateAsync(lobby, LobbyUpdatedReason.PlayerRespondedToInvitation);

                break;
            }
        }

        return new ServerResponse<LobbyGameViewModel>(lobby);
    }

    public async Task<ServerResponse<LobbyGameViewModel>> StartLobbyGame(StartLobbyBindingModel model)
    {
        var lobby = await _lobbyRepository.GetByIdAsync(model.LobbyId);

        if (lobby == null)
        {
            return new ServerResponse<LobbyGameViewModel>() { Success = false };
        }

        var result = await PolydystopiaGameManager.CreateGame(lobby, _gameRepository);

        lobby.StartedGameId = lobby.Id;

        if (result)
        {
            var lobbyDeleted = await _lobbyRepository.DeleteAsync(model.LobbyId);

            return new ServerResponse<LobbyGameViewModel>(lobby) { Success = lobbyDeleted };
        }

        return new ServerResponse<LobbyGameViewModel>(lobby) { Success = false };
    }
}
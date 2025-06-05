using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Game;
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
        foreach (var lobbyGameViewModel in myLobbies)
        {
            foreach (var participatorViewModel in lobbyGameViewModel.Participators)
            {
                participatorViewModel.InvitationState = PlayerInvitationState.Accepted;
            }
        }

        var response = new GetLobbyInvitationsViewModel() { Lobbies = myLobbies };
        return new ServerResponse<GetLobbyInvitationsViewModel>(response);
    }

    public async Task<ServerResponse<LobbyGameViewModel>> CreateLobby(CreateLobbyBindingModel model)
    {
        var response = new LobbyGameViewModel();
        response.Id = Guid.NewGuid();
        response.UpdatedReason = LobbyUpdatedReason.Created;
        response.DateCreated = DateTime.Now;
        response.DateModified = DateTime.Now;
        response.Name = "Love you " + model.GameName; //TODO: Cooler names
        response.MapPreset = model.MapPreset;
        response.MapSize = model.MapSize;
        response.OpponentCount = model.OpponentCount;
        response.GameMode = model.GameMode;
        response.OwnerId = _userGuid;
        response.DisabledTribes = model.DisabledTribes;
        //response.StartedGameId = Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360");
        response.IsPersistent = model.IsPersistent; //?
        response.IsSharable = true; //?
        response.TimeLimit = model.TimeLimit;
        response.ScoreLimit = model.ScoreLimit;
        response.InviteLink = "https://play.polytopia.io/lobby/4114-281c-464c-a8e7-6a79f4496360"; //TODO ?
        //response.MatchmakingGameId = response.Id.GetHashCode(); //?
        //response.ChallengermodeGameId = response.Id; //?
        //response.StartTime = DateTime.Now; //?
        response.GameContext = new GameContext()
        {
            ExternalMatchId = response.Id, //?
            ExternalTournamentId = response.Id, //?
        };
        response.Participators = new List<ParticipatorViewModel>();

        var ownUser = await _userRepository.GetByIdAsync(_userGuid);

        if (ownUser == null) return new ServerResponse<LobbyGameViewModel>(response) { Success = false };

        response.Participators.Add(new ParticipatorViewModel()
        {
            UserId = _userGuid,
            Name = ownUser.GetUniqueNameInternal(),
            NumberOfFriends = ownUser.NumFriends ?? 0,
            NumberOfMultiplayerGames = ownUser.NumMultiplayergames ?? 0,
            GameVersion = ownUser.GameVersions,
            MultiplayerRating = ownUser.MultiplayerRating ?? 0,
            SelectedTribe = model.OwnerTribe,
            SelectedTribeSkin = model.OwnerTribeSkin,
            AvatarStateData = ownUser.AvatarStateData,
            InvitationState = PlayerInvitationState.Accepted
        });


        response.Bots = new List<int>();

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
            await _lobbyRepository.DeleteAsync(lobbyId);

            return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
        }

        if (lobby != null && lobby.Participators.Any(p => p.UserId == _userGuid))
        {
            //TODO: What happens if the user is the owner?

            lobby.Participators.RemoveAll(p => p.UserId == _userGuid);

            await _lobbyRepository.UpdateAsync(lobby, LobbyUpdatedReason.PlayerLeftByRequest);

            //await Clients.Caller.SendAsync("OnLobbyUpdated", lobby); //TODO: Maybe send to all? Or all except caller?

            return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = true });
        }

        return new ServerResponse<BoolResponseViewModel>(new BoolResponseViewModel() { Result = false });
    }

    public ServerResponse<LobbyGameViewModel> RespondToLobbyInvitation(RespondToLobbyInvitation model) //TODO
    {
        var response = new LobbyGameViewModel();

        return new ServerResponse<LobbyGameViewModel>(response);
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
using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using Dystopia.Game.Lobby;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;

namespace Dystopia.Game.Matchmaking;

public static class PolydystopiaMatchmakingManager
{
    public static async Task<MatchmakingSubmissionViewModel> QueuePlayer(Guid playerId,
        SubmitMatchmakingBindingModel model,
        IClientProxy ownProxy,
        IPolydystopiaMatchmakingRepository _matchmakingRepository,
        IPolydystopiaUserRepository _userRepository,
        IPolydystopiaLobbyRepository _lobbyRepository)
    { // TODO make service instead of static method
        var fittingLobbies = await _matchmakingRepository.GetAllFittingLobbies(playerId, model.Version, model.MapSize,
            model.MapPreset, model.GameMode, model.ScoreLimit, model.TimeLimit, model.Platform, model.AllowCrossPlay);

        var selectedLobby = fittingLobbies
            .OrderByDescending(lobby => lobby.PlayerIds.Count)
            .FirstOrDefault();

        var ownUser = await _userRepository.GetByIdAsync(playerId);
        if(ownUser == null) return null;

        if (selectedLobby == null)
        {
            var lobbyGameViewModel = PolydystopiaLobbyManager.CreateLobby(model, (PolytopiaUserViewModel) ownUser);
            var participator = new ParticipatorViewModel()
            {
                UserId = ownUser.PolytopiaId,
                Name = ownUser.UserName,
                NumberOfFriends = ownUser.NumFriends,
                NumberOfMultiplayerGames = ownUser.NumGames,
                GameVersion = new List<ClientGameVersionViewModel>(),
                MultiplayerRating = ownUser.Elo,
                SelectedTribe = 0, //?
                SelectedTribeSkin = 0, //?
                AvatarStateData = ownUser.AvatarStateData,
                InvitationState = PlayerInvitationState.Invited
            };

            lobbyGameViewModel.Participators.Add(participator);

            var maxPlayers = model.OpponentCount != 0 ? model.OpponentCount+1 : (short)Random.Shared.Next(2, 9);

            var maxPlayers = model.OpponentCount != 0 ? model.OpponentCount : (short)Random.Shared.Next(2, 9);

            selectedLobby = new MatchmakingEntity
            {
                Id = Guid.NewGuid(),
                LobbyGameViewModelId = lobbyGameViewModel.Id,
                LobbyGameViewModel = null,
                Version = 0, //TODO,
                MapSize = model.MapSize,
                MapPreset = model.MapPreset,
                GameMode = model.GameMode,
                ScoreLimit = model.ScoreLimit,
                TimeLimit = model.TimeLimit,
                Platform = model.Platform,
                AllowCrossPlay = true,
                MaxPlayers = maxPlayers,
                PlayerIds = lobbyGameViewModel.Participators.Select(p => p.UserId).ToList(),

            };
            await _lobbyRepository.CreateAsync(lobbyGameViewModel.ToLobbyEntity());
            await _matchmakingRepository.CreateAsync(selectedLobby);
        }
        else
        {
            selectedLobby.PlayerIds.Add(playerId);

            var participator = new ParticipatorViewModel()
            {
                UserId = ownUser.PolytopiaId,
                Name = ownUser.Alias,
                NumberOfFriends = ownUser.NumFriends,
                NumberOfMultiplayerGames = ownUser.NumGames,
                GameVersion = new List<ClientGameVersionViewModel>(),
                MultiplayerRating = ownUser.Elo,
                SelectedTribe = 0, //?
                SelectedTribeSkin = 0, //?
                AvatarStateData = ownUser.AvatarStateData,
                InvitationState = PlayerInvitationState.Invited
            };

            selectedLobby.LobbyGameViewModel.Participators.Add((LobbyPlayerEntity)participator);

            await _lobbyRepository.UpdateAsync(selectedLobby.LobbyGameViewModel, LobbyUpdatedReason.PlayersInvited);
            await _matchmakingRepository.UpdateAsync(selectedLobby);
        }

        await ownProxy.SendAsync("OnLobbyInvitation", selectedLobby.LobbyGameViewModel);

        var submission = new MatchmakingSubmissionViewModel();
        submission.GameName = selectedLobby.LobbyGameViewModel.Name;
        submission.IsWaitingForOpponents = selectedLobby.MaxPlayers - selectedLobby.PlayerIds.Count > 0;

        var summary = new MatchmakingGameSummaryViewModel();
        summary.Id = 0; // ?
        summary.DateCreated = DateTime.Now;
        summary.DateModified = DateTime.Now;
        summary.Name = selectedLobby.LobbyGameViewModel.Name;
        summary.MapPreset = selectedLobby.MapPreset;
        summary.MapSize = selectedLobby.MapSize;
        summary.OpponentCount = (short)(selectedLobby.MaxPlayers - 1);
        summary.GameMode = selectedLobby.GameMode;
        summary.WithPickedTribe = model.SelectedTribe != 0; //?
        summary.LobbyId = selectedLobby.LobbyGameViewModel.Id;

        summary.Participators = new List<ParticipatorViewModel>();
        foreach (var participatorViewModel in selectedLobby.LobbyGameViewModel.Participators)
        {
            summary.Participators.Add((ParticipatorViewModel)participatorViewModel);
        }

        submission.MatchmakingGameSummaryViewModel = summary;

        return submission;
    }
}
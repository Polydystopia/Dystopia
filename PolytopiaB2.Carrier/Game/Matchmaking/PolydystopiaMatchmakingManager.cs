using Microsoft.AspNetCore.SignalR;
using PolytopiaB2.Carrier.Database.Lobby;
using PolytopiaB2.Carrier.Database.Matchmaking;
using PolytopiaB2.Carrier.Database.User;
using PolytopiaB2.Carrier.Game.Lobby;
using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Game.Matchmaking;

public static class PolydystopiaMatchmakingManager
{
    public static async Task<MatchmakingSubmissionViewModel?> QueuePlayer(Guid playerId, SubmitMatchmakingBindingModel model, IClientProxy ownProxy, IPolydystopiaMatchmakingRepository _matchmakingRepository, IPolydystopiaUserRepository _userRepository, IPolydystopiaLobbyRepository _lobbyRepository)
    {
        var fittingLobbies = await _matchmakingRepository.GetAllFittingLobbies(playerId, model.Version, model.MapSize, model.MapPreset, model.GameMode, model.ScoreLimit, model.TimeLimit, model.Platform, model.AllowCrossPlay);

        var selectedLobby = fittingLobbies
            .OrderByDescending(lobby => lobby.PlayerIds.Count)
            .FirstOrDefault();

        var ownUser = await _userRepository.GetByIdAsync(playerId);
        if(ownUser == null) return null;

        if (selectedLobby == null)
        {
            var lobbyGameViewModel = PolydystopiaLobbyManager.CreateLobby(model, ownUser);
            var participator = new ParticipatorViewModel()
            {
                UserId = ownUser.PolytopiaId,
                Name = ownUser.GetUniqueNameInternal(),
                NumberOfFriends = ownUser.NumFriends ?? 0,
                NumberOfMultiplayerGames = ownUser.NumMultiplayergames ?? 0,
                GameVersion = ownUser.GameVersions,
                MultiplayerRating = ownUser.MultiplayerRating ?? 0,
                SelectedTribe = 0, //?
                SelectedTribeSkin = 0, //?
                AvatarStateData = ownUser.AvatarStateData,
                InvitationState = PlayerInvitationState.Invited
            };

            lobbyGameViewModel.Participators.Add(participator);

            var maxPlayers = model.OpponentCount != 0 ? model.OpponentCount : (short)Random.Shared.Next(2, 9);

            selectedLobby = new MatchmakingEntity(lobbyGameViewModel, model.Version, model.MapSize, model.MapPreset, model.GameMode, model.ScoreLimit, model.TimeLimit, model.Platform, model.AllowCrossPlay, maxPlayers);
            await _lobbyRepository.CreateAsync(lobbyGameViewModel);
            await _matchmakingRepository.CreateAsync(selectedLobby);
        }
        else
        {
            selectedLobby.PlayerIds.Add(playerId);

            var participator = new ParticipatorViewModel()
            {
                UserId = ownUser.PolytopiaId,
                Name = ownUser.GetUniqueNameInternal(),
                NumberOfFriends = ownUser.NumFriends ?? 0,
                NumberOfMultiplayerGames = ownUser.NumMultiplayergames ?? 0,
                GameVersion = ownUser.GameVersions,
                MultiplayerRating = ownUser.MultiplayerRating ?? 0,
                SelectedTribe = 0, //?
                SelectedTribeSkin = 0, //?
                AvatarStateData = ownUser.AvatarStateData,
                InvitationState = PlayerInvitationState.Invited
            };

            selectedLobby.LobbyGameViewModel.Participators.Add(participator);

            await _lobbyRepository.UpdateAsync(selectedLobby.LobbyGameViewModel, LobbyUpdatedReason.PlayersInvited);
            await _matchmakingRepository.UpdateAsync(selectedLobby);
        }

        await ownProxy.SendAsync("OnLobbyInvitation", selectedLobby.LobbyGameViewModel);

        var submission = new MatchmakingSubmissionViewModel();
        submission.GameName = selectedLobby.LobbyGameViewModel.Name;
        submission.IsWaitingForOpponents = selectedLobby.MaxPlayers - selectedLobby.PlayerIds.Count > 0;

        var summary = new MatchmakingGameSummaryViewModel();
        summary.Id = (long)selectedLobby.LobbyGameViewModel.MatchmakingGameId;
        summary.DateCreated = selectedLobby.LobbyGameViewModel.DateCreated;
        summary.DateModified = selectedLobby.LobbyGameViewModel.DateModified;
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
            summary.Participators.Add(participatorViewModel);
        }

        submission.MatchmakingGameSummaryViewModel = summary;

        return submission;
    }
}
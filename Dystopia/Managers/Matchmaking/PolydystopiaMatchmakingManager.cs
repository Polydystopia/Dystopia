using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using Dystopia.Managers.Lobby;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Matchmaking;

public static class PolydystopiaMatchmakingManager
{
    public static async Task<MatchmakingSubmissionViewModel?> QueuePlayer(Guid playerId, SubmitMatchmakingBindingModel model, IClientProxy ownProxy, IPolydystopiaMatchmakingRepository _matchmakingRepository, IPolydystopiaUserRepository _userRepository, IPolydystopiaLobbyRepository _lobbyRepository)
    {
        var fittingLobbies = await _matchmakingRepository.GetAllFittingLobbies(playerId, model.Version, model.MapSize, model.MapPreset, model.GameMode, model.ScoreLimit, model.TimeLimit, model.Platform, model.AllowCrossPlay);

        var selectedMatchmaking = fittingLobbies
            .OrderByDescending(lobby => lobby.PlayerIds.Count)
            .FirstOrDefault();

        var ownUser = await _userRepository.GetByIdAsync(playerId);
        if(ownUser == null) return null;

        if (selectedMatchmaking == null)
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

            var maxPlayers = model.OpponentCount != 0 ? model.OpponentCount+1 : (short)Random.Shared.Next(2, 9);
            lobbyGameViewModel.OpponentCount = (short)(maxPlayers-1);

            var lobbyEntity = lobbyGameViewModel.ToEntity();

            selectedMatchmaking = new MatchmakingEntity(lobbyEntity, model.Version, lobbyGameViewModel.MapSize, lobbyGameViewModel.MapPreset, lobbyGameViewModel.GameMode, lobbyGameViewModel.ScoreLimit, lobbyGameViewModel.TimeLimit, model.Platform, model.AllowCrossPlay, maxPlayers);
            await _lobbyRepository.CreateAsync(lobbyEntity);
            await _matchmakingRepository.CreateAsync(selectedMatchmaking);
        }
        else
        {
            selectedMatchmaking.PlayerIds.Add(playerId);

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

            selectedMatchmaking.LobbyEntity.Participators.Add(participator);

            await _lobbyRepository.UpdateAsync(selectedMatchmaking.LobbyEntity);
            await _matchmakingRepository.UpdateAsync(selectedMatchmaking);
        }

        await ownProxy.SendAsync("OnLobbyInvitation", selectedMatchmaking.LobbyEntity.ToViewModel());

        var submission = new MatchmakingSubmissionViewModel();
        submission.GameName = selectedMatchmaking.LobbyEntity.Name;
        submission.IsWaitingForOpponents = selectedMatchmaking.MaxPlayers - selectedMatchmaking.PlayerIds.Count > 0;

        var summary = new MatchmakingGameSummaryViewModel();
        summary.Id = (long)selectedMatchmaking.LobbyEntity.MatchmakingGameId;
        summary.DateCreated = selectedMatchmaking.LobbyEntity.DateCreated;
        summary.DateModified = selectedMatchmaking.LobbyEntity.DateModified;
        summary.Name = selectedMatchmaking.LobbyEntity.Name;
        summary.MapPreset = selectedMatchmaking.MapPreset;
        summary.MapSize = selectedMatchmaking.MapSize;
        summary.OpponentCount = (short)(selectedMatchmaking.MaxPlayers - 1);
        summary.GameMode = selectedMatchmaking.GameMode;
        summary.WithPickedTribe = model.SelectedTribe != 0; //?
        summary.LobbyId = selectedMatchmaking.LobbyEntity.Id;

        summary.Participators = new List<ParticipatorViewModel>();
        foreach (var participatorViewModel in selectedMatchmaking.LobbyEntity.Participators)
        {
            summary.Participators.Add(participatorViewModel);
        }

        submission.MatchmakingGameSummaryViewModel = summary;

        return submission;
    }
}
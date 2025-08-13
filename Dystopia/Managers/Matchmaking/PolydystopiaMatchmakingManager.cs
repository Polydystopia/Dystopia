using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using Dystopia.Hubs;
using Dystopia.Managers.Lobby;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Matchmaking;

public static class PolydystopiaMatchmakingManager
{
    public static async Task<MatchmakingSubmissionViewModel?> QueuePlayer(Guid playerId, SubmitMatchmakingBindingModel model, IDystopiaHubClient ownProxy, IPolydystopiaMatchmakingRepository _matchmakingRepository, IPolydystopiaUserRepository _userRepository, IPolydystopiaLobbyRepository _lobbyRepository)
    {
        var fittingLobbies = await _matchmakingRepository.GetAllFittingLobbies(playerId, model.Version, model.MapSize, model.MapPreset, model.GameMode, model.ScoreLimit, model.TimeLimit, model.Platform, model.AllowCrossPlay);

        var selectedMatchmaking = fittingLobbies
            .OrderByDescending(lobby => lobby.PlayerIds.Count)
            .FirstOrDefault();

        var ownUser = await _userRepository.GetByIdAsync(playerId);
        if(ownUser == null) return null;

        if (selectedMatchmaking == null)
        {
            var lobbyEntity = PolydystopiaLobbyManager.CreateLobby(model, ownUser);

            selectedMatchmaking = new MatchmakingEntity(lobbyEntity, model.Version, lobbyEntity.MapSize, lobbyEntity.MapPreset, lobbyEntity.GameMode, lobbyEntity.ScoreLimit, lobbyEntity.TimeLimit, model.Platform, model.AllowCrossPlay, lobbyEntity.MaxPlayers);
            await _lobbyRepository.CreateAsync(lobbyEntity);
            await _matchmakingRepository.CreateAsync(selectedMatchmaking);
        }
        else
        {
            selectedMatchmaking.PlayerIds.Add(playerId);

            var participator = new LobbyParticipatorUserEntity()
            {
                UserId = ownUser.Id,
                InvitationState = PlayerInvitationState.Invited,
            };

            selectedMatchmaking.LobbyEntity.Participators.Add(participator);

            await _lobbyRepository.UpdateAsync(selectedMatchmaking.LobbyEntity);
            await _matchmakingRepository.UpdateAsync(selectedMatchmaking);
        }

        await ownProxy.OnLobbyInvitation(selectedMatchmaking.LobbyEntity.ToViewModel());

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
        foreach (var participatorViewModel in selectedMatchmaking.LobbyEntity.Participators.ToViewModels())
        {
            summary.Participators.Add(participatorViewModel);
        }

        submission.MatchmakingGameSummaryViewModel = summary;

        return submission;
    }
}
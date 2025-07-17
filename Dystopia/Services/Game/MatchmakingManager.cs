using Dystopia.Database.Lobby;
using Dystopia.Database.Matchmaking;
using Dystopia.Database.User;
using Dystopia.Game.Lobby;
using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Services.Game;

public class MatchmakingManager : IMatchmakingManager
{
    private readonly IPolydystopiaMatchmakingRepository _matchmakingRepository;
    private readonly IPolydystopiaUserRepository _userRepository;
    private readonly IPolydystopiaLobbyRepository _lobbyRepository;

    public MatchmakingManager(IPolydystopiaMatchmakingRepository matchmakingRepository,
        IPolydystopiaUserRepository userRepository,
        IPolydystopiaLobbyRepository lobbyRepository)
    {
        _matchmakingRepository = matchmakingRepository;
        _userRepository = userRepository;
        _lobbyRepository = lobbyRepository;
    }
    public async Task<MatchmakingSubmissionViewModel?> QueuePlayer(Guid playerId,
        SubmitMatchmakingBindingModel model,
        IClientProxy ownProxy
        )
    {
        var selectedLobby = await _matchmakingRepository.GetMostFittingLobbyOrDefault(new MatchMakingFilter
        {
            PlayerId       = playerId,
            Version        = model.Version,
            MapSize        = model.MapSize,
            MapPreset      = model.MapPreset,
            GameMode       = model.GameMode,
            ScoreLimit     = model.ScoreLimit,
            TimeLimit      = model.TimeLimit,
            Platform       = model.Platform,
            AllowCrossPlay = model.AllowCrossPlay
        });

        var ownUser = await _userRepository.GetByIdAsync(playerId);
        if(ownUser == null) return null;

        if (selectedLobby == null)
        {
            var lobby = PolydystopiaLobbyManager.CreateLobby(model, ownUser, out var opponents);
            
            selectedLobby = new MatchmakingEntity
            {
                Id = Guid.NewGuid(),
                LobbyGameViewModelId = lobby.Id,
                LobbyGameViewModel = null,
                Version = 0, //TODO,
                MapSize = model.MapSize,
                MapPreset = model.MapPreset,
                GameMode = model.GameMode,
                ScoreLimit = model.ScoreLimit,
                TimeLimit = model.TimeLimit,
                Platform = model.Platform,
                AllowCrossPlay = true,
                MaxPlayers = opponents+1,
                Players = lobby.Participators.Select(p => p.User!).ToList(),
            };
            await _lobbyRepository.CreateAsync(lobby);
            await _matchmakingRepository.CreateAsync(selectedLobby);
        }
        else
        {
            selectedLobby.Players.Add(playerId); // TODO 

            var participator = new LobbyPlayerEntity
            {
                UserId = playerId,
                User = null,
                LobbyId = selectedLobby.LobbyGameViewModelId,
                Lobby = selectedLobby.LobbyGameViewModel,
                DateLastCommand = null,
                DateLastStartTurn = null,
                DateLastEndTurn = null,
                DateCurrentTurnDeadline = null,
                TimeBank = null,
                LastConsumedTimeBank = null,
                InvitationState = PlayerInvitationState.Invited,
                SelectedTribe = model.SelectedTribe, // TODO how do we have this info already?
                SelectedTribeSkin = 0,
                AutoSkipStrikeCount = 0
            };

            selectedLobby.LobbyGameViewModel.Participators.Add(participator);

            await _lobbyRepository.UpdateAsync(selectedLobby.LobbyGameViewModel, LobbyUpdatedReason.PlayersInvited);
            await _matchmakingRepository.UpdateAsync(selectedLobby);
        }

        await ownProxy.SendAsync("OnLobbyInvitation", selectedLobby.LobbyGameViewModel);

        var submission = new MatchmakingSubmissionViewModel();
        submission.GameName = selectedLobby.LobbyGameViewModel.Name;
        submission.IsWaitingForOpponents = selectedLobby.MaxPlayers - selectedLobby.Players.Count > 0;

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
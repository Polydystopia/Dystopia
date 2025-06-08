using Newtonsoft.Json;
using PolytopiaB2.Carrier.Game;
using PolytopiaB2.Carrier.Patches;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.ViewModels;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<GameListingViewModel>> GetGameListingsV3()
    {
        PolytopiaDataManager.provider = new MyProvider(); //?

        var response = new GameListingViewModel();
        response.gameSummaries = new List<GameSummaryViewModel>();
        response.matchmakingGameSummaries = new List<MatchmakingGameSummaryViewModel>(); //TODO

        var myGames = await _gameRepository.GetAllGamesByPlayer(_userGuid);
        foreach (var game in myGames)
        {
            var succ = GameStateSummary.FromGameStateByteArray(game.CurrentGameStateData,
                out GameStateSummary stateSummary, out var gameState);

            var gameSettings = JsonConvert.DeserializeObject<GameSettings>(game.GameSettingsJson);

            var summary = new GameSummaryViewModel();
            summary.GameId = game.Id;
            summary.MatchmakingGameId = null;
            summary.OwnerId = game.OwnerId;
            summary.DateCreated = game.DateCreated;
            summary.DateLastCommand = game.DateLastCommand;
            summary.DateLastEndTurn = DateTime.Now.Subtract(TimeSpan.FromMinutes(10)); //TODO
            summary.DateEnded = null; //TODO
            summary.TimeLimit = 3600; //gameSettings.TimeLimit TODO
            summary.State = game.State;
            summary.Participators = new List<ParticipatorViewModel>();
            foreach (var player in gameSettings.players)
            {
                var playerData = player.Value;

                var participator = new ParticipatorViewModel()
                {
                    UserId = player.Key,
                    Name = playerData.GetNameInternal(), //TODO
                    NumberOfFriends = playerData.profile.numFriends,
                    NumberOfMultiplayerGames = playerData.profile.numMultiplayerGames,
                    GameVersion = new List<ClientGameVersionViewModel>() {}, //TODO
                    MultiplayerRating = playerData.profile.multiplayerRating,
                    SelectedTribe = 2, //TODO
                    SelectedTribeSkin = 0, //TODO
                    AvatarStateData = SerializationHelpers.ToByteArray(playerData.profile.avatarState, gameState.Version),
                    InvitationState = PlayerInvitationState.Accepted
                };

                summary.Participators.Add(participator);
            }
            summary.Result = null; //?

            summary.GameSummaryData = SerializationHelpers.ToByteArray(stateSummary, gameState.Version);
            summary.GameContext = new GameContext(); //?

            response.gameSummaries.Add(summary);
        }

        return new ServerResponse<GameListingViewModel>(response);
    }

    public async Task<ServerResponse<ResponseViewModel>> Resign(ResignBindingModel model)
    {
        var res = await PolydystopiaGameManager.Resign(model, _gameRepository, _userGuid);

        return new ServerResponse<ResponseViewModel>();
    }

    public async Task<ServerResponse<ResponseViewModel>> SendCommand(SendCommandBindingModel model)
    {
        var res = await PolydystopiaGameManager.SendCommand(model, _gameRepository, _userGuid);

        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }
}
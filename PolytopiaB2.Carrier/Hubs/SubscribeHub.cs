using PolytopiaBackendBase;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game.BindingModels;

namespace PolytopiaB2.Carrier.Hubs;

public partial class PolytopiaHub
{
    public ServerResponse<ResponseViewModel> SubscribeToParticipatingGameSummaries()
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> SubscribeToFriends()
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<BoolResponseViewModel> SubscribeToLobby(SubscribeToLobbyBindingModel model)
    {
        var response = new BoolResponseViewModel();
        response.Result = true;

        return new ServerResponse<BoolResponseViewModel>(response);
    }

    public ServerResponse<ResponseViewModel> SubscribeToGame(SubscribeToGameBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }
}
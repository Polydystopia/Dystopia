using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

namespace Dystopia.Hubs;

public partial class PolytopiaHub
{
    public async Task<ServerResponse<ResponseViewModel>> UpdateAvatar(AvatarBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();

        var user = await _userRepository.GetByIdAsync(_userGuid);

        if (user == null) return new ServerResponse<ResponseViewModel>(ErrorCode.UserNotFound, "User not found");

        var validAvatar = SerializationHelpers.FromByteArray<AvatarState>(model.AvatarStateData, out _);
        if (!validAvatar) return new ServerResponse<ResponseViewModel>(ErrorCode.InvalidUserCommand, "Avatar is invalid");

        user.AvatarStateData = model.AvatarStateData;
        await _userRepository.UpdateAsync(user);

        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> UploadNumSingleplayerGames(UploadNumSingleplayerGamesBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }

    public ServerResponse<ResponseViewModel> UploadTribeRating(UploadTribeRatingBindingModel model)
    {
        var responseViewModel = new ResponseViewModel();
        return new ServerResponse<ResponseViewModel>(responseViewModel);
    }
}
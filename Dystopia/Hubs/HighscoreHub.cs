using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;

namespace Dystopia.Hubs;

public partial class DystopiaHub
{
    public async Task<ServerResponseList<HighscoreViewModel>> GetHighscores(
        TribeHighscoresBindingModel model)
    {
        var highscores = new List<HighscoreViewModel>();

        var highscoreEntities = model.TribeType != null
            ? await _highscoreRepository.GetByTribeAsync((TribeData.Type)model.TribeType)
            : await _highscoreRepository.GetAsync();

        foreach (var highscoreEntity in highscoreEntities)
        {
            var highscoreViewModel = new HighscoreViewModel()
            {
                PolytopiaUserId = highscoreEntity.UserId,
                Username = highscoreEntity.User.Alias,
                Score = highscoreEntity.Score,
                TribeType = (int)highscoreEntity.Tribe,
                AvatarStateData = highscoreEntity.User.AvatarStateData,
                GameVersions = highscoreEntity.User.GameVersions
            };

            highscores.Add(highscoreViewModel);
        }

        return new ServerResponseList<HighscoreViewModel>(highscores);
    }

    public async Task<ServerResponse<ResponseViewModel>> UploadTribeRating(
        UploadTribeRatingBindingModel model)
    {
        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
}
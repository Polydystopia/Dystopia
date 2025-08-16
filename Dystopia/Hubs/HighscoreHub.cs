using Dystopia.Database.TribeRating;
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

    public async Task<ServerResponse<TribeRatingsViewModel>> GetTribeRatings()
    {
        var ratings = await _tribeRatingRepository.GetByUserAsync(_userGuid);

        var response = new TribeRatingsViewModel();
        response.PolytopiaUserId = Guid.Parse(_userId);
        response.Ratings = new Dictionary<int, TribeRatingViewModel>();

        if (ratings == null) return new ServerResponse<TribeRatingsViewModel>(response);

        foreach (var tribeRatingEntity in ratings)
        {
            response.Ratings.Add((int)tribeRatingEntity.Tribe, new TribeRatingViewModel()
            {
                TribeType = (int)tribeRatingEntity.Tribe,
                Rating = tribeRatingEntity.Rating,
                Score = tribeRatingEntity.Score
            });
        }

        return new ServerResponse<TribeRatingsViewModel>(response);
    }

    public async Task<ServerResponse<ResponseViewModel>> UploadTribeRating(
        UploadTribeRatingBindingModel model)
    {
        var user = await _userRepository.GetByIdAsync(_userGuid);
        if (user == null) return new ServerResponse<ResponseViewModel>(ErrorCode.UserNotFound, "User not found");

        var tasks = new List<Task>();
        foreach (var tribeRatingViewModel in model.Entries)
        {
            var entity = new TribeRatingEntity
            {
                Tribe = (TribeData.Type)tribeRatingViewModel.Key,
                UserId = user.Id,
                Rating = tribeRatingViewModel.Value.Rating,
                Score = tribeRatingViewModel.Value.Score,
            };

            tasks.Add(_tribeRatingRepository.AddOrUpdateAsync(entity));
        }

        await Task.WhenAll(tasks);

        return new ServerResponse<ResponseViewModel>(new ResponseViewModel());
    }
}
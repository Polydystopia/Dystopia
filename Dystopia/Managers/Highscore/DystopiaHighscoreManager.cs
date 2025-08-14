using Dystopia.Bridge;
using Dystopia.Database.Highscore;
using Dystopia.Database.User;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Highscore;

public class DystopiaHighscoreManager(IDystopiaHighscoreRepository highscoreRepository, ILogger<DystopiaHighscoreManager> logger)
    : IDystopiaHighscoreManager
{
    public bool ProcessHighscore(UploadHighscoresBindingModel model, UserEntity user)
    {
        logger.LogDebug("Trying to upload highscore for user {user}", user.UserName);

        try
        {
            var bridge = new DystopiaBridge();

            var success = bridge.ProcessHighscore(model.CurrentGameStateData, user.Alias, out var tribe, out var score);

            if (!success) return false;

            var highscore = new HighscoreEntity()
            {
                UserId = user.Id,
                Tribe = (TribeData.Type)tribe,
                Score = score,
                InitialGameStateData = model.InitialGameStateData,
                FinalGameStateData = model.CurrentGameStateData,
            };

            logger.LogDebug("Uploading highscore {tribe} - {score} for user {user}", tribe, score, user.UserName);

            highscoreRepository.SaveOrUpdateAsync(highscore);

            return true;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error uploading highscore for user {user}", user.UserName);
            return false;
        }
    }
}
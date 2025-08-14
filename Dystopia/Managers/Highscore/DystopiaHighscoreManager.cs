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
            var startGameStateValid = SerializationHelpers.FromByteArray(model.CurrentGameStateData, out GameState _);
            var finalGameStateValid = SerializationHelpers.FromByteArray(model.CurrentGameStateData, out GameState finalGameState);

            if (!finalGameStateValid) return false;

            var myPlayer = finalGameState.PlayerStates.FirstOrDefault(p => p.AccountId == user.Id);
            if (myPlayer == null) return false;

            var tribe = myPlayer.tribe;
            var score = myPlayer.endScore;

            if (tribe is TribeData.Type.None or TribeData.Type.Nature) return false;
            if (score == 0) return false;

            var highscore = new HighscoreEntity()
            {
                UserId = user.Id,
                Tribe = tribe,
                Score = score,
                InitialGameStateData = model.InitialGameStateData,
                FinalGameStateData = model.CurrentGameStateData,
            };

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
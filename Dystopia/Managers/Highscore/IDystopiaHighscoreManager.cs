using Dystopia.Database.User;
using PolytopiaBackendBase.Game;

namespace Dystopia.Managers.Highscore;

public interface IDystopiaHighscoreManager
{
    public bool ProcessHighscore(UploadHighscoresBindingModel model, UserEntity user);
}
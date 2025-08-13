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

        var highscore = new HighscoreViewModel();
        highscore.PolytopiaUserId = Guid.Empty;
        highscore.Username = "Not implemented yet";
        highscore.Score = 1;
        highscore.TribeType = 1;
        highscore.AvatarStateData = Convert.FromHexString("70000000280000000C000000000000000D00000000000000180000000000000023000000000000002900000000000000");
        highscore.GameVersions = new List<ClientGameVersionViewModel>();

        highscores.Add(highscore);

        return new ServerResponseList<HighscoreViewModel>(highscores);
    }
}
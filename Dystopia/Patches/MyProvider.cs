namespace Dystopia.Patches;

public class MyProvider : IPolytopiaDataProvider
{
    public string LoadAvatarData(int version)
    {
        var avatarDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "AvatarData", "1.txt");
        return File.ReadAllText(avatarDataPath);
    }

    public string LoadGameLogicData(int version)
    {
        var gameLogicDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "GameLogicData", "19.txt");
        return File.ReadAllText(gameLogicDataPath);
    }
}
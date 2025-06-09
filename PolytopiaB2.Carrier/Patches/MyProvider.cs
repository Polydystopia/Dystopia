namespace PolytopiaB2.Carrier.Patches;

public class MyProvider : IPolytopiaDataProvider
{
    public string LoadAvatarData(int version)
    {
        var avatarDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "Data", "AvatarData", "1.txt");
        return File.ReadAllText(avatarDataPath);
    }

    public string LoadGameLogicData(int version)
    {
        var gameLogicDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "External", "Data", "GameLogicData", "19.txt");
        return File.ReadAllText(gameLogicDataPath);
    }
}
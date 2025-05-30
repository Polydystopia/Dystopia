namespace PolytopiaB2.Carrier.Patches;

public class MyProvider : IPolytopiaDataProvider
{
    public string LoadAvatarData(int version)
    {
        return File.ReadAllText(@"C:\Steam\steamapps\common\The Battle of Polytopia\AvatarData\1.txt");
    }

    public string LoadGameLogicData(int version)
    {
        return File.ReadAllText(@"C:\Steam\steamapps\common\The Battle of Polytopia\GameLogicData\19.txt");
    }
}
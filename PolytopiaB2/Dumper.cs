namespace PolytopiaB2;

public class Dumper
{
    public static void DumpAll()
    {
        Directory.CreateDirectory("GameLogicData");

        for (int i = 0; i < 100; i++)
        {
            var json = PolytopiaDataManager.provider.LoadGameLogicData(i);
            if (!string.IsNullOrEmpty(json))
            {
                File.WriteAllText($"GameLogicData/{i}.txt", json);
            }
        }
    }
}
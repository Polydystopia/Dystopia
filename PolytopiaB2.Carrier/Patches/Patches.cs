using HarmonyLib;

namespace PolytopiaB2.Carrier.Patches;

[HarmonyPatch(typeof(AnalyticsManager))]
[HarmonyPatch("SetCrashMetaData")]
[HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
public static class CrashReportPatch
{
    static bool Prefix(string key, string data)
    {
        Console.WriteLine($"Skipped crash metadata: {key}={data}");

        return false;
    }
}

[HarmonyPatch(typeof(Paths), "GetUserDirectory")]
public static class GetUserDirectoryPatch
{
    static bool Prefix(ref string __result)
    {
        Directory.CreateDirectory(@"C:\Steam\steamapps\common\The Battle of Polytopia\UserDirectory");
        __result = @"C:\Steam\steamapps\common\The Battle of Polytopia\UserDirectory";
        return false;
    }
}

[HarmonyPatch(typeof(ClientBase), "SaveSession")]
public static class SaveSessionPatch
{
    static bool Prefix(ClientBase __instance, string gameId, bool showSaveErrorPopup)
    {
        Console.WriteLine($"Skipped SaveSession for game {gameId}");
        return false;
    }
}

[HarmonyPatch(typeof(AnalyticsHelpers), "SendGameStartEvent")]
public static class AnalyticsHelpersSendGameStartEventPatch
{
    static bool Prefix(Guid gameId, GameSettings settings, object tribe)
    {
        Console.WriteLine($"Skipped analytics: SendGameStartEvent for game {gameId}");
        return false;
    }
}

[HarmonyPatch(typeof(GameManager))]
[HarmonyPatch("GetSyncedLocalGameDataManager")]
[HarmonyPatch(new Type[] { })]
public static class GetSyncedLocalGameDataManagerPatch
{
    static bool Prefix(ref ISyncedLocalGameDataManager __result)
    {
        __result = new MySyncedLocalGameDataManager();
        
        return false;
    }
}
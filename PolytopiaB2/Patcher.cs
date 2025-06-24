using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMicrosoft.Extensions.Logging;
using Newtonsoft.Json;
using Polytopia.Data;
using PolytopiaBackendBase;
using Object = Il2CppSystem.Object;

namespace PolytopiaB2;

public class Patcher
{
    public static readonly Harmony harmony = new Harmony("polytopiaB2");

    public static void PatchAll()
    {
        harmony.PatchAll();

        BuildConfigHelper.GetSelectedBuildConfig().buildServerURL = BuildServerURL.Custom;
        BuildConfigHelper.GetSelectedBuildConfig().customServerURL = "http://localhost:5051";
    }

    [HarmonyPatch(
        typeof(PolytopiaDataManager),
        nameof(PolytopiaDataManager.GetGameLogicData),
        new[] { typeof(int), typeof(bool) }
    )]
    static class PolytopiaDataManager_GetGameLogicData_Patch
    {
        static bool Prefix(ref int version, ref bool force, ref GameLogicData __result)
        {
            //Plugin.Log.LogInfo($"Dumping game logic datas");
            //Dumper.DumpAll();

            return true;
        }
    }
}
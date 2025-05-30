using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMicrosoft.Extensions.Logging;
using Newtonsoft.Json;
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
}
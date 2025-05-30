using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace PolytopiaB2;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public override void Load()
    {
        Patcher.PatchAll();

        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

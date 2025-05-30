using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Object = Il2CppSystem.Object;

namespace PolytopiaB2;

public class LoggerPatcher
{
    [HarmonyPatch(typeof(Log), nameof(Log.Info),
        new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public static class LogInfoParamsArrayPatch
    {
        public static bool Prefix(string format, Il2CppReferenceArray<Il2CppSystem.Object> args)
        {
            LogHelper.Log(format, args, "Info");
            return false;
        }
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Warning),
        new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public static class LogWaningParamsArrayPatch
    {
        public static bool Prefix(string format, Il2CppReferenceArray<Il2CppSystem.Object> args)
        {
            LogHelper.Log(format, args, "Warning");
            return false;
        }
    }


    [HarmonyPatch(typeof(Log), nameof(Log.Error),
        new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public static class LogErrorParamsArrayPatch
    {
        public static bool Prefix(string format, Il2CppReferenceArray<Il2CppSystem.Object> args)
        {
            LogHelper.Log(format, args, "Error");
            return false;
        }
    }


    [HarmonyPatch(typeof(Log), nameof(Log.Verbose),
        new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public static class LogVerboseParamsArrayPatch
    {
        public static bool Prefix(string format, Il2CppReferenceArray<Il2CppSystem.Object> args)
        {
            LogHelper.Log(format, args, "Verbose");
            return false;
        }
    }


    [HarmonyPatch(typeof(Log), nameof(Log.Spam),
        new Type[] { typeof(string), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    public static class LogSpamParamsArrayPatch
    {
        public static bool Prefix(string format, Il2CppReferenceArray<Il2CppSystem.Object> args)
        {
            LogHelper.Log(format, args, "Spam");
            return false;
        }
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Exception), typeof(Il2CppSystem.Exception))]
    public static class LogExceptionsPatch
    {
        public static bool Prefix(Il2CppSystem.Exception exception)
        {
            Plugin.Log.LogError($"[Log.Exception] {exception.ToString()}");
            return false;
        }
        
    }

    private static class LogHelper
    {
        public static void Log(string format, Il2CppReferenceArray<Il2CppSystem.Object> args, string type)
        {
            try
            {
                string message;
                if (args != null && args.Length > 0)
                {
                    object[] regularArgs = new object[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] != null)
                        {
                            regularArgs[i] = args[i].ToString();
                        }
                        else
                        {
                            regularArgs[i] = "null";
                        }
                    }

                    message = string.Format(format, regularArgs);
                }
                else
                {
                    message = format;
                }

                string formattedMessage = $"[Log.{type}] {message}";
            
                switch (type)
                {
                    case "Info":
                        Plugin.Log.LogInfo(formattedMessage);
                        break;
                    case "Warning":
                        Plugin.Log.LogWarning(formattedMessage);
                        break;
                    case "Error":
                        Plugin.Log.LogError(formattedMessage);
                        break;
                    case "Verbose":
                        Plugin.Log.LogDebug(formattedMessage);
                        break;
                    case "Spam":
                        Plugin.Log.LogMessage(formattedMessage);
                        break;
                    default:
                        Plugin.Log.LogInfo(formattedMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[Log.{type}] {format} (Error formatting: {ex.Message})");
            }
        }
    }
}
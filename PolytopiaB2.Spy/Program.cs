using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Challengermode.Matchmaking;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.Enums;
using PolytopiaBackendBase.Game.ViewModels;

AssemblyRedirector.Initialize();

Console.WriteLine("Hello, World!");

BuildConfigHelper.GetSelectedBuildConfig().buildServerURL = BuildServerURL.Custom;
BuildConfigHelper.GetSelectedBuildConfig().customServerURL = "http://localhost:5051";

TimeHook.Initialize();

PolytopiaDataManager.provider = new MyProvider();
        
var harmony = new Harmony("spy");
harmony.PatchAll();

try
{
    PolytopiaBackendAdapter.Instance.HubConnection = new HubConnectionBuilder().WithUrl(new Uri("http://localhost:5051/gamehub"), (Action<HttpConnectionOptions>) (options =>
    {
        options.SkipNegotiation = false;
    })).ConfigureLogging((Action<ILoggingBuilder>) (options =>
    {
        options.SetMinimumLevel(PolytopiaBackendAdapter.Instance.SignalRLogLevel);
    })).WithAutomaticReconnect().Build();
    
    
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<string>(new Func<string, Task>(PolytopiaBackendAdapter.Instance.OnNotify));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<CommandArrayViewModel>(new Func<CommandArrayViewModel, Task>(PolytopiaBackendAdapter.Instance.OnCommand));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<GameStateViewModel, StateUpdateReason>(new Func<GameStateViewModel, StateUpdateReason, Task>(PolytopiaBackendAdapter.Instance.OnGameStateUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<GameSummaryViewModel, StateUpdateReason>(new Func<GameSummaryViewModel, StateUpdateReason, Task>(PolytopiaBackendAdapter.Instance.OnGameSummaryUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid, GameDeleteReason>(new Func<Guid, GameDeleteReason, Task>(PolytopiaBackendAdapter.Instance.OnGameDeleted));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<long, MatchmakingUpdateReason>(new Func<long, MatchmakingUpdateReason, Task>(PolytopiaBackendAdapter.Instance.OnMatchmakingGameUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<PlayerResignedViewModel>(new Func<PlayerResignedViewModel, Task>(PolytopiaBackendAdapter.Instance.OnPlayerResigned));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<PlayerSkippedViewModel>(new Func<PlayerSkippedViewModel, Task>(PolytopiaBackendAdapter.Instance.OnPlayerSkipped));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid>(new Func<Guid, Task>(PolytopiaBackendAdapter.Instance.OnInvitation));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<LobbyGameViewModel>(new Func<LobbyGameViewModel, Task>(PolytopiaBackendAdapter.Instance.OnLobbyInvitation));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid>(new Func<Guid, Task>(PolytopiaBackendAdapter.Instance.OnGameReadyToStart));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<List<PolytopiaFriendViewModel>>(new Func<List<PolytopiaFriendViewModel>, Task>(PolytopiaBackendAdapter.Instance.OnFriendsUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid, PlayerStatus>(new Func<Guid, PlayerStatus, Task>(PolytopiaBackendAdapter.Instance.OnPlayerStatusUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<LobbyGameViewModel>(new Func<LobbyGameViewModel, Task>(PolytopiaBackendAdapter.Instance.OnLobbyUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<PolytopiaUserViewModel>(new Func<PolytopiaUserViewModel, Task>(PolytopiaBackendAdapter.Instance.OnUserUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid>(new Func<Guid, Task>(PolytopiaBackendAdapter.Instance.OnFriendRequestReceived));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<Guid>(new Func<Guid, Task>(PolytopiaBackendAdapter.Instance.OnFriendRequestAccepted));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<int>(new Func<int, Task>(PolytopiaBackendAdapter.Instance.OnActionableGamesUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<TournamentViewModel, TournamentUpdateReason>(new Func<TournamentViewModel, TournamentUpdateReason, Task>(PolytopiaBackendAdapter.Instance.OnTournamentUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<TournamentPersonalUpdatedViewModel>(new Func<TournamentPersonalUpdatedViewModel, Task>(PolytopiaBackendAdapter.Instance.OnTournamentPersonalUpdated));
    PolytopiaBackendAdapter.Instance.HubConnection.BindMethod<TournamentMatchmakingQueueViewModel, TournamentMatchmakingUpdateReason>(new Func<TournamentMatchmakingQueueViewModel, TournamentMatchmakingUpdateReason, Task>(PolytopiaBackendAdapter.Instance.OnTournamentMatchmakingUpdated));
    
    await PolytopiaBackendAdapter.Instance.HubConnection.StartAsync();
    //await PolytopiaBackendAdapter.Instance.SubscribeOnConnect();
    PolytopiaBackendAdapter.Instance.HubConnection.Reconnecting += new Func<Exception, Task>(PolytopiaBackendAdapter.Instance.OnReconnecting);
    //PolytopiaBackendAdapter.Instance.HubConnection.Reconnected += new Func<string, Task>(PolytopiaBackendAdapter.Instance.OnReconnected);
    PolytopiaBackendAdapter.Instance.HubConnection.Closed += new Func<Exception, Task>(PolytopiaBackendAdapter.Instance.OnClosed);
}
catch (Exception ex)
{
    throw ex;
}

var client = new RemoteClient();

client.OpenSession(Guid.Parse("597f332b-281c-464c-a8e7-6a79f4496360")).Wait();

public static class AssemblyRedirector
{
    private static Assembly CustomResolveEventHandler(object sender, ResolveEventArgs args)
    {
        // Check if the requested assembly is the one we want to replace
        if (args.Name.Contains("SignalRNewtonsoftAotProtocol"))
        {
            Console.WriteLine($"Intercepting assembly load: {args.Name}");
            
            // Return your custom implementation assembly instead
            return typeof(PolytopiaB2.Spy.ModifiedProtocol.NewtonsoftJsonProtocolDependencyInjectionExtensions)
                .Assembly;
        }
        
        // For other assemblies, return null to use default resolution
        return null;
    }
    
    public static void Initialize()
    {
        // Register the event handler for assembly resolution
        AppDomain.CurrentDomain.AssemblyResolve += CustomResolveEventHandler;
        Console.WriteLine("Assembly redirector initialized");
    }
}


class MyLogger : Logger
{
    public override void LogSpam(string format, object[] args)
    {
        Console.WriteLine(format, args);
    }

    public override void LogVerbose(string format, object[] args)
    {
        Console.WriteLine(format, args);
    }

    public override void LogInfo(string format, object[] args)
    {
        Console.WriteLine(format, args);
    }

    public override void LogWarning(string format, object[] args)
    {
        Console.WriteLine(format, args);
    }

    public override void LogError(string format, object[] args)
    {
        Console.WriteLine(format, args);
    }

    public override void LogException(Exception exception)
    {
        Console.WriteLine(exception);
    }
}

class MyProvider : IPolytopiaDataProvider
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

[HarmonyPatch(typeof(Config), "GetBackendURI")]
public static class GetBackendURIPatch
{
    static bool Prefix(ref string __result)
    {
        __result = "http://localhost:5051"; 
        return false;
    }
}

[HarmonyPatch(typeof(BackendAdapter), "HasValidAccessToken")]
public static class HasValidAccessTokenPatch
{
    static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(GameManager), "GetRemoteGameDataManager")]
public static class GetRemoteGameDataManagerPatch
{
    static bool Prefix(ref RemoteGameDataManager __result)
    {
        __result = new RemoteGameDataManager();
        __result.Initialize();
        return false;
    }
}

public static class TimeHook
{
    [DllImport("kernel32.dll")]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect,
        out uint lpflOldProtect);

    private const uint PAGE_EXECUTE_READWRITE = 0x40;

    private static int _frameCount = 0;

    public static unsafe void Initialize()
    {
        try
        {
            MethodInfo methodInfo = typeof(UnityEngine.Time)
                .GetProperty("frameCount", BindingFlags.Public | BindingFlags.Static)
                .GetGetMethod();

            RuntimeMethodHandle handle = methodInfo.MethodHandle;

            IntPtr methodPtr = handle.GetFunctionPointer();

            uint oldProtect;
            VirtualProtect(methodPtr, (UIntPtr)8, PAGE_EXECUTE_READWRITE, out oldProtect);

            IntPtr replacementMethod = typeof(TimeHook)
                .GetMethod("GetCustomFrameCount", BindingFlags.Public | BindingFlags.Static)
                .MethodHandle.GetFunctionPointer();

            byte* dst = (byte*)methodPtr.ToPointer();

            *dst = 0xE9;
            *(int*)(dst + 1) = (int)(replacementMethod.ToInt64() - methodPtr.ToInt64() - 5);

            VirtualProtect(methodPtr, (UIntPtr)8, oldProtect, out oldProtect);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to hook Time.frameCount: {ex}");
        }
    }

    public static int GetCustomFrameCount()
    {
        return _frameCount++;
    }
}

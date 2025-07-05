using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using DystopiaShared;
using DystopiaShared.SharedModels;


namespace Dystopia.Bridge;

public class DystopiaBridge : IDystopiaCastle
{
    private static bool _shouldAlwaysUseManaged;
    private static readonly string BasePath = AppContext.BaseDirectory;

    private static readonly string DataFolder = Path.Combine(BasePath, "Data");
    private static readonly string GameLogicDataFolder = Path.Combine(DataFolder, "GameLogicData");

    private static readonly string NativeFolder = Path.Combine(BasePath, "Native");
    private static readonly string MagicFolder = Path.Combine(NativeFolder, "Magic");


    private static readonly string SharedLibraryExtension =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" : ".so";

    private static bool il2cppLoaded = false;
    private static IDystopiaCastle nativeCastle = null;

    private IDystopiaCastle GetFittingCastle(byte[] serializedGameState)
    {
        SerializationHelpers.PeekVersion(serializedGameState, out var version);
        return GetFittingCastle(version);
    }

    private IDystopiaCastle GetFittingCastle(SharedLobbyGameViewModel lobby)
    {
        var lowestVersionPlayer = lobby.Participators.Select(p => p.GameVersion.Min(p => p.GameVersion)).Min(p => p);
        return GetFittingCastle(lowestVersionPlayer);
    }

    private IDystopiaCastle GetFittingCastle(int version)
    {
        if (version <= VersionManager.GameVersion || _shouldAlwaysUseManaged) // MANAGED
        {
            return new DystopiaWhiteCastle();
        }
        else // NATIVE
        {
            if(!il2cppLoaded) throw new Exception("Il2Cpp not loaded");

            return nativeCastle;
        }
    }

    public static void InitIl2Cpp(bool shouldAlwaysUseManaged)
    {
        _shouldAlwaysUseManaged = shouldAlwaysUseManaged;
        if (shouldAlwaysUseManaged)
        {
            return;
        }

        var pluginDll = Path.Combine(MagicFolder, "DystopiaMagic.dll");

        var loadCtx = new PluginLoadContext(pluginDll);

        var pluginAsm = loadCtx.LoadFromAssemblyPath(pluginDll);


        var loaderType = pluginAsm.GetType(
            "DystopiaMagic.GameAssemblyLoader",
            throwOnError: true
        );
        var loader = (object)Activator.CreateInstance(loaderType)!;


        var m = loaderType.GetMethods();
        var mi1 = loaderType.GetMethods()[1];
        mi1.Invoke(loader, new object?[] { Path.Combine(NativeFolder, "GameAssembly" + SharedLibraryExtension) });
        var mi2 = loaderType.GetMethods()[2];
        var vsa = mi2.Invoke(loader,
            new object[]
            {
                GameLogicDataFolder
            });

        var castleType = pluginAsm.GetType(
            "DystopiaMagic.DystopiaBlackCastle",
            throwOnError: true
        );

        il2cppLoaded = true;

        nativeCastle = (IDystopiaCastle) Activator.CreateInstance(castleType)!;
    }

    public string GetVersion() //TODO
    {
        return "Unsupported";
    }

    public byte[] CreateGame(SharedLobbyGameViewModel lobby)
    {
        var castle = GetFittingCastle(lobby);

        Console.WriteLine(castle.GetVersion());

        var gs = castle.CreateGame(lobby);

        return gs;
    }

    public byte[] Update(byte[] serializedGameState)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.Update(serializedGameState);
    }

    public string GetGameSettingsJson(byte[] serializedGameState)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.GetGameSettingsJson(serializedGameState);
    }

    public byte[]? Resign(byte[] serializedGameState, string senderId)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.Resign(serializedGameState, senderId);
    }

    public bool SendCommand(byte[] serializedCommand, byte[] serializedGameState, out byte[] newGameState,
        out byte[][] newCommands)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.SendCommand(serializedCommand, serializedGameState, out newGameState, out newCommands);
    }

    public bool IsPlayerInGame(string playerId, byte[] serializedGameState)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.IsPlayerInGame(playerId, serializedGameState);
    }

    public byte[] GetSummary(byte[] serializedGameState)
    {
        var castle = GetFittingCastle(serializedGameState);

        return castle.GetSummary(serializedGameState);
    }
}

class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
        : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        if (name.Name == "DystopiaShared")
            return Assembly.Load(name);

        string? path = _resolver.ResolveAssemblyToPath(name);
        return path != null
            ? LoadFromAssemblyPath(path)
            : null;
    }
}
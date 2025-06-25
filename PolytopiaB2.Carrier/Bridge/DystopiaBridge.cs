using System.Reflection;
using System.Runtime.Loader;
using DystopiaShared;
using DystopiaShared.SharedModels;


namespace PolytopiaB2.Carrier.Bridge;

public class DystopiaBridge : IDystopiaCastle
{
    private const string PluginFolder =
        @"C:\Users\Juli\Desktop\source\polydystopia\DystopiaMagic\DystopiaMagic\bin\Debug\net6.0";

    private static bool il2cppLoaded = false;
    private IDystopiaCastle nativeCastle = null;

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
        if (version <= VersionManager.GameVersion) // MANAGED
        {
            return new DystopiaWhiteCastle();
        }
        else // NATIVE
        {
            if(!il2cppLoaded) nativeCastle = InitIl2Cpp();

            return nativeCastle;
        }
    }

    private IDystopiaCastle InitIl2Cpp()
    {
        string pluginDll = Path.Combine(PluginFolder, "DystopiaMagic.dll");

        var loadCtx = new PluginLoadContext(pluginDll);

        var pluginAsm = loadCtx.LoadFromAssemblyPath(pluginDll);


        var loaderType = pluginAsm.GetType(
            "DystopiaMagic.GameAssemblyLoader",
            throwOnError: true
        );
        var loader = (object)Activator.CreateInstance(loaderType)!;


        var m = loaderType.GetMethods();
        var mi1 = loaderType.GetMethods()[1];
        mi1.Invoke(loader, new[] { "GameAssembly.dll" });
        var mi2 = loaderType.GetMethods()[2];
        var vsa = mi2.Invoke(loader,
            new object[]
            {
                @"C:\Users\Juli\Desktop\source\polydystopia\DystopiaMagic\DystopiaMagic\bin\Debug\net6.0\GameLogicData"
            });

        var castleType = pluginAsm.GetType(
            "DystopiaMagic.DystopiaBlackCastle",
            throwOnError: true
        );

        il2cppLoaded = true;

        return (IDystopiaCastle)Activator.CreateInstance(castleType)!;
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
}

class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
        : base(isCollectible: true)
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
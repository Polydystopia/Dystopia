using System.Reflection;
using System.Runtime.Loader;
using DystopiaMagic;
using PolytopiaBackendBase.Game;


namespace PolytopiaB2.Carrier.Bridge;

public class DystopiaBridge : IDystopiaCastle
{
    private const string PluginFolder =
        @"C:\Users\Juli\Desktop\source\polydystopia\DystopiaMagic\DystopiaMagic\bin\Debug\net6.0";

    public GameState CreateGame(LobbyGameViewModel lobby)
    {
        string pluginDll = Path.Combine(PluginFolder, "DystopiaMagic.dll");

        var loadCtx = new PluginLoadContext(pluginDll);

        var pluginAsm = loadCtx.LoadFromAssemblyPath(pluginDll);


        var loaderType = pluginAsm.GetType(
            "DystopiaMagic.GameAssemblyLoader",
            throwOnError: true
        );
        var loader = (object) Activator.CreateInstance(loaderType)!;

        var m = loaderType.GetMethods();
        var mi1 = loaderType.GetMethods()[1];
        mi1.Invoke(loader, new []{"GameAssembly.dll"});
        var mi2 = loaderType.GetMethods()[2];
        var vsa = mi2.Invoke(loader, new object[] { @"C:\Users\Juli\Desktop\source\polydystopia\DystopiaMagic\DystopiaMagic\bin\Debug\net6.0\GameLogicData" });


        var castleType = pluginAsm.GetType(
            "DystopiaMagic.DystopiaBlackCastle",
            throwOnError: true
        );
        var castle = (object) Activator.CreateInstance(castleType)!;
        var mi = castleType.GetMethods()[1];
        if (mi == null)
            throw new MissingMethodException("DystopiaBlackCastle.CreateGame(LobbyGameViewModel) not found");

        var xxx = mi.Invoke(castle, new object[] { "hi du", 6868 });

        var result = mi.Invoke(castle, new object[] { lobby });
        if (result is not GameState gs)
            throw new InvalidCastException($"Expected CreateGame to return GameState, got {result?.GetType().FullName}");


        // 5) (Optional) unload when done
        loadCtx.Unload();

        return gs;
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
        // If it's the contract assembly, return the host's already‐loaded copy:
        if (name.Name == "PolytopiaBackendBase.Game")
            return Assembly.Load(name);

        // Otherwise, resolve from the plugin folder:
        string? path = _resolver.ResolveAssemblyToPath(name);
        return path != null
            ? LoadFromAssemblyPath(path)
            : null;
    }
}
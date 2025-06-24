using System.Reflection;
using System.Runtime.Loader;
using DystopiaMagic;
using DystopiaShared;
using PolytopiaBackendBase.Game;


namespace PolytopiaB2.Carrier.Bridge;

public class DystopiaBridge
{
    private const string PluginFolder =
        @"C:\Users\Juli\Desktop\source\polydystopia\DystopiaMagic\DystopiaMagic\bin\Debug\net6.0";

    public byte[] CreateGame(LobbyGameViewModel lobby)
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
        var castle = (IDystopiaCastle) Activator.CreateInstance(castleType)!;

        Console.WriteLine(castle.GetVersion());

        var gs = castle.CreateGame(lobby);

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
        if (name.Name == "DystopiaShared" || name.Name == "PolytopiaBackendBase")
            return Assembly.Load(name);

        string? path = _resolver.ResolveAssemblyToPath(name);
        return path != null
            ? LoadFromAssemblyPath(path)
            : null;
    }
}
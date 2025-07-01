using DystopiaMagic;
using DystopiaShared;
using DystopiaShared.SharedModels;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

Console.WriteLine("Hello World!");

try
{
    var loader = new GameAssemblyLoader();
    loader.Init();
    loader.LoadGameStates();



    Wrapper.PerformCommands(new GameState(), new Il2CppSystem.Collections.Generic.List<CommandBase>(), out var ff, out var qf);

    GameStateUtils.PerformCommands(new GameState(), new Il2CppSystem.Collections.Generic.List<CommandBase>(), out var lisst, out var list1);

    GameStateUtils.GetRandomPickableTribe(1, new GameSettings(),
        new Il2CppSystem.Collections.Generic.List<PlayerState>());

    GameStateUtils.ExecuteCommands(null, new Il2CppSystem.Collections.Generic.List<CommandBase>(), out var list2,
        out var events2, out var error);

    GameStateUtils.PerformCommands(null, null, out var list,
        out var events);

    var castle = new DystopiaBlackCastle();
    Console.WriteLine(castle.GetVersion());

    var sharedLobby = new SharedLobbyGameViewModel();

    var x = castle.CreateGame(sharedLobby);

    Console.WriteLine("yes gurl");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

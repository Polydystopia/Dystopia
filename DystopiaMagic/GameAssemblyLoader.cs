using System.Runtime.InteropServices;
using BepInEx.Unity.IL2CPP.Hook;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Startup;
using Polytopia.Data;

namespace DystopiaMagic;

public class GameAssemblyLoader
{
    [DllImport("GameAssembly", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void il2cpp_init(IntPtr domain_name);

    private bool IsInitialized { get; set; }
    private bool IsLoaded { get; set; }

    public void Init(string assemblyPath = @"GameAssembly.dll")
    {
        if(IsInitialized) return;
        
        Console.WriteLine($"loading assembly {assemblyPath} as GameAssembly");

        string domainName = "dystopiamagic";
        IntPtr domainNamePtr = Marshal.StringToHGlobalAnsi(domainName);

        try
        {
            IntPtr libHandle = NativeLibrary.Load(assemblyPath);

            var runtimeConfig = new RuntimeConfiguration()
            {
                UnityVersion = new Version(2021, 3, 0),
                DetourProvider = new MyDetourProvider()
            };

            Il2CppInteropRuntime.Create(runtimeConfig).Start();


            il2cpp_init(domainNamePtr);

            Console.WriteLine("IL2CPP initialized successfully.");

            IsInitialized = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            Marshal.FreeHGlobal(domainNamePtr);
        }
    }

    public void LoadGameStates(string gameLogicDataPath = @"GameLogicData")
    {
        if(!IsInitialized || IsLoaded) return;

        for (int i = 1; i <= Directory.GetFiles(gameLogicDataPath).Length; i++)
        {
            var data = new GameLogicData();
            data.Parse(File.ReadAllText($@"{gameLogicDataPath}\{i}.txt"));

            PolytopiaDataManager.gameLogicDatas.Add(i, data);
        }

        IsLoaded = true;
    }
}

public class MyDetourProvider : IDetourProvider
{
    public IDetour Create<TDelegate>(nint original, TDelegate target) where TDelegate : Delegate
    {
        var detour = INativeDetour.Create(original, target);
        var myDetour = new Il2CppInteropDetour(detour);

        return myDetour;
    }
}

internal class Il2CppInteropDetour : IDetour
{
    private readonly INativeDetour detour;

    public Il2CppInteropDetour(INativeDetour detour)
    {
        this.detour = detour;
    }

    public void Dispose() => detour.Dispose();

    public void Apply() => detour.Apply();

    public T GenerateTrampoline<T>() where T : Delegate => detour.GenerateTrampoline<T>();

    public nint Target => detour.OriginalMethodPtr;
    public nint Detour => detour.DetourMethodPtr;
    public nint OriginalTrampoline => detour.TrampolinePtr;
}
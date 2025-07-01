using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Il2CppException = Il2CppInterop.Runtime.Il2CppException;

namespace DystopiaMagic;

public class Wrapper
{
    public static unsafe CommandResult PerformCommands(
        GameState gameState,
        Il2CppSystem.Collections.Generic.List<CommandBase> commands,
        out Il2CppSystem.Collections.Generic.List<CommandBase> executedCommands,
        out Il2CppSystem.Collections.Generic.List<CommandResultEvent> events)
    {
        // prepare real locals for the two out‐args
        IntPtr outCmdsPtr = IntPtr.Zero;
        IntPtr outEventsPtr = IntPtr.Zero;

        // build the argument buffer
        IntPtr* args = stackalloc IntPtr[4];
        args[0] = IL2CPP.Il2CppObjectBaseToPtr(gameState);
        args[1] = IL2CPP.Il2CppObjectBaseToPtr(commands);

        // pass the ADDRESS of our locals, not zero
        args[2] = (IntPtr)(&outCmdsPtr);
        args[3] = (IntPtr)(&outEventsPtr);

        // invoke the native method
        IntPtr exception = IntPtr.Zero;
        IntPtr resultPtr = IL2CPP.il2cpp_runtime_invoke(
            GetPerformCommandsPtr(),
            IntPtr.Zero,
            (void**)args,
            ref exception
        );
        Il2CppException.RaiseExceptionIfNecessary(exception);

        // wrap the two out‐lists
        executedCommands = outCmdsPtr == IntPtr.Zero
            ? null
            : new Il2CppSystem.Collections.Generic.List<CommandBase>(outCmdsPtr);

        events = outEventsPtr == IntPtr.Zero
            ? null
            : new Il2CppSystem.Collections.Generic.List<CommandResultEvent>(outEventsPtr);

        // wrap and return the CommandResult
        return resultPtr == IntPtr.Zero
            ? null
            : Il2CppObjectPool.Get<CommandResult>(resultPtr);
    }

    public static IntPtr GetPerformCommandsPtr()
    {
        // make sure the GameStateUtils type initializer has run
        RuntimeHelpers.RunClassConstructor(typeof(GameStateUtils).TypeHandle);

        // look up the field
        var f = typeof(GameStateUtils).GetField(
            "NativeMethodInfoPtr_PerformCommands_Public_Static_CommandResult_GameState_List_1_CommandBase_byref_List_1_CommandBase_byref_List_1_CommandResultEvent_0",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        if (f == null)
            throw new InvalidOperationException("Field not found — did your binding generator change its name?");

        return (IntPtr)f.GetValue(null);
    }
}
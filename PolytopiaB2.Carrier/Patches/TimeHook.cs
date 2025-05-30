using System.Reflection;
using System.Runtime.InteropServices;

namespace PolytopiaB2.Carrier.Patches;

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

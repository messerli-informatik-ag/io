#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#else
using System.Runtime.InteropServices;
#endif

namespace Messerli.IO;

internal static class OperatingSystem
{
#if NET7_0_OR_GREATER
    [SupportedOSPlatformGuard("windows")]
    public static bool IsWindows() => System.OperatingSystem.IsWindows();
#else
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

#if NET7_0_OR_GREATER
    [SupportedOSPlatformGuard("macos")]
    public static bool IsMacOS() => System.OperatingSystem.IsMacOS();
#else
    public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
}

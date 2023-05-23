using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Messerli.IO;

public static class ConsoleUtility
{
    /// <summary><para>Determines whether the current process is the last one attached to the console.
    /// If this process is the last one, the console will be destroyed when the process exists.</para>
    /// <para>This function is useful for determining if the console should
    /// be kept open (e.g. by using <see cref="Console.ReadLine"/>).</para></summary>
    /// <example><code><![CDATA[
    /// using static Messerli.IO.ConsoleUtility;
    ///
    /// if (OperatingSystem.IsWindows() && IsLastProcessAttachedToConsole())
    /// {
    ///     Console.WriteLine("Press Enter to continue...");
    ///     Console.ReadLine();
    /// }
    /// ]]></code></example>
    /// <remarks>This API is only supported on windows only.</remarks>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static bool IsLastProcessAttachedToConsole()
        => Windows.IsLastProcessAttachedToConsole();

#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    private static class Windows
    {
        // Inspiration: https://devblogs.microsoft.com/oldnewthing/20160125-00/?p=92922
        public static bool IsLastProcessAttachedToConsole()
        {
            var processList = new uint[1];
            var processCount = GetConsoleProcessList(processList, (uint)processList.Length);
            return processCount == 1;
        }

        /// <summary>Retrieves a list of the processes attached to the current console.</summary>
        /// <param name="processList">A pointer to a buffer that receives an array of process identifiers upon success. The buffer must have space to receive at least 1 returned process id.</param>
        /// <param name="processCount">The maximum number of process identifiers that can be stored in the <paramref name="processList" /> buffer. This must be greater than 0.</param>
        /// <returns>[...] If the buffer is too small to hold all the valid process identifiers, the return value is the required number of array elements. [...].</returns>
        /// <footer>See <a href="https://docs.microsoft.com/en-us/windows/console/getconsoleprocesslist">GetConsoleProcessList</a> in Microsoft's documentation.</footer>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern uint GetConsoleProcessList(uint[] processList, uint processCount);
    }
}

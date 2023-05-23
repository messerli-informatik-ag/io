using static Messerli.IO.ConsoleUtility;

if (OperatingSystem.IsWindows() && IsLastProcessAttachedToConsole())
{
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}

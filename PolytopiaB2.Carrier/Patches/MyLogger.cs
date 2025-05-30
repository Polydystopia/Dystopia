namespace PolytopiaB2.Carrier.Patches;

public class MyLogger : Logger
{
    public override void LogSpam(string format, object[] args)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("[Spam] " + format, args);
        Console.ResetColor();
    }

    public override void LogVerbose(string format, object[] args)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("[Verbose] " + format, args);
        Console.ResetColor();
    }

    public override void LogInfo(string format, object[] args)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("[Info] " + format, args);
        Console.ResetColor();
    }

    public override void LogWarning(string format, object[] args)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[Warning] " + format, args);
        Console.ResetColor();
    }

    public override void LogError(string format, object[] args)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Error] " + format, args);
        Console.ResetColor();
    }

    public override void LogException(Exception exception)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("[Exception] " + exception);
        Console.ResetColor();
    }
}
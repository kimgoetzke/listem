namespace Listem.API.Utilities;

public static class Logger
{
    private const string Prefix = "[XXX]";

    public static void Log(string message)
    {
        Console.WriteLine($"{Prefix} {message}");
    }
}

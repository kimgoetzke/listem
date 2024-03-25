using System.Diagnostics;

namespace Listem.Mobile.Utilities;

/**
 * TODO: Remove this temporary class and use a proper logger.
 */
public static class Logger
{
    private const string LoggerTag = "Listem";
    private const string Prefix = "[XXX]";

    public static void Log(string message)
    {
#if __ANDROID__
        Android.Util.Log.Info(LoggerTag, $"{Prefix} {message}");
#elif DEBUG
        Console.WriteLine($"{Prefix} [DEPRECATED] {message}");
        Trace.WriteLine($"{Prefix} [DEPRECATED] {message}");
#endif
    }
}

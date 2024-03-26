using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Utilities;

public static class LoggerProvider
{
  internal static ILoggerFactory LoggerFactory { private get; set; } = null!;

  internal static ILogger CreateLogger(string name) => LoggerFactory.CreateLogger(name);
}

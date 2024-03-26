using Microsoft.Extensions.Logging;

namespace Listem.Mobile.Utilities;

public static class Logging
{
  public static ILoggerFactory LoggerFactory { private get; set; } = null!;

  public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

  internal static ILogger CreateLogger(string name) => LoggerFactory.CreateLogger(name);
}

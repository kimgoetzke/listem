namespace Microsoft.Extensions.Logging;

public static class LoggerExtensions
{
  public static void Debug(this ILogger logger, string? message, params object?[] args)
  {
    logger.Log(LogLevel.Debug, message, args);
  }

  public static void Info(this ILogger logger, string? message, params object?[] args)
  {
    logger.Log(LogLevel.Information, message, args);
  }

  public static void Info(
    this ILogger logger,
    Exception? exception,
    string? message,
    params object?[] args
  )
  {
    logger.Log(LogLevel.Information, exception, message, args);
  }

  public static void Warn(this ILogger logger, string? message, params object?[] args)
  {
    logger.Log(LogLevel.Warning, message, args);
  }

  public static void Error(this ILogger logger, string? message, params object?[] args)
  {
    logger.Log(LogLevel.Error, message, args);
  }
}

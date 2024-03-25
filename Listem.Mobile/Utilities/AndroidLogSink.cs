using Serilog.Core;
using Serilog.Events;

namespace Listem.Mobile.Utilities;

public class AndroidLogSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
#if __ANDROID__
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                Android.Util.Log.Debug(Constants.LoggerTag, message);
                break;
            case LogEventLevel.Verbose:
                Android.Util.Log.Verbose(Constants.LoggerTag, message);
                break;
            case LogEventLevel.Warning:
                Android.Util.Log.Warn(Constants.LoggerTag, message);
                break;
            case LogEventLevel.Error:
                Android.Util.Log.Error(Constants.LoggerTag, message);
                break;
            case LogEventLevel.Fatal:
                Android.Util.Log.Error(Constants.LoggerTag, message);
                break;
            case LogEventLevel.Information:
            default:
                Android.Util.Log.Info(Constants.LoggerTag, message);
                break;
        }
#endif
    }
}

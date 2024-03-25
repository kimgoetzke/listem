using Serilog.Core;
using Serilog.Events;

namespace Listem.Mobile.Utilities;

public class AndroidLogSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
#if __ANDROID__
        Android.Util.Log.Info(
            Listem.Mobile.Constants.LoggerTag,
            $"{Listem.Mobile.Constants.LoggerPrefix} {message}"
        );
#endif
    }
}

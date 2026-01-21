using Microsoft.Extensions.Logging;

namespace GICutscenes.Events;

public static class CommonEvents
{
    /// <summary>
    /// Stream ended too early, expected {Size}, read {Read}
    /// </summary>
    internal static readonly Action<ILogger, int, int, Exception?> LogStreamEndedTooEarly = LoggerMessage.Define<int, int>(
        LogLevel.Error,
        StreamEndedTooEarly,
        "Stream ended too early, expected {Size}, read {Read}");
    public static readonly EventId StreamEndedTooEarly = new(9000, $"{nameof(GICutscenes)}_Common_{nameof(StreamEndedTooEarly)}");
}
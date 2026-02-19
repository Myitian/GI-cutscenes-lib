using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace GICutscenes.CLI;

sealed class ConsoleOutLogger(LogLevel minLevel = LogLevel.Information) : ILogger
{
    static ConsoleOutLogger()
    {
        Task.Run(Process);
    }
    private static readonly BlockingCollection<LogEntry> _queue = new(8192);
    private static void Process()
    {
        AppDomain.CurrentDomain.ProcessExit += CompleteAdding;
        Span<char> buffer = stackalloc char[32];
        while (_queue.TryTake(out LogEntry entry, Timeout.Infinite))
        {
            Console.Out.Write('[');
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.BackgroundColor = ConsoleColor.Black;
            DateTime.Now.TryFormat(buffer, out int count, "yyyy/MM/dd HH:mm:ss.fff");
            Console.Out.Write(buffer[..count]);
            Console.ResetColor();
            Console.Out.Write("] [");
            ConsoleColorPair color = GetLogLevelConsoleColors(entry.LogLevel);
            Console.ForegroundColor = color.Foreground;
            Console.BackgroundColor = color.Background;
            Console.Out.Write(GetLogLevelString(entry.LogLevel));
            Console.ResetColor();
            Console.Out.Write("]: ");
            if (entry.Message is not null)
                Console.Out.WriteLine(entry.Message);
            if (entry.Exception is not null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Out.WriteLine(entry.Exception);
            }
            Console.ResetColor();
        }
    }
    private static void CompleteAdding(object? sender, EventArgs e)
    {
        _queue.CompleteAdding();
        AppDomain.CurrentDomain.ProcessExit -= CompleteAdding;
    }
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        string? message = formatter?.Invoke(state, exception);
        if (exception is null && message is null)
            return;
        _queue.TryAdd(new(logLevel, message, exception), Timeout.Infinite);
    }
    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= minLevel;
    public IDisposable BeginScope<TState>(TState state)
        => NullLogger.Instance.BeginScope(state);
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "FATAL",
            _ => logLevel.ToString()
        };
    }
    private static ConsoleColorPair GetLogLevelConsoleColors(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => new(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new(ConsoleColor.Green, ConsoleColor.Black),
            LogLevel.Warning => new(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new(ConsoleColor.Red, ConsoleColor.Black),
            LogLevel.Critical => new(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new(ConsoleColor.Gray, ConsoleColor.Black)
        };
    }
    private readonly struct ConsoleColorPair(ConsoleColor foreground, ConsoleColor background)
    {
        public readonly ConsoleColor Foreground = foreground;
        public readonly ConsoleColor Background = background;
    }
    private readonly struct LogEntry(LogLevel logLevel, string? message, Exception? exception)
    {
        public readonly DateTime DateTime = DateTime.Now;
        public readonly string? Message = message;
        public readonly Exception? Exception = exception;
        public readonly LogLevel LogLevel = logLevel;
    }
}
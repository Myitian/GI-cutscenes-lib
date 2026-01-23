using Microsoft.Extensions.Logging;
using SimpleArgs;

namespace GICutscenes.CLI;

static class Extensions
{
    public static string GetStringArgument(this ArgParser argx, string name)
    {
        return argx.TryGetString(name, out string? value) ? value : throw new KeyNotFoundException();
    }
    public static void InvokeLog(
        this Action<ILogger, Exception?> action,
        ILogger? logger,
        Exception? exception = null)
    {
        if (logger is not null)
            action(logger, exception);
    }
    public static void InvokeLog<T1>(
        this Action<ILogger, T1, Exception?> action,
        ILogger? logger,
        T1 arg1,
        Exception? exception = null)
    {
        if (logger is not null)
            action(logger, arg1, exception);
    }
    public static void InvokeLog<T1, T2>(
        this Action<ILogger, T1, T2, Exception?> action,
        ILogger? logger,
        T1 arg1,
        T2 arg2,
        Exception? exception = null)
    {
        if (logger is not null)
            action(logger, arg1, arg2, exception);
    }
    public static void InvokeLog<T1, T2, T3>(
        this Action<ILogger, T1, T2, T3, Exception?> action,
        ILogger? logger,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        Exception? exception = null)
    {
        if (logger is not null)
            action(logger, arg1, arg2, arg3, exception);
    }
}
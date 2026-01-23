using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SimpleArgs;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GICutscenes.CLI;

static partial class Program
{
    private static readonly Dictionary<string, ulong> _versionMap = [];
    private static ILogger _logger = NullLogger.Instance;
    private static LogLevel _logLevel = LogLevel.Information;
    private static KeyMode _keyMode = KeyMode.Default;
    private static ulong _key = 0;
    private static bool _usmOnly = true;
    private static bool _outputVideo = true;
    private static bool _outputAudio = true;

    private static int Main(string[] args)
    {
        const string argHelp = "--help";
        const string argInput = "--input";
        const string argDirectory = "--directory";
        const string argVersionJson = "--version-json";
        const string argLogLevel = "--log-level";
        const string argKeyMode = "--key-mode";
        const string argKey = "--key";
        const string argUSMOnly = "--usm-only";
        const string argOutputVideo = "--output-video";
        const string argOutputAudio = "--output-audio";
        Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
        ArgParser argx = new(args, ignoreCase: true,
            new(argHelp, 0, "-h", "-?"),
            new(argInput, 1, "-in", "-i")
            {
                Info = "path (required): USM file/folder"
            },
            new(argDirectory, 1, "-dir", "-d")
            {
                Default = "./",
                Info = "path: output directory"
            },
            new(argVersionJson, 1, "-vj")
            {
                Default = "./versions.json",
                Info = "path: versions.json"
            },
            new(argLogLevel, 1, "-log")
            {
                Default = "Information",
                Info = "enum: log level"
            },
            new(argKeyMode, 1, "-km")
            {
                Default = "Default",
                Info = "enum (flag): key mode"
            },
            new(argKey, 1, "-k")
            {
                Default = "0",
                Info = "uint64: key"
            },
            new(argUSMOnly, 1, "-usmonly")
            {
                Default = "true",
                Info = "boolean: only process files with .usm extension"
            },
            new(argOutputVideo, 1, "-v")
            {
                Default = "true",
                Info = "boolean: whether should output video files (*.ivf) or not"
            },
            new(argOutputAudio, 1, "-a")
            {
                Default = "true",
                Info = "boolean: whether should output audio files (*.hca) or not"
            });
        if (argx.Results.ContainsKey(argHelp))
        {
            PrintHelp(argx);
            return 0;
        }
        if (!argx.TryGetString(argInput, out string? usm))
        {
            Console.Out.WriteLine($"""
                Missing value for argument {argInput}

                """);
            PrintHelp(argx);
            return 1;
        }
        string outputDir = argx.GetStringArgument(argDirectory);
        string versionJson = argx.GetStringArgument(argVersionJson);
        if (!argx.TryGetEnum(argLogLevel, out _logLevel))
        {
            string value = argx.GetStringArgument(argLogLevel);
            Console.Out.WriteLine($"Invalid value for argument {argLogLevel}: {value}");
            return 1;
        }
        if (!argx.TryGetEnum(argKeyMode, out _keyMode))
        {
            string value = argx.GetStringArgument(argKeyMode);
            Console.Out.WriteLine($"Invalid value for argument {argKeyMode}: {value}");
            return 1;
        }
        if (!argx.TryGet(argKey, out _key))
        {
            string value = argx.GetStringArgument(argKey);
            Console.Out.WriteLine($"Invalid value for argument {argKey}: {value}");
            return 1;
        }
        if (!argx.TryGetBoolean(argUSMOnly, out _usmOnly))
        {
            string value = argx.GetStringArgument(argUSMOnly);
            Console.Out.WriteLine($"Invalid value for argument {argUSMOnly}: {value}");
            return 1;
        }
        if (!argx.TryGetBoolean(argOutputVideo, out _outputVideo))
        {
            string value = argx.GetStringArgument(argOutputVideo);
            Console.Out.WriteLine($"Invalid value for argument {argOutputVideo}: {value}");
            return 1;
        }
        if (!argx.TryGetBoolean(argOutputAudio, out _outputAudio))
        {
            string value = argx.GetStringArgument(argOutputAudio);
            Console.Out.WriteLine($"Invalid value for argument {argOutputAudio}: {value}");
            return 1;
        }
        if (_logLevel < LogLevel.None)
            _logger = new ConsoleOutLogger(_logLevel);
        if (!File.Exists(versionJson))
            _logger?.LogAction(Events.LogFileNotFoundWarning, versionJson);
        else
        {
            using FileStream versionsStream = File.OpenRead(versionJson);
            VersionList? versions = JsonSerializer.Deserialize(versionsStream, VersionJson.Default.VersionList);
            if (versions?.List is null)
                _logger?.LogAction(Events.LogNoVersionInfoLoaded);
            else
                VersionInfo.Flatten(versions.List, FlattenAction);
        }

        Directory.CreateDirectory(outputDir);
        long count = 0, total = 0;
        if (File.Exists(usm))
        {
            count += ProcessUSM(usm, outputDir) ? 1 : 0;
            total++;
        }
        else if (Directory.Exists(usm))
        {
            foreach (string usmFile in Directory.EnumerateFiles(usm, _usmOnly ? "*.usm" : "*", new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple
            }))
            {
                string relative = Path.GetRelativePath(usm, Path.GetDirectoryName(usmFile)!);
                string rOutputDir = relative == "." ? outputDir : Path.Combine(outputDir, relative);
                count += ProcessUSM(usmFile, rOutputDir) ? 1 : 0;
                total++;
            }
        }
        else
        {
            _logger?.LogAction(Events.LogFileNotFoundError, usm);
            return 1;
        }
        _logger?.LogAction(Events.LogStatistics, total, count);
        return 0;
    }
    private static void PrintHelp(ArgParser argx)
    {
        Console.Out.WriteLine("""
                Usage:
                  GICutscenes.CLI [...arguments]

                """);
        argx.WriteHelp(Console.Out);
        Console.Out.WriteLine("""


                Enum information:
                Supports case-insensitive names and decimal numbers.

                  valid value for enum LogLevel
                    Trace       = 0 // Unused in this project
                    Debug       = 1
                    Information = 2
                    Warning     = 3
                    Error       = 4
                    Critical    = 5 // Unused in this project
                    None        = 6
                  
                  valid value for enum KeyMode: // Flag enum, combine value by commas (,)
                    None                           = 0
                    FromExternal                   = 1  // Conflict with PreferExternalFallbackProvided
                    FromProvided                   = 2  // Conflict with PreferExternalFallbackProvided
                    FromName                       = 4
                    PreferExternalFallbackProvided = 8  // Conflict with FromExternal or FromProvided
                    Default                        = 12 // = PreferExternalFallbackProvided, FromName
                """);
    }

    private static void FlattenAction(string name, ulong key, bool encAudio)
        => _versionMap[name] = key;
    private static bool ProcessUSM(string file, string outputDir)
    {
        if (!File.Exists(file))
            return false;
        _logger?.LogAction(Events.LogProcessFile, file);
        if (_usmOnly && !Path.GetExtension(file.AsSpan()).Equals(".usm", StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogAction(Events.LogNotUSM);
            return false;
        }
        string usmName = Path.GetFileNameWithoutExtension(file);
        ulong key = _keyMode.HasFlag(KeyMode.FromName) ? KeyUtils.GetEncryptionKey(usmName) : 0;
        if (_keyMode.HasFlag(KeyMode.PreferExternalFallbackProvided))
        {
            if (_versionMap.TryGetValue(usmName, out ulong keyE))
                key += keyE;
            else
            {
                _logger?.LogAction(Events.LogNoVersionInfoFor, usmName);
                key += _key;
            }
        }
        else
        {
            if (_keyMode.HasFlag(KeyMode.FromProvided))
                key += _key;
            if (_keyMode.HasFlag(KeyMode.FromExternal))
            {
                if (!_versionMap.TryGetValue(usmName, out ulong keyE))
                {
                    _logger?.LogAction(Events.LogNoVersionInfoFor, usmName);
                    return false;
                }
                key += keyE;
            }
        }
        _logger?.LogAction(Events.LogReadFile, file);
        bool result;
        try
        {
            using FileStream usmStream = File.OpenRead(file);
            if (!_outputVideo && !_outputAudio)
                result = DemuxContext.TryDemuxAndDecrypt(key, usmStream, null, null, _logger);
            else
            {
                Context ctx = new(outputDir, usmName);
                result = DemuxContext.TryDemuxAndDecrypt(
                    key,
                    usmStream,
                    _outputVideo ? ctx.VideoOutputFactory : null,
                    _outputAudio ? ctx.AudioOutputFactory : null,
                    _logger);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogAction(Events.LogUSMDemuxFailed, ex);
            return false;
        }
        if (!result)
        {
            _logger?.LogAction(Events.LogUSMDemuxFailed);
            return false;
        }
        _logger?.LogAction(Events.LogDone);
        return true;
    }

    [Flags]
    enum KeyMode
    {
        None = 0,
        FromExternal = 0b0001,
        FromProvided = 0b0010,
        FromName = 0b0100,
        PreferExternalFallbackProvided = 0b1000,
        Default = PreferExternalFallbackProvided | FromName,
    }
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(VersionList))]
    sealed partial class VersionJson : JsonSerializerContext;
    sealed class Context(string outputDir, string usmName)
    {
        public Stream VideoOutputFactory()
        {
            string ivfFile = Path.Combine(outputDir, $"{usmName}.ivf");
            _logger?.LogAction(Events.LogCreateFile, ivfFile);
            return File.Open(ivfFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        public Stream AudioOutputFactory(byte chNo)
        {
            string hcaFile = Path.Combine(outputDir, $"{usmName}.{chNo}.hca");
            _logger?.LogAction(Events.LogCreateFile, hcaFile);
            return File.Open(hcaFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }
}
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SimpleArgs;
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
    private static bool _usmOnly = false;
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
                Default = "false",
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
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(BuildLoggerFactory);
            _logger = loggerFactory.CreateLogger("Main");
        }
        if (!File.Exists(versionJson))
            Events.LogFileNotFoundWarning.InvokeLog(_logger, versionJson);
        else
        {
            using FileStream versionsStream = File.OpenRead(versionJson);
            VersionList? versions = JsonSerializer.Deserialize(versionsStream, VersionJson.Default.VersionList);
            if (versions?.List is null)
                Events.LogNoVersionInfoLoaded.InvokeLog(_logger);
            else
                VersionInfo.Flatten(versions.List, FlattenAction);
        }

        Directory.CreateDirectory(outputDir);
        int count = 0;
        if (File.Exists(usm))
            count += ProcessUSM(usm, outputDir) ? 1 : 0;
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
                string rOutputDir = Path.Combine(outputDir, Path.GetRelativePath(usmFile, usm));
                count += ProcessUSM(usmFile, rOutputDir) ? 1 : 0;
            }
        }
        else
        {
            Events.LogFileNotFoundError.InvokeLog(_logger, usm);
            return 1;
        }
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
    private static void BuildLoggerFactory(ILoggingBuilder builder)
    {
        builder.AddConsole();
        builder.SetMinimumLevel(_logLevel);
    }
    private static bool ProcessUSM(string file, string outputDir)
    {
        if (!File.Exists(file))
            return false;
        Events.LogProcessFile.InvokeLog(_logger, file);
        if (_usmOnly && !Path.GetExtension(file.AsSpan()).Equals(".usm", StringComparison.OrdinalIgnoreCase))
        {
            Events.LogNotUSM.InvokeLog(_logger);
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
                Events.LogNoVersionInfoFor.InvokeLog(_logger, usmName);
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
                    Events.LogNoVersionInfoFor.InvokeLog(_logger, usmName);
                    return false;
                }
                key += keyE;
            }
        }
        Events.LogReadFile.InvokeLog(_logger, file);
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
            Events.LogUSMDemuxFailed.InvokeLog(_logger, ex);
            return false;
        }
        if (!result)
        {
            Events.LogUSMDemuxFailed.InvokeLog(_logger);
            return false;
        }
        Events.LogDone.InvokeLog(_logger);
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
            Events.LogCreateFile.InvokeLog(_logger, ivfFile);
            return File.Open(ivfFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        public Stream AudioOutputFactory(byte chNo)
        {
            string hcaFile = Path.Combine(outputDir, $"{usmName}.{chNo}.hca");
            Events.LogCreateFile.InvokeLog(_logger, hcaFile);
            return File.Open(hcaFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }
}
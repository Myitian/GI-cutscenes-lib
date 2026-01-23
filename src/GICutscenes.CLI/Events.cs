using Microsoft.Extensions.Logging;

namespace GICutscenes.CLI;

static class Events
{
    /// <summary>
    /// Done!
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogDone = LoggerMessage.Define(
        LogLevel.Information,
        Done,
        "Done!");
    public static readonly EventId Done = new(1900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(Done)}");

    /// <summary>
    /// Read file: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogReadFile = LoggerMessage.Define<string>(
        LogLevel.Information,
        ReadFile,
        "Read file: {File}");
    public static readonly EventId ReadFile = new(2900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(ReadFile)}");

    /// <summary>
    /// Create file: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogCreateFile = LoggerMessage.Define<string>(
        LogLevel.Information,
        CreateFile,
        "Create file: {File}");
    public static readonly EventId CreateFile = new(2901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(CreateFile)}");

    /// <summary>
    /// Process file: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogProcessFile = LoggerMessage.Define<string>(
        LogLevel.Information,
        ProcessFile,
        "Process file: {File}");
    public static readonly EventId ProcessFile = new(2902, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(ProcessFile)}");

    /// <summary>
    /// File not found: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogFileNotFoundWarning = LoggerMessage.Define<string>(
        LogLevel.Error,
        FileNotFoundWarning,
        "File not found: {File}");
    public static readonly EventId FileNotFoundWarning = new(8900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(FileNotFoundWarning)}");

    /// <summary>
    /// No version info for {File}, skipping...
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogNoVersionInfoFor = LoggerMessage.Define<string>(
        LogLevel.Warning,
        NoVersionInfoFor,
        "No version info for {File}, skipping...");
    public static readonly EventId NoVersionInfoFor = new(8901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(NoVersionInfoFor)}");

    /// <summary>
    /// Not a USM file, skipping...
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogNotUSM = LoggerMessage.Define(
        LogLevel.Warning,
        NotUSM,
        "Not a USM file, skipping...");
    public static readonly EventId NotUSM = new(8902, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(NotUSM)}");

    /// <summary>
    /// File not found: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogFileNotFoundError = LoggerMessage.Define<string>(
        LogLevel.Error,
        FileNotFoundError,
        "File not found: {File}");
    public static readonly EventId FileNotFoundError = new(9900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(FileNotFoundError)}");

    /// <summary>
    /// No version info loaded
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogNoVersionInfoLoaded = LoggerMessage.Define(
        LogLevel.Error,
        NoVersionInfoLoaded,
        "No version info loaded");
    public static readonly EventId NoVersionInfoLoaded = new(9901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(NoVersionInfoLoaded)}");

    /// <summary>
    /// USM demux failed
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogUSMDemuxFailed = LoggerMessage.Define(
        LogLevel.Error,
        USMDemuxFailed,
        "USM demux failed");
    public static readonly EventId USMDemuxFailed = new(9902, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(USMDemuxFailed)}");

    /// <summary>
    /// HCA demux failed: Ch.{ChNo}
    /// </summary>
    internal static readonly Action<ILogger, byte, Exception?> LogHCADecryptFailed = LoggerMessage.Define<byte>(
        LogLevel.Error,
        HCADecryptFailed,
        "HCA demux failed: Ch.{ChNo}");
    public static readonly EventId HCADecryptFailed = new(9903, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(HCADecryptFailed)}");

    /// <summary>
    /// Missing value for argument {Argument}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogMissingValueForArgument = LoggerMessage.Define<string>(
        LogLevel.Error,
        MissingValueForArgument,
        "Missing value for argument {Argument}");
    public static readonly EventId MissingValueForArgument = new(9904, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(MissingValueForArgument)}");

    /// <summary>
    /// Invalid value for argument {Argument}: {Value}
    /// </summary>
    internal static readonly Action<ILogger, string, string, Exception?> LogInvalidValueForArgument = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        InvalidValueForArgument,
        "Invalid value for argument {Argument}: {Value}");
    public static readonly EventId InvalidValueForArgument = new(9905, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(InvalidValueForArgument)}");
}
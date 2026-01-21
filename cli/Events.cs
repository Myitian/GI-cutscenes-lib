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
    /// No version info for {File}, skipping...
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogNoVersionInfoFor = LoggerMessage.Define<string>(
        LogLevel.Warning,
        NoVersionInfoFor,
        "No version info for {File}, skipping...");
    public static readonly EventId NoVersionInfoFor = new(8900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(NoVersionInfoFor)}");

    /// <summary>
    /// Not a USM file, skipping...
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogNotUSM = LoggerMessage.Define(
        LogLevel.Warning,
        NotUSM,
        "Not a USM file, skipping...");
    public static readonly EventId NotUSM = new(8901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(NotUSM)}");

    /// <summary>
    /// File not found: {File}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogFileNotFound = LoggerMessage.Define<string>(
        LogLevel.Error,
        FileNotFound,
        "File not found: {File}");
    public static readonly EventId FileNotFound = new(9900, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(FileNotFound)}");

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
    public static readonly EventId USMDemuxFailed = new(9901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(USMDemuxFailed)}");

    /// <summary>
    /// HCA demux failed: Ch.{ChNo}
    /// </summary>
    internal static readonly Action<ILogger, byte, Exception?> LogHCADecryptFailed = LoggerMessage.Define<byte>(
        LogLevel.Error,
        HCADecryptFailed,
        "HCA demux failed: Ch.{ChNo}");
    public static readonly EventId HCADecryptFailed = new(9901, $"{nameof(GICutscenes)}_{nameof(CLI)}_{nameof(HCADecryptFailed)}");
}
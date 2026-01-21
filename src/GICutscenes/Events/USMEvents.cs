using GICutscenes.FileTypes;
using Microsoft.Extensions.Logging;

namespace GICutscenes.Events;

public static class USMEvents
{
    /// <summary>
    /// Skip @SFV chunk
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogSkipSFVChunk = LoggerMessage.Define(
        LogLevel.Debug,
        SkipSFVChunk,
        "Skip @SFV chunk because videoOutputFactory is null");
    public static readonly EventId SkipSFVChunk = new(2100, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipSFVChunk)}");

    /// <summary>
    /// Skip @SFA chunk
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogSkipSFAChunk = LoggerMessage.Define(
        LogLevel.Debug,
        SkipSFAChunk,
        "Skip @SFA chunk because audioOutputFactory is null");
    public static readonly EventId SkipSFAChunk = new(2101, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipSFAChunk)}");

    /// <summary>
    /// Skip unused @CUE chunk
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogSkipCUEChunk = LoggerMessage.Define(
        LogLevel.Information,
        SkipCUEChunk,
        "Skip unused @CUE chunk");
    public static readonly EventId SkipCUEChunk = new(2102, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipCUEChunk)}");

    /// <summary>
    /// Skip unknown chunk {Signature}
    /// </summary>
    internal static readonly Action<ILogger, uint, Exception?> LogSkipUnknownChunk = LoggerMessage.Define<uint>(
        LogLevel.Information,
        SkipUnknownChunk,
        "Skip unknown chunk {Signature}");
    public static readonly EventId SkipUnknownChunk = new(2103, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipUnknownChunk)}");

    /// <summary>
    /// Skip unused video data type {DataType}
    /// </summary>
    internal static readonly Action<ILogger, byte, Exception?> LogSkipUnusedVideoDataType = LoggerMessage.Define<byte>(
        LogLevel.Debug,
        SkipUnusedVideoDataType,
        "Skip unused video data type {DataType}");
    public static readonly EventId SkipUnusedVideoDataType = new(2104, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipUnusedVideoDataType)}");

    /// <summary>
    /// Skip unused audio data type {DataType}
    /// </summary>
    internal static readonly Action<ILogger, byte, Exception?> LogSkipUnusedAudioDataType = LoggerMessage.Define<byte>(
        LogLevel.Debug,
        SkipUnusedAudioDataType,
        "Skip unused audio data type {DataType}");
    public static readonly EventId SkipUnusedAudioDataType = new(2105, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(SkipUnusedAudioDataType)}");

    /// <summary>
    /// Invalid data: {DataSize}, {DataOffset}, {PaddingSize}
    /// </summary>
    internal static readonly Action<ILogger, uint, byte, ushort, Exception?> LogInvalidData = LoggerMessage.Define<uint, byte, ushort>(
        LogLevel.Error,
        InvalidData,
        "Invalid data: {DataSize}, {DataOffset}, {PaddingSize}");
    public static readonly EventId InvalidData = new(9100, $"{nameof(GICutscenes)}_{nameof(USM)}_{nameof(InvalidData)}");
}
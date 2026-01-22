using GICutscenes.FileTypes;
using Microsoft.Extensions.Logging;

namespace GICutscenes.Events;

public static class HCAEvents
{
    /// <summary>
    /// Invalid signature: {Signature}
    /// </summary>
    internal static readonly Action<ILogger, string, Exception?> LogInvalidSignature = LoggerMessage.Define<string>(
        LogLevel.Error,
        InvalidSignature,
        "Invalid signature: {Signature}");
    public static readonly EventId InvalidSignature = new(9200, $"{nameof(GICutscenes)}_{nameof(HCA)}_{nameof(InvalidSignature)}");

    /// <summary>
    /// Invalid header: unknown block {Signature}
    /// </summary>
    internal static readonly Action<ILogger, uint, Exception?> LogInvalidHeaderUnknownBlock = LoggerMessage.Define<uint>(
        LogLevel.Error,
        InvalidHeaderUnknownBlock,
        "Invalid header: unknown block {Signature}");
    public static readonly EventId InvalidHeaderUnknownBlock = new(9201, $"{nameof(GICutscenes)}_{nameof(HCA)}_{nameof(InvalidHeaderUnknownBlock)}");

    /// <summary>
    /// Invalid header: block size is zero
    /// </summary>
    internal static readonly Action<ILogger, Exception?> LogInvalidHeaderBlockSizeIsZero = LoggerMessage.Define(
        LogLevel.Error,
        InvalidHeaderBlockSizeIsZero,
        "Invalid header: block size is zero");
    public static readonly EventId InvalidHeaderBlockSizeIsZero = new(9202, $"{nameof(GICutscenes)}_{nameof(HCA)}_{nameof(InvalidHeaderBlockSizeIsZero)}");

    /// <summary>
    /// Invalid header: unknown cipher type {CipherType}
    /// </summary>
    internal static readonly Action<ILogger, ushort, Exception?> LogInvalidHeaderUnknownCipherType = LoggerMessage.Define<ushort>(
        LogLevel.Error,
        InvalidHeaderUnknownCipherType,
        "Invalid signature: unknown cipher type {CipherType}");
    public static readonly EventId InvalidHeaderUnknownCipherType = new(9203, $"{nameof(GICutscenes)}_{nameof(HCA)}_{nameof(InvalidHeaderUnknownCipherType)}");
}
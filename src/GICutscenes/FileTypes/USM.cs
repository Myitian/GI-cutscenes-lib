using GICutscenes.Events;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GICutscenes.FileTypes;

public readonly struct USM : IEquatable<USM>
{
    [InlineArray(0x20)]
    private struct Mask
    {
        private byte _;
    }
    private readonly Mask _videoMask1;
    private readonly Mask _videoMask2;
    private readonly Mask _audioMask;
    public USM(ulong key)
    {
        Span<byte> videoMask1 = _videoMask1;
        Span<byte> videoMask2 = _videoMask2;
        Span<byte> audioMask = _audioMask;
        videoMask1[0x00] = (byte)key;
        videoMask1[0x01] = (byte)(key >> (1 * 8));
        videoMask1[0x02] = (byte)(key >> (2 * 8));
        videoMask1[0x03] = (byte)((key >> (3 * 8)) - 0x34);
        videoMask1[0x04] = (byte)((key >> (4 * 8)) + 0xF9);
        videoMask1[0x05] = (byte)((key >> (5 * 8)) ^ 0x13);
        videoMask1[0x06] = (byte)((key >> (6 * 8)) + 0x61);
        videoMask1[0x07] = (byte)~videoMask1[0x00];
        videoMask1[0x08] = (byte)(videoMask1[0x02] + videoMask1[0x01]);
        videoMask1[0x09] = (byte)(videoMask1[0x01] - videoMask1[0x07]);
        videoMask1[0x0A] = (byte)~videoMask1[0x02];
        videoMask1[0x0B] = (byte)~videoMask1[0x01];
        videoMask1[0x0C] = (byte)(videoMask1[0x0B] + videoMask1[0x09]);
        videoMask1[0x0D] = (byte)(videoMask1[0x08] - videoMask1[0x03]);
        videoMask1[0x0E] = (byte)~videoMask1[0x0D];
        videoMask1[0x0F] = (byte)(videoMask1[0x0A] - videoMask1[0x0B]);
        videoMask1[0x10] = (byte)(videoMask1[0x08] - videoMask1[0x0F]);
        videoMask1[0x11] = (byte)(videoMask1[0x10] ^ videoMask1[0x07]);
        videoMask1[0x12] = (byte)~videoMask1[0x0F];
        videoMask1[0x13] = (byte)(videoMask1[0x03] ^ 0x10);
        videoMask1[0x14] = (byte)(videoMask1[0x04] - 0x32);
        videoMask1[0x15] = (byte)(videoMask1[0x05] + 0xED);
        videoMask1[0x16] = (byte)(videoMask1[0x06] ^ 0xF3);
        videoMask1[0x17] = (byte)(videoMask1[0x13] - videoMask1[0x0F]);
        videoMask1[0x18] = (byte)(videoMask1[0x15] + videoMask1[0x07]);
        videoMask1[0x19] = (byte)(0x21 - videoMask1[0x13]);
        videoMask1[0x1A] = (byte)(videoMask1[0x14] ^ videoMask1[0x17]);
        videoMask1[0x1B] = (byte)(videoMask1[0x16] << 1);
        videoMask1[0x1C] = (byte)(videoMask1[0x17] + 0x44);
        videoMask1[0x1D] = (byte)(videoMask1[0x03] + videoMask1[0x04]);
        videoMask1[0x1E] = (byte)(videoMask1[0x05] - videoMask1[0x16]);
        videoMask1[0x1F] = (byte)(videoMask1[0x1D] ^ videoMask1[0x13]);
        Span<ulong> videoMask1View = MemoryMarshal.Cast<byte, ulong>(videoMask1);
        Span<ulong> videoMask2View = MemoryMarshal.Cast<byte, ulong>(videoMask2);
        Span<ulong> audioMaskView = MemoryMarshal.Cast<byte, ulong>(audioMask);
        ulong maskA = BitConverter.IsLittleEndian ? 0x00FF00FF00FF00FFUL : 0xFF00FF00FF00FF00UL;
        ulong maskB = BitConverter.IsLittleEndian ?
            (((ulong)'C' << 56) | ((ulong)'U' << 40) | ((ulong)'R' << 24) | ((ulong)'U' << 8)) :
            (((ulong)'U' << 48) | ((ulong)'R' << 32) | ((ulong)'U' << 16) | ((ulong)'C' << 0));
        for (int i = 0; i < 4; i++)
            audioMaskView[i] = ((videoMask2View[i] = ~videoMask1View[i]) & maskA) | maskB;
    }
    private void MaskVideo(Span<byte> data)
    {
        const int dataOffset = 0x40;
        if (data.Length - dataOffset < 0x200)
            return;
        data = data[dataOffset..];
        ReadOnlySpan<byte> videoMask1 = _videoMask1;
        ReadOnlySpan<byte> videoMask2 = _videoMask2;
        Span<byte> mask = stackalloc byte[0x20];
        videoMask2.CopyTo(mask);
        for (int i = 0x100; i < data.Length; i++)
            mask[i & 0x1F] = (byte)((data[i] ^= mask[i & 0x1F]) ^ videoMask2[i & 0x1F]);
        videoMask1.CopyTo(mask);
        for (int i = 0; i < 0x100; i++)
            data[i] ^= mask[i & 0x1F] ^= data[0x100 + i];
    }
    // Not used anyway, but might be in the future
    private void MaskAudio(Span<byte> data)
    {
        const int dataOffset = 0x140;
        data = data[dataOffset..];
        ReadOnlySpan<byte> audioMask = _audioMask;
        for (int i = 0; i < data.Length; i++)  // To be confirmed, could start at the current index of data as well...
            data[i] ^= audioMask[i & 0x1F];
    }
    public bool TryDemux(
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        Stream? videoOutput = null;
        Dictionary<byte, Stream>? audioOutputs = null;
        try
        {
            Span<byte> byteBlock = stackalloc byte[0x20];
            while (input.ReadAtLeast(byteBlock, 0x20, false) == 0x20)
            {
                uint signature = BinaryPrimitives.ReadUInt32BigEndian(byteBlock);
                uint dataSize = BinaryPrimitives.ReadUInt32BigEndian(byteBlock[4..]);
                byte dataOffset = byteBlock[9];
                ushort paddingSize = BinaryPrimitives.ReadUInt16BigEndian(byteBlock[10..]);
                byte chNo = byteBlock[12];
                byte dataType = byteBlock[15];
                int size = (int)(dataSize - dataOffset - paddingSize);
                if (size < 0 || dataOffset < 0x18)
                {
                    USMEvents.LogInvalidData.InvokeLog(logger, dataSize, dataOffset, paddingSize);
                    return false;
                }
                input.Seek(dataOffset - 0x18, SeekOrigin.Current);
                using PooledArrayHandle<byte> array = new(size);
                Span<byte> data = array.Array.AsSpan(0, size);
                int read = input.ReadAtLeast(data, size, false);
                if (read < size)
                {
                    CommonEvents.LogStreamEndedTooEarly.InvokeLog(logger, size, read);
                    return false;
                }
                switch (signature)
                {
                    case ('C' << 24) | ('R' << 16) | ('I' << 8) | 'D':
                        break;
                    // Video block
                    case ('@' << 24) | ('S' << 16) | ('F' << 8) | 'V':
                        if (videoOutputFactory is null)
                        {
                            USMEvents.LogSkipSFVChunk.InvokeLog(logger);
                            break;
                        }
                        switch (dataType)
                        {
                            case 0:
                                MaskVideo(data);
                                videoOutput ??= videoOutputFactory.Invoke();
                                videoOutput.Write(data);
                                break;
                            default: // Not implemented, we don't have any uses for it
                                USMEvents.LogSkipUnusedVideoDataType.InvokeLog(logger, dataType);
                                break;
                        }
                        break;
                    // Audio block
                    case ('@' << 24) | ('S' << 16) | ('F' << 8) | 'A':
                        if (audioOutputFactory is null)
                        {
                            USMEvents.LogSkipSFAChunk.InvokeLog(logger);
                            break;
                        }
                        switch (dataType)
                        {
                            case 0:
                                // Might need some extra work if the audio has to be decrypted during the demuxing
                                // (hello AudioMask)
                                audioOutputs ??= [];
                                ref Stream? audioOutput = ref CollectionsMarshal.GetValueRefOrAddDefault(audioOutputs, chNo, out _);
                                audioOutput ??= audioOutputFactory(chNo);
                                audioOutput.Write(data);
                                break;
                            default: // No need to implement it, we lazy
                                USMEvents.LogSkipUnusedAudioDataType.InvokeLog(logger, dataType);
                                break;
                        }
                        break;
                    default:
                        if (logger is null)
                            break;
                        if (signature is (('@' << 24) | ('C' << 16) | ('U' << 8) | 'E'))
                            // Might be used to play a certain part of the video, but shouldn't be needed anyway
                            // (appears in cutscene Cs_Sumeru_AQ30161501_DT)
                            USMEvents.LogSkipCUEChunk.InvokeLog(logger);
                        else
                            USMEvents.LogSkipUnknownChunk.InvokeLog(logger, signature);
                        break;
                }
                input.Seek(paddingSize, SeekOrigin.Current);
            }
            return true;
        }
        finally
        {
            // Closing Streams
            videoOutput?.Dispose();
            if (audioOutputs is not null)
            {
                foreach (Stream audioOutput in audioOutputs.Values)
                    audioOutput.Dispose();
            }
        }
    }
    public async Task<bool> TryDemuxAsync(
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        Stream? videoOutput = null;
        Dictionary<byte, Stream>? audioOutputs = null;
        try
        {
            const int blockInfoSize = 0x20;
            byte[] blockInfo = new byte[blockInfoSize];
            while (await input.ReadAtLeastAsync(blockInfo, blockInfoSize, false, cancellationToken).ConfigureAwait(false) == blockInfoSize)
            {
                uint signature = BinaryPrimitives.ReadUInt32BigEndian(blockInfo);
                uint dataSize = BinaryPrimitives.ReadUInt32BigEndian(blockInfo.AsSpan(4));
                byte dataOffset = blockInfo[9];
                ushort paddingSize = BinaryPrimitives.ReadUInt16BigEndian(blockInfo.AsSpan(10));
                byte chNo = blockInfo[12];
                byte dataType = blockInfo[15];
                int size = (int)(dataSize - dataOffset - paddingSize);
                if (size < 0 || dataOffset < 0x18)
                {
                    USMEvents.LogInvalidData.InvokeLog(logger, dataSize, dataOffset, paddingSize);
                    return false;
                }
                input.Seek(dataOffset - 0x18, SeekOrigin.Current);
                using PooledArrayHandle<byte> array = new(size);
                Memory<byte> data = array.Array.AsMemory(0, size);
                int read = await input.ReadAtLeastAsync(data, size, false, cancellationToken).ConfigureAwait(false);
                if (read < size)
                {
                    CommonEvents.LogStreamEndedTooEarly.InvokeLog(logger, size, read);
                    return false;
                }
                switch (signature)
                {
                    case ('C' << 24) | ('R' << 16) | ('I' << 8) | 'D':
                        break;
                    // Video block
                    case ('@' << 24) | ('S' << 16) | ('F' << 8) | 'V':
                        if (videoOutputFactory is null)
                        {
                            USMEvents.LogSkipSFVChunk.InvokeLog(logger);
                            break;
                        }
                        switch (dataType)
                        {
                            case 0:
                                MaskVideo(data.Span);
                                videoOutput ??= videoOutputFactory.Invoke();
                                await videoOutput.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                                break;
                            default: // Not implemented, we don't have any uses for it
                                USMEvents.LogSkipUnusedVideoDataType.InvokeLog(logger, dataType);
                                break;
                        }
                        break;
                    // Audio block
                    case ('@' << 24) | ('S' << 16) | ('F' << 8) | 'A':
                        if (audioOutputFactory is null)
                        {
                            USMEvents.LogSkipSFAChunk.InvokeLog(logger);
                            break;
                        }
                        switch (dataType)
                        {
                            case 0:
                                // Might need some extra work if the audio has to be decrypted during the demuxing
                                // (hello AudioMask)
                                audioOutputs ??= [];
                                ref Stream? audioOutput = ref CollectionsMarshal.GetValueRefOrAddDefault(audioOutputs, chNo, out _);
                                audioOutput ??= audioOutputFactory(chNo);
                                await audioOutput.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                                break;
                            default: // No need to implement it, we lazy
                                USMEvents.LogSkipUnusedAudioDataType.InvokeLog(logger, dataType);
                                break;
                        }
                        break;
                    default:
                        if (logger is null)
                            break;
                        if (signature is (('@' << 24) | ('C' << 16) | ('U' << 8) | 'E'))
                            // Might be used to play a certain part of the video, but shouldn't be needed anyway
                            // (appears in cutscene Cs_Sumeru_AQ30161501_DT)
                            USMEvents.LogSkipCUEChunk.InvokeLog(logger);
                        else
                            USMEvents.LogSkipUnknownChunk.InvokeLog(logger, signature);
                        break;
                }
                input.Seek(paddingSize, SeekOrigin.Current);
            }
            return true;
        }
        finally
        {
            // Closing Streams
            if (videoOutput is not null)
                await videoOutput.DisposeAsync().ConfigureAwait(false);
            if (audioOutputs is not null)
            {
                foreach (Stream audioOutput in audioOutputs.Values)
                    await audioOutput.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
    public bool Equals(USM other)
        => ((ReadOnlySpan<byte>)_videoMask1).SequenceEqual(other._videoMask1)
        && ((ReadOnlySpan<byte>)_videoMask2).SequenceEqual(other._videoMask2)
        && ((ReadOnlySpan<byte>)_audioMask).SequenceEqual(other._audioMask);
    public override bool Equals(object? obj)
        => obj is USM other && Equals(other);
    public override int GetHashCode()
    {
        HashCode h = new();
        h.AddBytes(_videoMask1);
        h.AddBytes(_videoMask2);
        h.AddBytes(_audioMask);
        return h.ToHashCode();
    }
    public static bool operator ==(USM left, USM right)
        => left.Equals(right);
    public static bool operator !=(USM left, USM right)
        => !left.Equals(right);
}
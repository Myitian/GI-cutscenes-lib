using GICutscenes.Events;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GICutscenes.FileTypes;

public readonly struct HCA : IEquatable<HCA>
{
    [InlineArray(0x100)]
    private struct CiphTable
    {
        private byte _;
    }
    private readonly CiphTable _ciphTable;
    private static ReadOnlySpan<byte> Mask0 => [
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
        0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
        0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
        0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
        0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
        0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
        0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,
        0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
        0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
        0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
        0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
        0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
        0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
        0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
        0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF];
    private static ReadOnlySpan<byte> Mask1 => [
        0x00, 0x9A, 0xDD, 0x44, 0x7F, 0x7E, 0x71, 0xC8, 0x33, 0xA2, 0x45, 0x8C, 0x27, 0x06, 0x59, 0x90,
        0x5B, 0xAA, 0xAD, 0xD4, 0xCF, 0x8E, 0x41, 0x58, 0x83, 0xB2, 0x15, 0x1C, 0x77, 0x16, 0x29, 0x20,
        0xAB, 0xBA, 0x7D, 0x64, 0x1F, 0x9E, 0x11, 0xE8, 0xD3, 0xC2, 0xE5, 0xAC, 0xC7, 0x26, 0xF9, 0xB0,
        0xFB, 0xCA, 0x4D, 0xF4, 0x6F, 0xAE, 0xE1, 0x78, 0x23, 0xD2, 0xB5, 0x3C, 0x17, 0x36, 0xC9, 0x40,
        0x4B, 0xDA, 0x1D, 0x84, 0xBF, 0xBE, 0xB1, 0x08, 0x73, 0xE2, 0x85, 0xCC, 0x67, 0x46, 0x99, 0xD0,
        0x9B, 0xEA, 0xED, 0x14, 0x0F, 0xCE, 0x81, 0x98, 0xC3, 0xF2, 0x55, 0x5C, 0xB7, 0x56, 0x69, 0x60,
        0xEB, 0xFA, 0xBD, 0xA4, 0x5F, 0xDE, 0x51, 0x28, 0x13, 0x02, 0x25, 0xEC, 0x07, 0x66, 0x39, 0xF0,
        0x3B, 0x0A, 0x8D, 0x34, 0xAF, 0xEE, 0x21, 0xB8, 0x63, 0x12, 0xF5, 0x7C, 0x57, 0x76, 0x09, 0x80,
        0x8B, 0x1A, 0x5D, 0xC4, 0xFE, 0xF1, 0x48, 0xB3, 0x22, 0xC5, 0x0C, 0xA7, 0x86, 0xD9, 0x10, 0xDB,
        0x2A, 0x2D, 0x54, 0x4F, 0x0E, 0xC1, 0xD8, 0x03, 0x32, 0x95, 0x9C, 0xF7, 0x96, 0xA9, 0xA0, 0x2B,
        0x3A, 0xFD, 0xE4, 0x9F, 0x1E, 0x91, 0x68, 0x53, 0x42, 0x65, 0x2C, 0x47, 0xA6, 0x79, 0x30, 0x7B,
        0x4A, 0xCD, 0x74, 0xEF, 0x2E, 0x61, 0xF8, 0xA3, 0x52, 0x35, 0xBC, 0x97, 0xB6, 0x49, 0xC0, 0xCB,
        0x5A, 0x9D, 0x04, 0x3F, 0x3E, 0x31, 0x88, 0xF3, 0x62, 0x05, 0x4C, 0xE7, 0xC6, 0x19, 0x50, 0x1B,
        0x6A, 0x6D, 0x94, 0x8F, 0x4E, 0x01, 0x18, 0x43, 0x72, 0xD5, 0xDC, 0x37, 0xD6, 0xE9, 0xE0, 0x6B,
        0x7A, 0x3D, 0x24, 0xDF, 0x5E, 0xD1, 0xA8, 0x93, 0x82, 0xA5, 0x6C, 0x87, 0xE6, 0xB9, 0x70, 0xBB,
        0x8A, 0x0D, 0xB4, 0x2F, 0x6E, 0xA1, 0x38, 0xE3, 0x92, 0x75, 0xFC, 0xD7, 0xF6, 0x89, 0x0B, 0xFF];
    private static ReadOnlySpan<ushort> CheckSumTable => [
        0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022,
        0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041,
        0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1,
        0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082,
        0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1,
        0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2,
        0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162,
        0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132, 0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101,
        0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321,
        0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342,
        0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2,
        0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381,
        0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2,
        0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1,
        0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261,
        0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202];
    [SkipLocalsInit]
    public HCA(ulong key)
    {
        // Note: Using `SkipLocalsInit` here seems to have almost no effect on reducing execution time.
        //       It might reduce a few instructions, such as `vmovdqa` or `vmovdqu`.
        //       Since it doesn't cause any problems, we've kept it.
        Span<byte> ciphTable = _ciphTable;
        Span<byte> t1 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(t1, key - 1);
        ReadOnlySpan<byte> t2 = [
            t1[1],
            (byte)(t1[1] ^ t1[6]),
            (byte)(t1[2] ^ t1[3]),
            t1[2],
            (byte)(t1[2] ^ t1[1]),
            (byte)(t1[3] ^ t1[4]),
            t1[3],
            (byte)(t1[3] ^ t1[2]),
            (byte)(t1[4] ^ t1[5]),
            t1[4],
            (byte)(t1[4] ^ t1[3]),
            (byte)(t1[5] ^ t1[6]),
            t1[5],
            (byte)(t1[5] ^ t1[4]),
            (byte)(t1[6] ^ t1[1]),
            t1[6]
        ];
        Span<byte> t3 = stackalloc byte[0x100];
        Span<byte> t31 = stackalloc byte[0x10];
        Span<byte> t32 = stackalloc byte[0x10];
        Span<ulong> t3u = MemoryMarshal.Cast<byte, ulong>(t3);
        Span<ulong> t32u = MemoryMarshal.Cast<byte, ulong>(t32);
        InitTempTable(t1[0], t31);
        for (int i = 0; i < 0x10; i++)
        {
            InitTempTable(t2[i], t32);
            ulong v = 0x0101010101010101UL * (byte)(t31[i] << 4);
            t3u[i * 2] = v | t32u[0];
            t3u[i * 2 + 1] = v | t32u[1];
        }
        int iTable = 1;
        for (int i = 0, v = 0; i < 0x100; i++)
        {
            v += 0x11;
            byte a = t3[v & 0xFF];
            if (a is not (0 or 0xFF))
                ciphTable[iTable++] = a;
        }
        ciphTable[0] = 0;
        ciphTable[0xFF] = 0xFF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void InitTempTable(uint key, Span<byte> table)
        {
            uint mul = ((key & 1) << 3) | 5;
            uint add = (key & 0xE) | 1;
            key >>= 4;
            for (int i = 0; i < table.Length; i++)
            {
                key = key * mul + add;
                table[i] = (byte)(key & 0xF);
            }
        }
    }
    public static void Mask(Span<byte> data, ReadOnlySpan<byte> ciphTable)
    {
        foreach (ref byte b in data)
            b = ciphTable[b];
    }
    public static ReadOnlySpan<byte> GetMask(in HCA hca, int type)
    {
        return type switch
        {
            // case 0:
            //     for (int i = 0; i < 0x100; i++)
            //         ciphTable[i] = (byte)i;
            0 => Mask0,
            // case 1:
            //     for (int i = 0, v = 0; i < 0xFF; i++)
            //     {
            //         v = v * 13 + 11 & 0xFF;
            //         if (v is 0 or 0xFF) v = v * 13 + 11 & 0xFF;
            //         _ciphTable[i] = (byte)v;
            //     }
            //     ciphTable[0] = 0;
            //     ciphTable[0xFF] = 0xFF;
            1 => Mask1,
            // case 56:
            _ => hca._ciphTable,
        };
    }
    public static ushort CheckSum(ReadOnlySpan<byte> data)
    {
        ushort sum = 0;
        foreach (byte b in data)
            sum = (ushort)((sum << 8) ^ CheckSumTable[(sum >> 8) ^ b]);
        return sum;
    }
    public readonly bool TryDecrypt(
        Stream input,
        Stream output,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        const int magicSize = 8;
        Span<byte> hcaBytes = stackalloc byte[magicSize];
        if (input.ReadAtLeast(hcaBytes, magicSize, false) != magicSize)
            return false;
        uint sign = BinaryPrimitives.ReadUInt32BigEndian(hcaBytes) & 0x7F7F7F7F;
        int headerSize = BinaryPrimitives.ReadUInt16BigEndian(hcaBytes[6..]);
        if (sign is not (('H' << 24) | ('C' << 16) | ('A' << 8)) || headerSize < magicSize)
        {
            HCAEvents.LogInvalidSignature.InvokeLog(logger, Convert.ToHexString(hcaBytes));
            return false;
        }
        BinaryPrimitives.WriteUInt32BigEndian(hcaBytes, sign);
        ushort blockSize = 0;
        ushort cipherType = 0;
        using (PooledArrayHandle<byte> array = new(headerSize))
        {
            hcaBytes.CopyTo(array.Array);
            Span<byte> header = array.Array.AsSpan(magicSize, headerSize - magicSize);
            if (input.ReadAtLeast(header, header.Length, false) != header.Length)
                return false;
            while (header.Length >= 4)
            {
                sign = BinaryPrimitives.ReadUInt32BigEndian(header) & 0x7F7F7F7F;
                BinaryPrimitives.WriteUInt32BigEndian(header, sign);
                switch (sign)
                {
                    case ('f' << 24) | ('m' << 16) | ('t' << 8) when header.Length >= 16:
                        header = header[16..];
                        break;
                    case ('c' << 24) | ('o' << 16) | ('m' << 8) | 'p' when header.Length >= 16:
                        blockSize = BinaryPrimitives.ReadUInt16BigEndian(header[4..]);
                        header = header[16..];
                        break;
                    case ('d' << 24) | ('e' << 16) | ('c' << 8) when header.Length >= 12:
                        blockSize = BinaryPrimitives.ReadUInt16BigEndian(header[4..]);
                        header = header[12..];
                        break;
                    case ('v' << 24) | ('b' << 16) | ('r' << 8) when header.Length >= 8:
                        header = header[8..];
                        break;
                    case ('a' << 24) | ('t' << 16) | ('h' << 8) when header.Length >= 6:
                        header = header[6..];
                        break;
                    case ('l' << 24) | ('o' << 16) | ('o' << 8) | 'p' when header.Length >= 16:
                        header = header[16..];
                        break;
                    case ('c' << 24) | ('i' << 16) | ('p' << 8) | 'h' when header.Length >= 6:
                        cipherType = BinaryPrimitives.ReadUInt16BigEndian(header[4..]);
                        header = header[6..];
                        break;
                    case ('r' << 24) | ('v' << 16) | ('a' << 8) when header.Length >= 8:
                        header = header[8..];
                        break;
                    case ('c' << 24) | ('o' << 16) | ('m' << 8) | 'm' when header.Length >= 5:
                        header = header[5..];
                        break;
                    case ('p' << 24) | ('a' << 16) | ('d' << 8):
                        goto HEAD_END;
                    default:
                        HCAEvents.LogInvalidHeaderUnknownBlock.InvokeLog(logger, sign);
                        return false;
                }
            }
        HEAD_END:
            if (blockSize == 0)
            {
                HCAEvents.LogInvalidHeaderBlockSizeIsZero.InvokeLog(logger);
                return false;
            }
            if (cipherType is not (0 or 1 or 56))
            {
                HCAEvents.LogInvalidHeaderUnknownCipherType.InvokeLog(logger, cipherType);
                return false;
            }
            header = array.Array.AsSpan(headerSize);
            ushort checksum = CheckSum(header[..^2]);
            BinaryPrimitives.WriteUInt16BigEndian(header[^2..], checksum);
            output.Write(array.Array, 0, headerSize);
        }
        if (cipherType == 0)
        {
            input.CopyTo(output);
            return true;
        }
        ReadOnlySpan<byte> ciphTable = GetMask(in this, cipherType);
        using (PooledArrayHandle<byte> array = new(blockSize))
        {
            Span<byte> block = array.Array.AsSpan(0, blockSize);
            while (input.ReadAtLeast(block, blockSize, false) is int read and not 0)
            {
                Mask(block, ciphTable);
                switch (blockSize - read)
                {
                    case 0:
                        ushort checksum = CheckSum(block[..^2]);
                        BinaryPrimitives.WriteUInt16BigEndian(block[^2..], checksum);
                        break;
                    case 1:
                        checksum = CheckSum(block[..^1]);
                        block[^1] = (byte)(checksum >> 8);
                        goto default;
                    default:
                        CommonEvents.LogStreamEndedTooEarly.InvokeLog(logger, blockSize, read);
                        break;
                }
                output.Write(array.Array, 0, blockSize);
            }
            return true;
        }
    }
    public readonly async Task<bool> TryDecryptAsync(
        Stream input,
        Stream output,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        const int magicSize = 8;
        byte[] hcaBytes = new byte[magicSize];
        if (await input.ReadAtLeastAsync(hcaBytes, magicSize, false, cancellationToken).ConfigureAwait(false) != magicSize)
            return false;
        uint sign = BinaryPrimitives.ReadUInt32BigEndian(hcaBytes) & 0x7F7F7F7F;
        int headerSize = BinaryPrimitives.ReadUInt16BigEndian(hcaBytes.AsSpan(6));
        if (sign is not (('H' << 24) | ('C' << 16) | ('A' << 8)) || headerSize < magicSize)
        {
            HCAEvents.LogInvalidSignature.InvokeLog(logger, Convert.ToHexString(hcaBytes));
            return false;
        }
        BinaryPrimitives.WriteUInt32BigEndian(hcaBytes, sign);
        ushort blockSize = 0;
        ushort cipherType = 0;
        using (PooledArrayHandle<byte> array = new(headerSize))
        {
            hcaBytes.CopyTo(array.Array);
            Memory<byte> header = array.Array.AsMemory(magicSize, headerSize - magicSize);
            if (await input.ReadAtLeastAsync(header, header.Length, false, cancellationToken).ConfigureAwait(false) != header.Length)
                return false;
            while (header.Length >= 4)
            {
                sign = BinaryPrimitives.ReadUInt32BigEndian(header.Span) & 0x7F7F7F7F;
                BinaryPrimitives.WriteUInt32BigEndian(header.Span, sign);
                switch (sign)
                {
                    case ('f' << 24) | ('m' << 16) | ('t' << 8) when header.Length >= 16:
                        header = header[16..];
                        break;
                    case ('c' << 24) | ('o' << 16) | ('m' << 8) | 'p' when header.Length >= 16:
                        blockSize = BinaryPrimitives.ReadUInt16BigEndian(header.Span[4..]);
                        header = header[16..];
                        break;
                    case ('d' << 24) | ('e' << 16) | ('c' << 8) when header.Length >= 12:
                        blockSize = BinaryPrimitives.ReadUInt16BigEndian(header.Span[4..]);
                        header = header[12..];
                        break;
                    case ('v' << 24) | ('b' << 16) | ('r' << 8) when header.Length >= 8:
                        header = header[8..];
                        break;
                    case ('a' << 24) | ('t' << 16) | ('h' << 8) when header.Length >= 6:
                        header = header[6..];
                        break;
                    case ('l' << 24) | ('o' << 16) | ('o' << 8) | 'p' when header.Length >= 16:
                        header = header[16..];
                        break;
                    case ('c' << 24) | ('i' << 16) | ('p' << 8) | 'h' when header.Length >= 6:
                        cipherType = BinaryPrimitives.ReadUInt16BigEndian(header.Span[4..]);
                        header = header[6..];
                        break;
                    case ('r' << 24) | ('v' << 16) | ('a' << 8) when header.Length >= 8:
                        header = header[8..];
                        break;
                    case ('c' << 24) | ('o' << 16) | ('m' << 8) | 'm' when header.Length >= 5:
                        header = header[5..];
                        break;
                    case ('p' << 24) | ('a' << 16) | ('d' << 8):
                        goto HEAD_END;
                    default:
                        HCAEvents.LogInvalidHeaderUnknownBlock.InvokeLog(logger, sign);
                        return false;
                }
            }
        HEAD_END:
            if (blockSize == 0)
            {
                HCAEvents.LogInvalidHeaderBlockSizeIsZero.InvokeLog(logger);
                return false;
            }
            if (cipherType is not (0 or 1 or 56))
            {
                HCAEvents.LogInvalidHeaderUnknownCipherType.InvokeLog(logger, cipherType);
                return false;
            }
            header = array.Array.AsMemory(0, headerSize);
            ushort checksum = CheckSum(header.Span[..^2]);
            BinaryPrimitives.WriteUInt16BigEndian(header.Span[^2..], checksum);
            await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        }
        if (cipherType == 0)
        {
            await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
            return true;
        }
        using (PooledArrayHandle<byte> array = new(blockSize))
        {
            Memory<byte> block = array.Array.AsMemory(0, blockSize);
            while (await input.ReadAtLeastAsync(block, blockSize, false, cancellationToken).ConfigureAwait(false) is int read and not 0)
            {
                Span<byte> span = block.Span;
                Mask(span, GetMask(in this, cipherType));
                switch (blockSize - read)
                {
                    case 0:
                        ushort checksum = CheckSum(span[..^2]);
                        BinaryPrimitives.WriteUInt16BigEndian(span[^2..], checksum);
                        break;
                    case 1:
                        checksum = CheckSum(span[..^1]);
                        span[^1] = (byte)(checksum >> 8);
                        goto default;
                    default:
                        CommonEvents.LogStreamEndedTooEarly.InvokeLog(logger, blockSize, read);
                        break;
                }
                await output.WriteAsync(block, cancellationToken).ConfigureAwait(false);
            }
            return true;
        }
    }
    public bool Equals(HCA other)
        => ((ReadOnlySpan<byte>)_ciphTable).SequenceEqual(other._ciphTable);
    public override bool Equals(object? obj)
        => obj is HCA other && Equals(other);
    public override int GetHashCode()
    {
        HashCode h = new();
        h.AddBytes(_ciphTable);
        return h.ToHashCode();
    }
    public static bool operator ==(HCA left, HCA right)
        => left.Equals(right);
    public static bool operator !=(HCA left, HCA right)
        => !left.Equals(right);
}
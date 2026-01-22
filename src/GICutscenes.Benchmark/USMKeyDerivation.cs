using BenchmarkDotNet.Attributes;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GICutscenes.Benchmark;

[MediumRunJob]
[MeanColumn]
[DisassemblyDiagnoser]
[MarkdownExporter]
public class USMKeyDerivation
{
    [InlineArray(4)]
    private struct KeyStruct
    {
        private byte _;
    }
    [InlineArray(0x20)]
    private struct Mask
    {
        private byte _;
    }
    private ulong _key;
    private KeyStruct _key1;
    private KeyStruct _key2;
    private Mask _videoMask1;
    private Mask _videoMask2;
    private Mask _audioMask;
    [GlobalSetup]
    public void GlobalSetup() => SetKey((ulong)Random.Shared.NextInt64());
    [Benchmark(Baseline = true)]
    public void Original() // about 20 ns on [.NET 10.0.2, X64 RyuJIT x86-64-v3]
    {
        // Note: Compared to the real original algorithm, array allocations has been removed.
        Span<byte> key1 = _key1;
        Span<byte> key2 = _key2;
        Span<byte> videoMask1 = _videoMask1;
        Span<byte> videoMask2 = _videoMask2;
        Span<byte> audioMask = _audioMask;
        videoMask1[0x00] = key1[0];
        videoMask1[0x01] = key1[1];
        videoMask1[0x02] = key1[2];
        videoMask1[0x03] = (byte)(key1[3] - 0x34);
        videoMask1[0x04] = (byte)(key2[0] + 0xF9);
        videoMask1[0x05] = (byte)(key2[1] ^ 0x13);
        videoMask1[0x06] = (byte)(key2[2] + 0x61);
        videoMask1[0x07] = (byte)(videoMask1[0x00] ^ 0xFF);
        videoMask1[0x08] = (byte)(videoMask1[0x02] + videoMask1[0x01]);
        videoMask1[0x09] = (byte)(videoMask1[0x01] - videoMask1[0x07]);
        videoMask1[0x0A] = (byte)(videoMask1[0x02] ^ 0xFF);
        videoMask1[0x0B] = (byte)(videoMask1[0x01] ^ 0xFF);
        videoMask1[0x0C] = (byte)(videoMask1[0x0B] + videoMask1[0x09]);
        videoMask1[0x0D] = (byte)(videoMask1[0x08] - videoMask1[0x03]);
        videoMask1[0x0E] = (byte)(videoMask1[0x0D] ^ 0xFF);
        videoMask1[0x0F] = (byte)(videoMask1[0x0A] - videoMask1[0x0B]);
        videoMask1[0x10] = (byte)(videoMask1[0x08] - videoMask1[0x0F]);
        videoMask1[0x11] = (byte)(videoMask1[0x10] ^ videoMask1[0x07]);
        videoMask1[0x12] = (byte)(videoMask1[0x0F] ^ 0xFF);
        videoMask1[0x13] = (byte)(videoMask1[0x03] ^ 0x10);
        videoMask1[0x14] = (byte)(videoMask1[0x04] - 0x32);
        videoMask1[0x15] = (byte)(videoMask1[0x05] + 0xED);
        videoMask1[0x16] = (byte)(videoMask1[0x06] ^ 0xF3);
        videoMask1[0x17] = (byte)(videoMask1[0x13] - videoMask1[0x0F]);
        videoMask1[0x18] = (byte)(videoMask1[0x15] + videoMask1[0x07]);
        videoMask1[0x19] = (byte)(0x21 - videoMask1[0x13]);
        videoMask1[0x1A] = (byte)(videoMask1[0x14] ^ videoMask1[0x17]);
        videoMask1[0x1B] = (byte)(videoMask1[0x16] + videoMask1[0x16]);
        videoMask1[0x1C] = (byte)(videoMask1[0x17] + 0x44);
        videoMask1[0x1D] = (byte)(videoMask1[0x03] + videoMask1[0x04]);
        videoMask1[0x1E] = (byte)(videoMask1[0x05] - videoMask1[0x16]);
        videoMask1[0x1F] = (byte)(videoMask1[0x1D] ^ videoMask1[0x13]);
        for (int i = 0; i < 0x20; i++)
        {
            videoMask2[i] = (byte)(videoMask1[i] ^ 0xFF);
            audioMask[i] = (byte)((i & 1) == 1 ? "URUC"u8[i >> 1 & 3] : videoMask1[i] ^ 0xFF);
        }
    }
    [Benchmark]
    public void Optmized() // about 7 ns on [.NET 10.0.2, X64 RyuJIT x86-64-v3]
    {
        ulong key = _key;
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
    public void SetKey(ulong key)
    {
        _key = key;
        Span<byte> keyBytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(keyBytes, _key);
        keyBytes[..4].CopyTo(_key1);
        keyBytes[4..].CopyTo(_key2);
    }
    public ReadOnlySpan<byte> GetVideoMask1() => _videoMask1;
    public ReadOnlySpan<byte> GetVideoMask2() => _videoMask2;
    public ReadOnlySpan<byte> GetAudioMask() => _audioMask;
    public bool ValidateHashCode(ulong key)
    {
        SetKey(key);
        Original();
        HashCode h = new();
        h.AddBytes(GetVideoMask1());
        h.AddBytes(GetVideoMask2());
        h.AddBytes(GetAudioMask());
        int a = h.ToHashCode();
        SetKey(key);
        Optmized();
        h = new();
        h.AddBytes(GetVideoMask1());
        h.AddBytes(GetVideoMask2());
        h.AddBytes(GetAudioMask());
        if (a != h.ToHashCode())
            return false;
        return true;
    }
}

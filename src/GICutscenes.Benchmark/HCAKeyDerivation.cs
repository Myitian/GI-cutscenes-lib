using BenchmarkDotNet.Attributes;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GICutscenes.Benchmark;

[MediumRunJob]
[MeanColumn]
[DisassemblyDiagnoser]
[MarkdownExporter]
public class HCAKeyDerivation
{
    [InlineArray(0x100)]
    private struct CiphTable
    {
        private byte _;
    }
    private ulong _key;
    private CiphTable _ciphTable;
    [GlobalSetup]
    public void GlobalSetup() => SetKey((ulong)Random.Shared.NextInt64());
    [Benchmark(Baseline = true)]
    public void Original() // about 420 ns on [.NET 10.0.2, X64 RyuJIT x86-64-v3]
    {
        // Note: Compared to the real original algorithm, array allocations has been changed to stack allocations.
        ulong key = _key;
        Span<byte> ciphTable = _ciphTable;
        Span<byte> t1 = stackalloc byte[8];
        uint key1 = (uint)key;
        uint key2 = (uint)(key >> 32);

        if (key1 == 0)
            key2--;
        key1--;
        for (int i = 0; i < 7; i++)
        {
            t1[i] = (byte)key1;
            key1 = (key1 >> 8) | (key2 << 24);
            key2 >>= 8;
        }
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
        InitTempTable(t1[0], t31);
        for (int i = 0; i < 0x10; i++)
        {
            InitTempTable(t2[i], t32);
            byte v = (byte)(t31[i] << 4);
            int index = 0;
            foreach (byte j in t32)
            {
                t3[i * 0x10 + index] = (byte)(v | j);
                index++;
            }
        }
        int iTable = 1;
        for (int i = 0, v = 0; i < 0x100; i++)
        {
            v = v + 0x11 & 0xFF;
            byte a = t3[v];
            if (a != 0 && a != 0xFF) ciphTable[iTable++] = a;
        }
        ciphTable[0] = 0;
        ciphTable[0xFF] = 0xFF;

        static void InitTempTable(byte key, Span<byte> table)
        {
            int mul = ((key & 1) << 3) | 5;
            int add = (key & 0xE) | 1;
            key >>= 4;
            for (int i = 0; i < table.Length; i++)
            {
                key = (byte)(key * mul + add & 0xF);
                table[i] = key;
            }
        }
    }
    [Benchmark]
    [SkipLocalsInit]
    public void Optmized() // about 300 ns on [.NET 10.0.2, X64 RyuJIT x86-64-v3]
    {
        Span<byte> ciphTable = _ciphTable;
        Span<byte> t1 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(t1, _key - 1);
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
    public void SetKey(ulong key) => _key = key;
    public ReadOnlySpan<byte> GetCiphTable() => _ciphTable;
    public bool ValidateHashCode(ulong key)
    {
        SetKey(key);
        Original();
        HashCode h = new();
        h.AddBytes(GetCiphTable());
        int a = h.ToHashCode();
        SetKey(key);
        Optmized();
        h = new();
        h.AddBytes(GetCiphTable());
        if (a != h.ToHashCode())
            return false;
        return true;
    }
}

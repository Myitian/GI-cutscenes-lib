using BenchmarkDotNet.Running;
using GICutscenes.Benchmark;

USMKeyDerivation usm = new();
HCAKeyDerivation hca = new();
for (int i = 0; i < 0x100; i++)
{
    ulong key = (ulong)Random.Shared.NextInt64();
    if (!usm.ValidateHashCode(key))
    {
        Console.WriteLine($"Failed to pass test 0x{key:X16} on USM");
        return;
    }
    if (!hca.ValidateHashCode(key))
    {
        Console.WriteLine($"Failed to pass test 0x{key:X16} on HCA");
        return;
    }
}
BenchmarkRunner.Run<USMKeyDerivation>(args: args);
BenchmarkRunner.Run<HCAKeyDerivation>(args: args);
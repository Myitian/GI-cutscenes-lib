namespace GICutscenes;

public static class KeyUtils
{
    public static ulong GetEncryptionKey(ReadOnlySpan<char> name)
    {
        ReadOnlySpan<char> rawKey = name is "MDAQ001_OPNew_Part1"
                                         or "MDAQ001_OPNew_Part2_PlayerBoy"
                                         or "MDAQ001_OPNew_Part2_PlayerGirl" ?
                                         "MDAQ001_OP" : name;
        ulong key = 0;
        foreach (char c in rawKey)
            key = c + 3 * key;
        key &= 0xFFFFFFFFFFFFFF;
        return key == 0 ? 0x100000000000000 : key;
    }
    public static ulong GetEncryptionKey(ReadOnlySpan<char> name, ulong key2)
    {
        ulong key = (GetEncryptionKey(name) + key2) & 0xFFFFFFFFFFFFFF;
        return key == 0 ? 0x100000000000000 : key;
    }
}
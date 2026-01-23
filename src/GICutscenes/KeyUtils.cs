namespace GICutscenes;

public static class KeyUtils
{
    public static ulong GetEncryptionKey(ReadOnlySpan<char> name, bool autoRenameKey = true)
    {
        ReadOnlySpan<char> rawKey = autoRenameKey && name
            is "MDAQ001_OPNew_Part1"
            or "MDAQ001_OPNew_Part2_PlayerBoy"
            or "MDAQ001_OPNew_Part2_PlayerGirl" ? "MDAQ001_OP" : name;
        ulong key = 0;
        foreach (char c in rawKey)
            key = key * 3 + c;
        return key;
    }
}
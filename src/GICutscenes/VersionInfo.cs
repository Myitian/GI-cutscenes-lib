using System.Text.Json.Serialization;

namespace GICutscenes;

#pragma warning disable CA1002,CA2227
public sealed class VersionInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    [JsonPropertyName("videos")]
    public List<string>? Videos { get; set; }
    [JsonPropertyName("videoGroups")]
    public List<VersionInfo>? VideoGroups { get; set; }
    [JsonPropertyName("key")]
    public ulong? Key { get; set; }
    [JsonPropertyName("encAudio")]
    public bool? EncAudio { get; set; }

    private static void FlattenCore(
        VersionInfo version,
        Action<string, ulong, bool> map,
        ulong key,
        bool encAudio)
    {
        ulong newKey = version.Key ?? key;
        bool newEncAudio = version.EncAudio ?? encAudio;
        if (version.Videos is { Count: > 0 })
        {
            foreach (string video in version.Videos)
            {
                if (video is not null)
                    map(video, newKey, newEncAudio);
            }
        }
        if (version.VideoGroups is { Count: > 0 })
            Flatten(version.VideoGroups, map, newKey, newEncAudio);
    }
    public static void Flatten(
        IEnumerable<VersionInfo> versions,
        Action<string, ulong, bool> map,
        ulong key = 0,
        bool encAudio = false)
    {
        ArgumentNullException.ThrowIfNull(versions);
        ArgumentNullException.ThrowIfNull(map);
        foreach (VersionInfo version in versions)
        {
            if (version is not null)
                FlattenCore(version, map, key, encAudio);
        }
    }
    public static void Flatten(
        scoped ReadOnlySpan<VersionInfo> versions,
        Action<string, ulong, bool> map,
        ulong key = 0,
        bool encAudio = false)
    {
        ArgumentNullException.ThrowIfNull(map);
        foreach (VersionInfo version in versions)
        {
            if (version is not null)
                FlattenCore(version, map, key, encAudio);
        }
    }
}
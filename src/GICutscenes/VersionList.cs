using System.Text.Json.Serialization;

namespace GICutscenes;

#pragma warning disable CA1002,CA2227
public sealed class VersionList
{
    [JsonPropertyName("list")]
    public List<VersionInfo>? List { get; set; }
}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GICutscenes.CLI;

static partial class Program
{
    private static FrozenDictionary<string, ulong> VersionMap = FrozenDictionary<string, ulong>.Empty;
    private static ILogger Logger = NullLogger.Instance;
    private static int Main()
    {
        Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
        using (ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()))
            Logger = loggerFactory.CreateLogger("Main");

        string versionJson = Path.Combine(AppContext.BaseDirectory, "versions.json");
        if (!File.Exists(versionJson))
        {
            Events.LogFileNotFound.InvokeLog(Logger, versionJson);
            return 1;
        }
        using (FileStream versionsStream = File.OpenRead(versionJson))
        {
            VersionList? versions = JsonSerializer.Deserialize(versionsStream, VersionJson.Default.VersionList);
            if (versions?.List is not { Count: > 0 })
            {
                Events.LogNoVersionInfoLoaded.InvokeLog(Logger);
                return 1;
            }
            else
            {
                Dictionary<string, ulong> map = [];
                VersionInfo.Flatten(versions.List, (name, key, _) => map[name] = key);
                VersionMap = map.ToFrozenDictionary();
            }
        }

        KeyMode keyMode = KeyMode.Mixed;

        Console.WriteLine("USM file/folder:");
        string usm = Console.ReadLine().AsSpan().Trim().Trim('"').ToString();
        Console.WriteLine("Output folder:");
        string output = Console.ReadLine().AsSpan().Trim().Trim('"').ToString();
        Directory.CreateDirectory(output);
        if (Directory.Exists(usm))
        {
            foreach (string usmFile in Directory.EnumerateFiles(usm, "*.usm", new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false,
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple
            }))
                ProcessUSM(usmFile, output, keyMode);
        }
        else
        {
            ProcessUSM(usm, output, keyMode);
        }
        return 0;
    }

    private static bool ProcessUSM(
        string file,
        string outputDir = "./",
        KeyMode keyMode = KeyMode.Mixed,
        bool validateExtension = true)
    {
        Events.LogProcessFile.InvokeLog(Logger, file);
        if (validateExtension && !Path.GetExtension(file.AsSpan()).Equals(".usm", StringComparison.OrdinalIgnoreCase))
        {
            Events.LogNotUSM.InvokeLog(Logger);
            return false;
        }
        string usmName = Path.GetFileNameWithoutExtension(file);
        ulong key1 = 0, key2 = 0;
        if (keyMode.HasFlag(KeyMode.FromExternal) && !VersionMap.TryGetValue(usmName, out key2))
        {
            Events.LogNoVersionInfoFor.InvokeLog(Logger, usmName);
            return false;
        }
        if (keyMode.HasFlag(KeyMode.FromName))
            key1 = KeyUtils.GetEncryptionKey(usmName);
        ulong key = KeyUtils.MergeEncryptionKey(key1, key2);
        Events.LogReadFile.InvokeLog(Logger, file);
        bool result;
        try
        {
            using FileStream usmStream = File.OpenRead(file);
            result = Context.TryDemuxAndDecrypt(key, usmStream, VideoOutputFactory, AudioOutputFactory, Logger);
        }
        catch (Exception ex)
        {
            Events.LogUSMDemuxFailed.InvokeLog(Logger, ex);
            return false;
        }
        if (!result)
        {
            Events.LogUSMDemuxFailed.InvokeLog(Logger);
            return false;
        }
        Events.LogDone.InvokeLog(Logger);
        return true;

        Stream VideoOutputFactory()
        {
            string ivfFile = Path.Combine(outputDir, $"{usmName}.ivf");
            Events.LogCreateFile.InvokeLog(Logger, ivfFile);
            return File.Open(ivfFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
        Stream AudioOutputFactory(byte chNo)
        {
            string hcaFile = Path.Combine(outputDir, $"{usmName}.{chNo}.hca");
            Events.LogCreateFile.InvokeLog(Logger, hcaFile);
            return File.Open(hcaFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(VersionList))]
    sealed partial class VersionJson : JsonSerializerContext;

    [Flags]
    enum KeyMode
    {
        None = 0,
        FromExternal = 0b01,
        FromName = 0b10,
        Mixed = FromExternal | FromName,
    }
}
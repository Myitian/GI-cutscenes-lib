using GICutscenes.FileTypes;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;

namespace GICutscenes.CLI;

sealed class DemuxContext
{
    // manually flattened some lambdas' captures
    private readonly Func<byte, Stream> _audioOutputFactory;
    private readonly ILogger? _logger;
    private readonly List<Task<bool>> _tasks = [];
    private readonly HCA _hca;
    private DemuxContext(Func<byte, Stream> audioOutputFactory, ILogger? logger, ulong key)
    {
        _audioOutputFactory = audioOutputFactory;
        _logger = logger;
        _hca = new(key);
    }
    public static bool TryDemuxAndDecrypt(
        ulong key,
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null)
    {
        USM usm = new(key);
        if (audioOutputFactory is null)
            return usm.TryDemux(input, videoOutputFactory, null, logger);
        DemuxContext ctx = new(audioOutputFactory, logger, key);
        bool demuxResult = usm.TryDemux(input, videoOutputFactory, ctx.AudioOutputFactory, logger);
        bool[] results = Task.WhenAll(ctx._tasks).Result;
        return demuxResult && Array.TrueForAll(results, Identity);
    }
    private static T Identity<T>(T value) => value;
    private Stream AudioOutputFactory(byte chNo)
    {
        AnonymousPipeServerStream pipeServer = new(PipeDirection.Out);
        AnonymousPipeClientStream pipeClient = new(PipeDirection.In, pipeServer.ClientSafePipeHandle);
        _tasks.Add(DecryptAsync(chNo, pipeClient));
        return pipeServer;
    }
    private async Task<bool> DecryptAsync(byte chNo, AnonymousPipeClientStream pipeClient)
    {
        bool result;
        try
        {
            using (pipeClient)
            {
                using Stream hcaStream = _audioOutputFactory(chNo);
                result = await _hca
                    .TryDecryptAsync(pipeClient, hcaStream, _logger)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Events.LogHCADecryptFailed.InvokeLog(_logger, chNo, ex);
            return false;
        }
        if (!result)
        {
            Events.LogHCADecryptFailed.InvokeLog(_logger, chNo);
            return false;
        }
        return true;
    }
}
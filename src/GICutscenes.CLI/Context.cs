using GICutscenes.FileTypes;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;

namespace GICutscenes.CLI;

sealed class Context
{
    // manually flattened some lambdas' captures
    private readonly Stream _input;
    private readonly Func<Stream>? _videoOutputFactory;
    private readonly Func<byte, Stream>? _audioOutputFactory;
    private readonly ILogger? _logger;
    private readonly List<Task<bool>> tasks = [];
    private readonly USM _usm;
    private readonly HCA _hca;

    private Context(
        ulong key,
        Stream input,
        Func<Stream>? videoOutputFactory,
        Func<byte, Stream>? audioOutputFactory,
        ILogger? logger)
    {
        _input = input;
        _videoOutputFactory = videoOutputFactory;
        _audioOutputFactory = audioOutputFactory;
        _logger = logger;
        _usm = new(key);
        _hca = new(key);
    }

    public static bool TryDemuxAndDecrypt(
        ulong key,
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null)
    {
        if (audioOutputFactory is null)
            return new USM(key).TryDemux(input, videoOutputFactory, null, logger);
        return new Context(key, input, videoOutputFactory, audioOutputFactory, logger).TryDemuxAndDecrypt();
    }
    private bool TryDemuxAndDecrypt()
    {
        bool demuxResult = _usm.TryDemux(_input, _videoOutputFactory, AudioOutputFactory, _logger);
        bool[] results = Task.WhenAll(tasks).Result;
        return demuxResult && Array.TrueForAll(results, Identity);
    }
    private Stream AudioOutputFactory(byte chNo)
    {
        AnonymousPipeServerStream pipeServer = new(PipeDirection.Out);
        AnonymousPipeClientStream pipeClient = new(PipeDirection.In, pipeServer.GetClientHandleAsString());
        tasks.Add(Task.Run(DecryptHCA));
        return pipeServer;

        bool DecryptHCA()
        {
            bool result;
            try
            {
                using (pipeClient)
                {
                    using Stream hcaStream = _audioOutputFactory!(chNo);
                    result = _hca.TryDecrypt(pipeClient, hcaStream, _logger);
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

    public static Task<bool> TryDemuxAndDecryptAsync(
        ulong key,
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (audioOutputFactory is null)
            return new USM(key).TryDemuxAsync(input, videoOutputFactory, null, logger, cancellationToken);
        return new Context(key, input, videoOutputFactory, audioOutputFactory, logger).TryDemuxAndDecryptAsyncCore(cancellationToken);
    }
    private async Task<bool> TryDemuxAndDecryptAsyncCore(CancellationToken cancellationToken)
    {
        bool demuxResult = await _usm
            .TryDemuxAsync(_input, _videoOutputFactory, AudioOutputFactory, _logger, cancellationToken)
            .ConfigureAwait(false);
        bool[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return demuxResult && Array.TrueForAll(results, Identity);

        Stream AudioOutputFactory(byte chNo)
        {
            AnonymousPipeServerStream pipeServer = new(PipeDirection.Out);
            AnonymousPipeClientStream pipeClient = new(PipeDirection.In, pipeServer.GetClientHandleAsString());
            tasks.Add(DecryptHCAAsync(chNo, pipeClient, cancellationToken));
            return pipeServer;
        }
    }
    private async Task<bool> DecryptHCAAsync(byte chNo, AnonymousPipeClientStream pipeClient, CancellationToken cancellationToken)
    {
        bool result;
        try
        {
            using (pipeClient)
            {
                using Stream hcaStream = _audioOutputFactory!(chNo);
                result = await _hca
                    .TryDecryptAsync(pipeClient, hcaStream, _logger, cancellationToken)
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
    private static T Identity<T>(T value) => value;
}
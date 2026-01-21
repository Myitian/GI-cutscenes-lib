using GICutscenes.CLI;
using GICutscenes.FileTypes;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;

#pragma warning disable CA1031
sealed class CachedKey(ulong key)
{
    public USM USM = new(key);
    public HCA HCA = new(key);

    public bool TryDemuxAndDecrypt(
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null)
    {
        if (audioOutputFactory is null)
            return USM.TryDemux(input, videoOutputFactory, null, logger);

        List<Task<bool>> tasks = [];
        bool demuxResult = USM.TryDemux(input, videoOutputFactory, AudioOutputFactory, logger);
        bool[] results = Task.WhenAll(tasks).Result;
        return demuxResult && Array.TrueForAll(results, static it => it);

        Stream AudioOutputFactory(byte chNo)
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
                        using Stream hcaStream = audioOutputFactory(chNo);
                        result = HCA.TryDecrypt(pipeClient, hcaStream, logger);
                    }
                }
                catch (Exception ex)
                {
                    Events.LogHCADecryptFailed.InvokeLog(logger, chNo, ex);
                    return false;
                }
                if (!result)
                {
                    Events.LogHCADecryptFailed.InvokeLog(logger, chNo);
                    return false;
                }
                return true;
            }
        }
    }
    public Task<bool> TryDemuxAndDecryptAsync(
        Stream input,
        Func<Stream>? videoOutputFactory = null,
        Func<byte, Stream>? audioOutputFactory = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        if (audioOutputFactory is null)
            return USM.TryDemuxAsync(input, videoOutputFactory, null, logger, cancellationToken);
        else
            return TryDemuxAndDecryptAsyncCore(this, input, videoOutputFactory, audioOutputFactory, logger, cancellationToken);

        static async Task<bool> TryDemuxAndDecryptAsyncCore(
            CachedKey cachedKey,
            Stream input,
            Func<Stream>? videoOutputFactory,
            Func<byte, Stream> audioOutputFactory,
            ILogger? logger,
            CancellationToken cancellationToken = default)
        {
            List<Task<bool>> tasks = [];
            bool demuxResult = await cachedKey.USM
                .TryDemuxAsync(input, videoOutputFactory, AudioOutputFactory, logger, cancellationToken)
                .ConfigureAwait(false);
            bool[] results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return demuxResult && Array.TrueForAll(results, static it => it);

            Stream AudioOutputFactory(byte chNo)
            {
                AnonymousPipeServerStream pipeServer = new(PipeDirection.Out);
                AnonymousPipeClientStream pipeClient = new(PipeDirection.In, pipeServer.GetClientHandleAsString());
                tasks.Add(DecryptHCA(cachedKey, audioOutputFactory, logger, chNo, pipeClient, cancellationToken));
                return pipeServer;

                static async Task<bool> DecryptHCA(
                    CachedKey cachedKey,
                    Func<byte, Stream> audioOutputFactory,
                    ILogger? logger,
                    byte chNo,
                    AnonymousPipeClientStream pipeClient,
                    CancellationToken cancellationToken)
                {
                    bool result;
                    try
                    {
                        using (pipeClient)
                        {
                            using Stream hcaStream = audioOutputFactory(chNo);
                            result = await cachedKey.HCA
                                .TryDecryptAsync(pipeClient, hcaStream, logger, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Events.LogHCADecryptFailed.InvokeLog(logger, chNo, ex);
                        return false;
                    }
                    if (!result)
                    {
                        Events.LogHCADecryptFailed.InvokeLog(logger, chNo);
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}
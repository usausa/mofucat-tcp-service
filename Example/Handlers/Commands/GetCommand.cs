namespace Example.Handlers.Commands;

using System.Buffers;
using System.Runtime.CompilerServices;

using Example.Handlers;
using Example.Service;

public sealed class GetCommand : ICommand
{
    private readonly FeatureService featureService;

    public GetCommand(FeatureService featureService)
    {
        this.featureService = featureService;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(in ReadOnlySequence<byte> command) => command.SequentialEqual("get"u8);

    public ValueTask<bool> ExecuteAsync(in ReadOnlySequence<byte> options, IBufferWriter<byte> writer)
    {
        var value = featureService.QueryFeature();
        writer.WriteAndAdvanceOk(value ? "on"u8 : "off"u8);

        return ValueTask.FromResult(true);
    }
}

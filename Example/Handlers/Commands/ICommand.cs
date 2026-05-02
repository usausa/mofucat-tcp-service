namespace Example.Handlers.Commands;

using System.Buffers;

public interface ICommand
{
    bool Match(in ReadOnlySequence<byte> command);

    ValueTask<bool> ExecuteAsync(in ReadOnlySequence<byte> options, IBufferWriter<byte> writer);
}

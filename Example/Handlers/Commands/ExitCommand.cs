namespace Example.Handlers.Commands;

using System.Buffers;
using System.Runtime.CompilerServices;

using Example.Handlers;

public sealed class ExitCommand : ICommand
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Match(in ReadOnlySequence<byte> command) => command.SequentialEqual("exit"u8);

    public ValueTask<bool> ExecuteAsync(in ReadOnlySequence<byte> options, IBufferWriter<byte> writer) => ValueTask.FromResult(false);
}

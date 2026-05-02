namespace Example.Handlers;

using System.Buffers;
using System.Runtime.CompilerServices;

public static class CommandExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAndAdvanceOk(this IBufferWriter<byte> writer)
    {
        "ok\r\n"u8.CopyTo(writer.GetSpan(4));
        writer.Advance(4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAndAdvanceOk(this IBufferWriter<byte> writer, ReadOnlySpan<byte> option)
    {
        var total = 3 + option.Length + 2;
        var span = writer.GetSpan(total);
        "ok "u8.CopyTo(span);
        option.CopyTo(span[3..]);
        "\r\n"u8.CopyTo(span[(3 + option.Length)..]);
        writer.Advance(total);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteAndAdvanceNg(this IBufferWriter<byte> writer)
    {
        "ng\r\n"u8.CopyTo(writer.GetSpan(4));
        writer.Advance(4);
    }
}

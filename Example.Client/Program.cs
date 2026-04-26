using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 && Int32.TryParse(args[1], out var parsedPort) ? parsedPort : 18888;
var useTls = args.Length > 2 && String.Equals(args[2], "tls", StringComparison.OrdinalIgnoreCase);

using var client = new TcpClient();
await client.ConnectAsync(host, port);

await using Stream stream = useTls
    ? await CreateTlsStreamAsync(client, host).ConfigureAwait(false)
    : client.GetStream();

using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
{
    NewLine = "\r\n",
    AutoFlush = true
};

Console.WriteLine($"Connected to {host}:{port} ({(useTls ? "TLS" : "TCP")})");
Console.WriteLine("Commands: get, set 1, set 0, exit");

while (true)
{
    Console.Write("> ");
    var line = Console.ReadLine();
    if (String.IsNullOrWhiteSpace(line))
    {
        continue;
    }

    await writer.WriteLineAsync(line).ConfigureAwait(false);

    if (String.Equals(line, "exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var response = await reader.ReadLineAsync().ConfigureAwait(false);
    Console.WriteLine(response);
}

static async Task<SslStream> CreateTlsStreamAsync(TcpClient client, string host)
{
    var sslStream = new SslStream(
        client.GetStream(),
        leaveInnerStreamOpen: false,
        static (_, _, _, _) => true);

    await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
    {
        TargetHost = host,
        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        RemoteCertificateValidationCallback = static (_, _, _, _) => true
    }).ConfigureAwait(false);

    return sslStream;
}

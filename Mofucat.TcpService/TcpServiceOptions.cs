namespace Mofucat.TcpService;

using System.Diagnostics.CodeAnalysis;
using System.Net;

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;

public sealed class TcpServiceOptions
{
    private readonly KestrelServerOptions serverOptions;

    public SocketTransportOptions TransportOptions { get; }

    public bool GracefulShutdown { get; set; }

    internal TcpServiceOptions(KestrelServerOptions serverOptions, SocketTransportOptions transportOptions)
    {
        this.serverOptions = serverOptions;
        TransportOptions = transportOptions;
    }

    public void Listen<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IPAddress address, int port)
        where T : ConnectionHandler
    {
        serverOptions.Listen(address, port, static config => ConfigureListenOptions<T>(config));
    }

    public void Listen<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IPEndPoint endPoint)
        where T : ConnectionHandler
    {
        serverOptions.Listen(endPoint, static config => ConfigureListenOptions<T>(config));
    }

    public void ListenLocalhost<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(int port)
        where T : ConnectionHandler
    {
        serverOptions.ListenLocalhost(port, static config => ConfigureListenOptions<T>(config));
    }

    public void ListenAnyIP<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(int port)
        where T : ConnectionHandler
    {
        serverOptions.ListenAnyIP(port, static config => ConfigureListenOptions<T>(config));
    }

    private static void ConfigureListenOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(ListenOptions config)
        where T : ConnectionHandler
    {
        config.Protocols = HttpProtocols.None;
        config.UseConnectionHandler<T>();
    }
}

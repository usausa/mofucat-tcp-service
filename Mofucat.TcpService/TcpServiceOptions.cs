namespace Mofucat.TcpService;

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

    /// <summary>Listens on the specified IP address and port using the specified connection handler.</summary>
    public void Listen<T>(IPAddress address, int port)
        where T : ConnectionHandler =>
        Listen<T>(address, port, static _ => { });

    /// <summary>Listens on the specified IP address and port using the specified connection handler.</summary>
    public void Listen<T>(IPAddress address, int port, Action<ListenOptions> configure)
        where T : ConnectionHandler
    {
        serverOptions.Listen(address, port, config => ConfigureListenOptions<T>(config, configure));
    }

    /// <summary>Listens on the specified endpoint using the specified connection handler.</summary>
    public void Listen<T>(IPEndPoint endPoint)
        where T : ConnectionHandler =>
        Listen<T>(endPoint, static _ => { });

    /// <summary>Listens on the specified endpoint using the specified connection handler.</summary>
    public void Listen<T>(IPEndPoint endPoint, Action<ListenOptions> configure)
        where T : ConnectionHandler
    {
        serverOptions.Listen(endPoint, config => ConfigureListenOptions<T>(config, configure));
    }

    /// <summary>Listens on localhost using the specified connection handler.</summary>
    public void ListenLocalhost<T>(int port)
        where T : ConnectionHandler =>
        ListenLocalhost<T>(port, static _ => { });

    /// <summary>Listens on localhost using the specified connection handler.</summary>
    public void ListenLocalhost<T>(int port, Action<ListenOptions> configure)
        where T : ConnectionHandler
    {
        serverOptions.ListenLocalhost(port, config => ConfigureListenOptions<T>(config, configure));
    }

    /// <summary>Listens on any IP address using the specified connection handler.</summary>
    public void ListenAnyIP<T>(int port)
        where T : ConnectionHandler =>
        ListenAnyIP<T>(port, static _ => { });

    /// <summary>Listens on any IP address using the specified connection handler.</summary>
    public void ListenAnyIP<T>(int port, Action<ListenOptions> configure)
        where T : ConnectionHandler
    {
        serverOptions.ListenAnyIP(port, config => ConfigureListenOptions<T>(config, configure));
    }

    private static void ConfigureListenOptions<T>(ListenOptions config, Action<ListenOptions> configure)
        where T : ConnectionHandler
    {
        config.Protocols = HttpProtocols.None;
        configure(config);
        config.UseConnectionHandler<T>();
    }
}

namespace Mofucat.TcpService;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CA1812
internal sealed class TcpService : IHostedService, IDisposable
{
    private readonly KestrelServer kestrelServer;

    private readonly bool gracefulShutdown;

    public TcpService(IServiceProvider serviceProvider, IEnumerable<Action<TcpServiceOptions>> registrations)
    {
        var serverOptions = new KestrelServerOptions
        {
            ApplicationServices = serviceProvider
        };
        var transportOptions = new SocketTransportOptions();

        var options = new TcpServiceOptions(serverOptions, transportOptions);
        foreach (var registration in registrations)
        {
            ArgumentNullException.ThrowIfNull(registration);
            registration(options);
        }

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var transportFactory = new SocketTransportFactory(new OptionsWrapper<SocketTransportOptions>(transportOptions), loggerFactory);
        kestrelServer = new KestrelServer(new OptionsWrapper<KestrelServerOptions>(serverOptions), transportFactory, loggerFactory);

        gracefulShutdown = options.GracefulShutdown;
    }

    public void Dispose() => kestrelServer.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) =>
        kestrelServer.StartAsync(NullHttpApplication.Instance, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) =>
        gracefulShutdown ? kestrelServer.StopAsync(cancellationToken) : kestrelServer.StopAsync(new CancellationToken(true));
}
#pragma warning restore CA1812

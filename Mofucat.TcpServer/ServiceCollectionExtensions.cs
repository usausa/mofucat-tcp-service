namespace Mofucat.TcpServer;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>Adds a Kestrel-based TCP server as a hosted service.</summary>
    public static IServiceCollection AddTcpServer(this IServiceCollection services, Action<TcpServerOptions> options)
    {
        services.AddSingleton(options);
        services.AddHostedService<TcpServerService>();
        return services;
    }
}

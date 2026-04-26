namespace Mofucat.TcpService;

using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>Adds a Kestrel-based TCP server as a hosted service.</summary>
    public static IServiceCollection AddTcpService(this IServiceCollection services, Action<TcpServiceOptions> options)
    {
        services.AddSingleton(options);
        services.AddHostedService<TcpService>();
        return services;
    }
}

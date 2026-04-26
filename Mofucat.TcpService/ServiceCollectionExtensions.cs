namespace Mofucat.TcpService;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    /// <summary>Adds a Kestrel-based TCP server as a hosted service.</summary>
    public static IServiceCollection AddTcpService(this IServiceCollection services, Action<TcpServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TcpService>());
        return services;
    }
}

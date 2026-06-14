namespace Mofucat.TcpService;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

internal sealed class NullHttpApplication : IHttpApplication<object>
{
    public static readonly NullHttpApplication Instance = new();

    private NullHttpApplication()
    {
    }

    public object CreateContext(IFeatureCollection contextFeatures) => throw CreateNotSupported();

    public Task ProcessRequestAsync(object context) => throw CreateNotSupported();

    public void DisposeContext(object context, Exception? exception)
    {
    }

    private static InvalidOperationException CreateNotSupported() =>
        new("TcpService does not support HTTP.");
}

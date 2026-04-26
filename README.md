# Mofucat.TcpService

[![NuGet](https://img.shields.io/nuget/v/Mofucat.TcpService.svg)](https://www.nuget.org/packages/Mofucat.TcpService)

Kestrel-based TCP server library.

## Quick start

### Implement a `ConnectionHandler`

```csharp
using Microsoft.AspNetCore.Connections;

public sealed class SampleHandler : ConnectionHandler
{
    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        var writer = connection.Transport.Output;
        "ok\r\n"u8.CopyTo(writer.GetSpan(4));
        writer.Advance(4);
        await writer.FlushAsync();
    }
}
```

### Register the TCP service

```csharp
using Mofucat.TcpService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888);
});

builder.Build().Run();
```

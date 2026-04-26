# Mofucat.TcpService

`Mofucat.TcpService` is a small library that hosts a Kestrel-based TCP server as an `IHostedService` in a .NET application.

[![NuGet](https://img.shields.io/nuget/v/Mofucat.TcpService.svg)](https://www.nuget.org/packages/Mofucat.TcpService)

## Overview

`Mofucat.TcpService` uses Kestrel connection handlers to host raw TCP endpoints in a Generic Host or Worker Service.

It is intended for scenarios such as:

- adding a custom TCP protocol listener to a Worker Service
- implementing a lightweight TCP server with `ConnectionHandler`
- managing the TCP server lifecycle through the standard hosting model

## Installation

```powershell
dotnet add package Mofucat.TcpService
```

## Quick start

### 1. Implement a `ConnectionHandler`

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

### 2. Register the TCP service

```csharp
using Mofucat.TcpService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888);
});

builder.Build().Run();
```

## Multiple registrations

`AddTcpService()` can be called multiple times. All registrations are combined and applied to the same hosted TCP service.

```csharp
builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<PublicHandler>(18888);
});

builder.Services.AddTcpService(static options =>
{
    options.Listen<AdminHandler>(System.Net.IPAddress.Loopback, 18889);
});
```

## Protocol support

This library targets raw TCP only.

- HTTP is not supported
- HTTP/1.1, HTTP/2, and HTTP/3 are not public configuration targets
- the library forces `HttpProtocols.None`

The public API is intentionally narrowed so callers cannot enable unrelated Kestrel HTTP settings.

## TCP-focused configuration

The exposed configuration is limited to TCP-relevant options.

```csharp
builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888);
});
```

`TransportOptions` is available for socket transport tuning, but endpoint registration no longer accepts arbitrary `ListenOptions` callbacks.

## SSL/TLS support

This library does not expose TLS listener support.

If SSL/TLS support is needed, it should be implemented explicitly in application code as raw TCP stream handling, for example with `SslStream`, rather than by enabling Kestrel HTTPS features.

## Sample

The `Example` project shows a simple command-based TCP server over plain TCP.

```csharp
using Example.Handlers;
using Example.Handlers.Commands;
using Example.Service;

using Mofucat.TcpService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888);
});
builder.Services.AddSingleton<ICommand, ExitCommand>();
builder.Services.AddSingleton<ICommand, GetCommand>();
builder.Services.AddSingleton<ICommand, SetCommand>();

builder.Services.AddSingleton<FeatureService>();

builder.Build().Run();
```

- Plain TCP endpoint: `0.0.0.0:18888`

## Client sample

The `Example.Client` project is a console client. Its TLS mode is a standalone `SslStream` example for raw TCP, not a feature provided by this library.

```powershell
dotnet run --project Example.Client -- 127.0.0.1 18888
```

The sample protocol supports:

- `get`
- `set 1`
- `set 0`
- `exit`

Example responses:

- `ok\r\n`
- `ok on\r\n`
- `ok off\r\n`
- `ng\r\n`

## API

### `AddTcpService`

Registers the hosted TCP service and appends the provided configuration.

### `TcpServiceOptions`

`TcpServiceOptions` exposes:

- `Listen<T>(IPAddress address, int port)`
- `Listen<T>(IPEndPoint endPoint)`
- `ListenLocalhost<T>(int port)`
- `ListenAnyIP<T>(int port)`
- `TransportOptions`
- `GracefulShutdown`

All `Listen*` methods require `T : ConnectionHandler`.

### `TransportOptions`

`TransportOptions` exposes `SocketTransportOptions` directly.

```csharp
builder.Services.AddTcpService(static options =>
{
    options.TransportOptions.IOQueueCount = Environment.ProcessorCount;
    options.ListenLocalhost<SampleHandler>(18888);
});
```

### `GracefulShutdown`

The default value is `false`.

```csharp
builder.Services.AddTcpService(static options =>
{
    options.GracefulShutdown = true;
    options.ListenAnyIP<SampleHandler>(18888);
});
```

Set it to `true` to use graceful shutdown behavior during host stop.

## Requirements

- .NET 10
- Environment with `Microsoft.AspNetCore.App`

## Build

```powershell
dotnet build
```

## License

See the license file in this repository.

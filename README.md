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
    options.ListenLocalhost<AdminHandler>(18889);
});
```

## Per-endpoint configuration

Each `Listen*` method has an overload that accepts `Action<ListenOptions>`.

```csharp
builder.Services.AddTcpService(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888, static listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.None;
    });
});
```

This allows additional endpoint-level configuration while still binding the specified `ConnectionHandler`.

## Sample

The `Example` project shows a simple command-based TCP server.

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
builder.Services.AddTcpService(static options =>
{
    options.ListenLocalhost<SampleHandler>(18889, static listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.None;
    });
});
builder.Services.AddSingleton<ICommand, ExitCommand>();
builder.Services.AddSingleton<ICommand, GetCommand>();
builder.Services.AddSingleton<ICommand, SetCommand>();

builder.Services.AddSingleton<FeatureService>();

builder.Build().Run();
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
- `Listen<T>(IPAddress address, int port, Action<ListenOptions> configure)`
- `Listen<T>(IPEndPoint endPoint)`
- `Listen<T>(IPEndPoint endPoint, Action<ListenOptions> configure)`
- `ListenLocalhost<T>(int port)`
- `ListenLocalhost<T>(int port, Action<ListenOptions> configure)`
- `ListenAnyIP<T>(int port)`
- `ListenAnyIP<T>(int port, Action<ListenOptions> configure)`
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

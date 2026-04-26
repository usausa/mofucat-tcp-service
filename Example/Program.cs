using Example.Handlers;
using Example.Handlers.Commands;
using Example.Service;

using Mofucat.TcpService;

var builder = Host.CreateApplicationBuilder(args);

// TCP Server
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

// Service
builder.Services.AddSingleton<FeatureService>();

builder.Build().Run();

using Example.Handlers;
using Example.Handlers.Commands;
using Example.Service;
using Mofucat.TcpServer;

var builder = Host.CreateApplicationBuilder(args);

// TCP Server
builder.Services.AddTcpServer(static options =>
{
    options.ListenAnyIP<SampleHandler>(18888);
});
builder.Services.AddSingleton<ICommand, ExitCommand>();
builder.Services.AddSingleton<ICommand, GetCommand>();
builder.Services.AddSingleton<ICommand, SetCommand>();

// Service
builder.Services.AddSingleton<FeatureService>();

builder.Build().Run();

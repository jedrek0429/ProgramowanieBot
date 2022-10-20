﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProgramowanieBot;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddSingleton<TokenService>()
            .AddSingleton<BotService>()
            .AddHostedService(s => s.GetRequiredService<BotService>())
            .AddSingleton<ApplicationCommandService>()
            .AddHostedService(s => s.GetRequiredService<ApplicationCommandService>());
});
var host = builder.Build();
await host.RunAsync();

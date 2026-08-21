using PackControl.Edge.Configuration;
using PackControl.Edge.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<EdgeOptions>(builder.Configuration.GetSection("Edge"));
builder.Services.AddSingleton<LocalSpoolWriter>();
builder.Services.AddHostedService<DirectoryWatchWorker>();

var host = builder.Build();
host.Run();

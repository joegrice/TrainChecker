using TrainChecker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));
builder.Services.AddHttpClient<NationalRailService>();

var host = builder.Build();
await host.RunAsync();

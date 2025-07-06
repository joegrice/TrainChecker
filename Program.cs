using TrainChecker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<NationalRailService>();
builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/trains", async (NationalRailService service) =>
{
    var trains = await service.GetTrainStatusAsync(DateTime.Now.ToString("HH:mm"));
    return Results.Ok(trains);
});

app.Run();
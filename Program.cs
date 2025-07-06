using Quartz;
using TrainChecker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<NationalRailService>();
builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("TrainCheckJob");
    q.AddJob<TrainCheckJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("TrainCheckJob-trigger")
        .WithCronSchedule("0 30 7 ? * MON-FRI *"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/trains", async (NationalRailService service) =>
{
    var trains = await service.GetTrainStatusAsync(DateTime.Now.ToString("HH:mm"));
    return Results.Ok(trains);
});

app.Run();

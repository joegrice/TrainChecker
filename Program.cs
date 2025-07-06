using System.Text;
using Quartz;
using TrainChecker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<NationalRailService>();
builder.Services.AddHttpClient<TelegramService>();
builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));
builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

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
app.MapGet("/trains", async (NationalRailService nationalRailService, TelegramService telegramService) =>
{
    var huxleyResponse = await nationalRailService.GetTrainStatusAsync(DateTime.Now.ToString("HH:mm"));
    if (huxleyResponse?.TrainServices != null)
    {
        var message = new StringBuilder();
        message.AppendLine("*Train Status Update*");
        foreach (var trainService in huxleyResponse.TrainServices)
        {
            var origin = trainService.Origin?.FirstOrDefault()?.LocationName ?? "Unknown";
            var destination = trainService.Destination?.FirstOrDefault()?.LocationName ?? "Unknown";
            message.AppendLine($"- *{trainService.ScheduledTimeOfDeparture}* from {origin} to {destination} is {trainService.EstimatedTimeOfDeparture}");
        }
        await telegramService.SendMessageAsync(message.ToString());
    }
    return Results.Ok(huxleyResponse);
});

app.Run();

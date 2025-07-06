using Quartz;
using TrainChecker.Configuration;
using TrainChecker.Jobs;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<INationalRailService, NationalRailService>();
builder.Services.AddHttpClient<ITelegramService, TelegramService>();
builder.Services.AddScoped<TrainChecker.Services.Train.ITrainService, TrainChecker.Services.Train.TrainService>();
builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));
builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

builder.Services.AddQuartz(q =>
{
    var forwardJobKey = new JobKey("TrainCheckJobForward");
    q.AddJob<TrainCheckJob>(opts => opts
        .WithIdentity(forwardJobKey)
        .UsingJobData("DepartureStation", builder.Configuration["TrainChecker:DepartureStation"])
        .UsingJobData("ArrivalStation", builder.Configuration["TrainChecker:ArrivalStation"]));
    q.AddTrigger(opts => opts
        .ForJob(forwardJobKey)
        .WithIdentity("TrainCheckJobForward-trigger")
        .WithCronSchedule("0 30 7 ? * MON-FRI *"));

    var reverseJobKey = new JobKey("TrainCheckJobReverse");
    q.AddJob<TrainCheckJob>(opts => opts
        .WithIdentity(reverseJobKey)
        .UsingJobData("DepartureStation", builder.Configuration["TrainChecker:ArrivalStation"])
        .UsingJobData("ArrivalStation", builder.Configuration["TrainChecker:DepartureStation"]));
    q.AddTrigger(opts => opts
        .ForJob(reverseJobKey)
        .WithIdentity("TrainCheckJobReverse-5pm-trigger")
        .WithCronSchedule("0 0 17 ? * MON-FRI *"));
    q.AddTrigger(opts => opts
        .ForJob(reverseJobKey)
        .WithIdentity("TrainCheckJobReverse-530pm-trigger")
        .WithCronSchedule("0 30 17 ? * MON-FRI *"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();

namespace TrainChecker
{
    public partial class Program { }
}

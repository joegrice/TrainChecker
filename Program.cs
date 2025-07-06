using Quartz;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Options;
using TrainChecker.Jobs;

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
app.MapControllers();

app.Run();

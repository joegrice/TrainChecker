using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Quartz;
using TrainChecker.Configuration;
using TrainChecker.Extensions;
using TrainChecker.Helpers;
using TrainChecker.Jobs;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Services.Train;
using TrainChecker.Swagger;

namespace TrainChecker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers();
        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Train Checker API",
                Description = "An ASP.NET Core Web API for checking train statuses."
            });
            options.OperationFilter<SwaggerDefaultValues>();
        });
                        
        // Configure options
        builder.Services.AddOptions<TrainCheckerOptions>()
            .BindConfiguration(TrainCheckerOptions.TrainChecker);
        builder.Services.AddOptions<TelegramOptions>()
            .BindConfiguration(TelegramOptions.Telegram);
        builder.Services.AddOptions<ScheduledJobOptions>()
            .BindConfiguration(ScheduledJobOptions.Quartz)
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // Configure HttpClient with base address from configuration
        builder.Services.AddHttpClient<INationalRailService, NationalRailService>((serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<TrainCheckerOptions>>().Value.BaseAddress);
        });
            
        builder.Services.AddHttpClient<ITelegramService, TelegramService>();
        builder.Services.AddScoped<ITrainService, TrainService>();
        
        builder.Services.AddQuartz(q =>
        {
            var trainCheckerOptions = builder.Configuration.GetRequiredSection(TrainCheckerOptions.TrainChecker)
                .Get<TrainCheckerOptions>();
            var scheduledJobOptions = builder.Configuration.GetRequiredSection(ScheduledJobOptions.Quartz)
                .Get<ScheduledJobOptions>();
            
            q.AddJobAndTriggers<TrainCheckJob>("TrainCheckJobForward", 
                trainCheckerOptions.DepartureStation,
                trainCheckerOptions.ArrivalStation, 
                scheduledJobOptions.Forward.Schedules);
            
            q.AddJobAndTriggers<TrainCheckJob>("TrainCheckJobReverse", 
                trainCheckerOptions.ArrivalStation,
                trainCheckerOptions.DepartureStation, 
                scheduledJobOptions.Reverse.Schedules);
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        // Send a startup notification
        var telegramService = app.Services.GetRequiredService<ITelegramService>();
        telegramService.SendMessageAsync($"{ApplicationVersion.Name} v{ApplicationVersion.Version} started");

        app.Run();
    }
}
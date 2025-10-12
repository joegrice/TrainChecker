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
using QuartzOptions = TrainChecker.Configuration.QuartzOptions;

namespace TrainChecker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure CORS options
        builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.Cors));
        var corsOptions = builder.Configuration.GetRequiredSection(CorsOptions.Cors).Get<CorsOptions>();
        
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: myAllowSpecificOrigins,
                policy =>
                {
                    if (corsOptions.AllowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(corsOptions.AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                });
        });

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
                        
        // Configure TrainChecker options first
        builder.Services.Configure<TrainCheckerOptions>(builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker));
        builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));
        builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection(QuartzOptions.Quartz));
            
        // Configure HttpClient with base address from configuration
        builder.Services.AddHttpClient<INationalRailService, NationalRailService>((serviceProvider, client) =>
        {
            var trainCheckerOptions = serviceProvider.GetRequiredService<IOptions<TrainCheckerOptions>>().Value;
            client.BaseAddress = new Uri(trainCheckerOptions.BaseAddress);
        });
            
        builder.Services.AddHttpClient<ITelegramService, TelegramService>();
        builder.Services.AddScoped<ITrainService, TrainService>();

        builder.Services.AddQuartz(q =>
        {
            var trainCheckerOptions = builder.Configuration.GetSection(TrainCheckerOptions.TrainChecker).Get<TrainCheckerOptions>();
            var quartzOptions = builder.Configuration.GetSection(QuartzOptions.Quartz).Get<QuartzOptions>();
            
            q.AddJobAndTriggers<TrainCheckJob>("TrainCheckJobForward", 
                trainCheckerOptions.DepartureStation,
                trainCheckerOptions.ArrivalStation, 
                quartzOptions.Forward.Schedules);
            
            q.AddJobAndTriggers<TrainCheckJob>("TrainCheckJobReverse", 
                trainCheckerOptions.ArrivalStation,
                trainCheckerOptions.DepartureStation, 
                quartzOptions.Reverse.Schedules);
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(myAllowSpecificOrigins);
        app.MapControllers();

        // Send a startup notification
        var telegramService = app.Services.GetRequiredService<ITelegramService>();
        telegramService.SendMessageAsync($"{ApplicationVersion.Name} v{ApplicationVersion.Version} started");

        app.Run();
    }
}
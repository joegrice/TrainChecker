using Quartz;
using TrainChecker.Configuration;
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

        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: myAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Allow your frontend origin
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
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
            
        // Configure HttpClient with base address from configuration
        builder.Services.AddHttpClient<INationalRailService, NationalRailService>((serviceProvider, client) =>
        {
            var trainCheckerOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<TrainCheckerOptions>>().Value;
            client.BaseAddress = new Uri(trainCheckerOptions.BaseAddress);
        });
            
        builder.Services.AddHttpClient<ITelegramService, TelegramService>();
        builder.Services.AddScoped<ITrainService, TrainService>();

        builder.Services.AddQuartz(q =>
        {
            var forwardJobKey = new JobKey("TrainCheckJobForward");
            q.AddJob<TrainCheckJob>(opts => opts
                .WithIdentity(forwardJobKey)
                .UsingJobData("DepartureStation", builder.Configuration["TrainChecker:DepartureStation"])
                .UsingJobData("ArrivalStation", builder.Configuration["TrainChecker:ArrivalStation"]));
            q.AddTrigger(opts => opts
                .ForJob(forwardJobKey)
                .WithIdentity("TrainCheckJobForward-730am-trigger")
                .WithCronSchedule("0 30 7 ? * MON-FRI *"));
            q.AddTrigger(opts => opts
                .ForJob(forwardJobKey)
                .WithIdentity("TrainCheckJobForward-748am-trigger")
                .WithCronSchedule("0 48 7 ? * MON-FRI *"));

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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(myAllowSpecificOrigins);
        app.MapControllers();

        app.Run();
    }
}
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using TrainChecker.Configuration;
using TrainChecker.Jobs;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Services.Train;
using TrainChecker.Swagger;
using Microsoft.EntityFrameworkCore;
using TrainChecker.Data;

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
        builder.Services.AddScoped<ITelegramValidationService, TelegramValidationService>(); // Register new service
        builder.Services.AddScoped<ITrainService, TrainService>();

        if (!builder.Environment.IsEnvironment("IntegrationTests"))
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
                        ValidAudience = builder.Configuration["Jwt:Audience"]!,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });
        }

        if (!builder.Environment.IsEnvironment("IntegrationTests"))
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

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
                .WithIdentity("TrainCheckJobForward-745am-trigger")
                .WithCronSchedule("0 45 7 ? * MON-FRI *"));

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
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors(myAllowSpecificOrigins);
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        app.MapGet("/Error", () => Results.Problem())
            .ExcludeFromDescription();

        // Apply migrations on startup, but not for integration tests
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!app.Environment.IsEnvironment("IntegrationTests"))
            {
                dbContext.Database.Migrate();
            }
        }

        app.Run();
    }
}

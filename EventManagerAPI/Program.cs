using EventManagerAPI.Extensions;
using EventManagerAPI.Services;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using Domain.Interfaces.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureUnitOfWork();
builder.Services.ConfigureService();
builder.Services.AddHttpClient();

AppSettingConfiguration appConfig = builder.Configuration.Get<AppSettingConfiguration>();
BackgroundConfig backgroundConfig = builder.Configuration.GetSection("BackgroundConfig").Get<BackgroundConfig>();

//Config caching
builder.Services.AddMemoryCache();

// using DI with config
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton<IJWTService, JWTService>();
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
// Register CustomHealthCheck
builder.Services.AddSingleton<IHealthCheck, OpenWeatherAPIHealthCheck>();
builder.Services.AddSingleton<IHealthCheck, DatabaseHealthCheck>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

//Config from appsetting.json
builder.Services.Configure<OpenWeatherAPIConfig>(builder.Configuration.GetSection("OpenWeatherAPIConfig"));
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = appConfig.JWTSection.Issuer,
            ValidAudience = appConfig.JWTSection.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.JWTSection.SecretKey)),
        };
    });

//Config Hangfire
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); // Configure Hangfire services https://docs.hangfire.io/
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// config the seri log
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

//Add healthcheck
builder.Services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                    healthQuery: "SELECT 1;",
                    name: "sql",
                    failureStatus: HealthStatus.Degraded,
                    tags: new string[] { "db", "sql", "sqlserver" })
                .AddCheck<OpenWeatherAPIHealthCheck>(
                    name: "openWeatheApi_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new string[] { "api", "OpenWeatherAPI"})
                .AddCheck<DatabaseHealthCheck>(
                    name: "database_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new string[] { "sql", "Database" });

//Add healthcheck ui
builder.Services.AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("WebApp_endpoint", "https://localhost:7296/webapp_health");
                    setup.SetEvaluationTimeInSeconds(10);
                    setup.SetMinimumSecondsBetweenFailureNotifications(60);
                })
                .AddInMemoryStorage();

//Add DB context
builder.Services.AddDbContext<EventContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Config swagger with jwt
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure Hangfire dashboard 
// Route to /hangfire to see dashboard
app.UseHangfireDashboard();
app.UseHangfireServer();
RecurringJob.AddOrUpdate<IBackgroundService>(backgroundConfig.JobId, x => x.ProcessEventRemindersAsync(), backgroundConfig.CronExpression);

//Config HealthCheck
app.UseHealthChecks("/healthcheck");
app.MapHealthChecks("/webapp_health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});
app.MapHealthChecksUI(u => u.UIPath = "/myfavorite-ui");


app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.UseAuthentication();

app.UseAuthorization();

app.Run();


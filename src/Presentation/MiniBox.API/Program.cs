using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MiniBox.API.Extensions;
using MiniBox.API.Hubs;
using MiniBox.API.Middlewares;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Application.Features;
using MiniBox.Domain.Entities.Orders;
using MiniBox.Domain.Events;
using MiniBox.Infrastructure;
using MiniBox.Infrastructure.Persistence;
using StackExchange.Redis;

if (args.Length == 1 && args[0] == "--health-check") Environment.Exit(await MiniBox.API.HealthCheck.RunAsync());

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey must be configured.");
builder.Services.AddInfrastructure(connectionString, redisConnection, jwtSigningKey);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplication();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = "MiniBox",
        ValidateAudience = true, ValidAudience = "MiniBox.Client",
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, MiniBox.API.Services.CurrentUserService>();
builder.Services.AddSingleton<ISignalRNotificationService, MiniBox.API.Services.SignalRNotificationService>();
builder.Services.AddTransient<INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>, MiniBox.API.Services.OrderCreatedDomainEventHandler>();
builder.Services.AddSingleton<MiniBox.API.Services.PerformanceMetrics>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration["Frontend:Origin"] ?? "http://localhost:8080")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddHealthChecks().AddNpgSql(connectionString);

var app = builder.Build();
await DatabaseInitializer.InitializeAsync(app.Services);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<PerformanceLoggingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/metrics", (MiniBox.API.Services.PerformanceMetrics metrics) => Results.Text(metrics.ToPrometheus(), "text/plain; version=0.0.4; charset=utf-8"));
app.MapControllers();
app.MapHub<PresenceHub>("/hubs/presence");
app.MapHub<StoreEventsHub>("/hubs/store-events");
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();

public partial class Program;

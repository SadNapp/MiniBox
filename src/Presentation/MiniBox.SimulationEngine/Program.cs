using Microsoft.EntityFrameworkCore;
using MiniBox.Infrastructure.Authentication;
using MiniBox.Infrastructure.Persistence;
using MiniBox.SimulationEngine.BackgroundWorkers;
using MiniBox.SimulationEngine.Generators;
using MiniBox.SimulationEngine.Models;
using MiniBox.SimulationEngine.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<SimulationSettings>(builder.Configuration.GetSection("Simulation"));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
    npgsql.ConfigureDataSource(dataSource => dataSource.EnableDynamicJson())));
builder.Services.AddSingleton<ProductGenerator>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<DataSeederService>();
builder.Services.AddHttpClient<SimulatedApiClient>(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddSingleton<UserBehaviorSimulator>();
builder.Services.AddSingleton<LiveCheckoutSimulator>();
builder.Services.AddHostedService<TrafficGeneratorWorker>();
builder.Services.AddHostedService<VirtualPresenceWorker>();

var host = builder.Build();
await host.RunAsync();

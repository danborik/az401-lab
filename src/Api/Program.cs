using System.Reflection;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Lab.Common;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// Lab 5.6 (SRE Agent): set app setting CRASH_ON_STARTUP=true to break the app on purpose.
if (string.Equals(builder.Configuration["CRASH_ON_STARTUP"], "true", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("CRASH_ON_STARTUP is true. Remove the app setting to recover.");
}

// Lab 3.10: Azure App Configuration + feature flags, only when an endpoint is configured.
// Without it, flags come from the "FeatureManagement" section of appsettings.json.
var appConfigEndpoint = builder.Configuration["AppConfig:Endpoint"];
var useAppConfig = !string.IsNullOrWhiteSpace(appConfigEndpoint);
if (useAppConfig)
{
    builder.Configuration.AddAzureAppConfiguration(options =>
        options.Connect(new Uri(appConfigEndpoint!), new DefaultAzureCredential())
               .UseFeatureFlags(flags => flags.SetRefreshInterval(TimeSpan.FromSeconds(30))));
    builder.Services.AddAzureAppConfiguration();
}

// Lab 5.3: Application Insights via OpenTelemetry, only when a connection string is configured.
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

builder.Services.AddFeatureManagement();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("proxy", client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddSingleton<GreetingService>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
if (useAppConfig)
{
    app.UseAzureAppConfiguration();
}

var version = Assembly.GetEntryAssembly()?
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion ?? "unknown";

app.MapGet("/", () => Results.Ok(new
{
    service = "az401-lab",
    endpoints = new[] { "/health", "/version", "/greet?name=Dan", "/weather?city=Prague&days=5",
                        "/weather-proxy", "/slow?ms=2000", "/error", "/config" }
}));

// Health probe for App Service health check, slot smoke tests and load tests.
app.MapHealthChecks("/health");

// Shows which build is running; useful after slot swaps (lab 3.9).
app.MapGet("/version", (IWebHostEnvironment env) => Results.Ok(new
{
    version,
    environment = env.EnvironmentName,
    machine = Environment.MachineName
}));

// Feature flag demo (lab 3.10).
app.MapGet("/greet", async (string? name, IFeatureManager features, GreetingService greeter) =>
{
    var useNew = await features.IsEnabledAsync("NewGreeting");
    return Results.Ok(new { message = greeter.Greet(name, useNew), newGreeting = useNew });
});

// Main business endpoint. Chaos:FailRate (0..1) makes a share of calls fail, for alert labs (5.5-5.7).
app.MapGet("/weather", (string? city, int? days, IConfiguration config) =>
{
    var failRate = config.GetValue<double>("Chaos:FailRate");
    if (failRate > 0 && Random.Shared.NextDouble() < failRate)
    {
        return Results.Problem("Simulated failure (Chaos:FailRate).", statusCode: 500);
    }

    try
    {
        var forecast = WeatherForecaster.Forecast(city ?? "Prague", days ?? 5, DateOnly.FromDateTime(DateTime.UtcNow));
        return Results.Ok(forecast);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Outbound call, so Application Insights shows a dependency and a distributed trace (lab 5.3).
app.MapGet("/weather-proxy", async (IHttpClientFactory factory, IConfiguration config, CancellationToken ct) =>
{
    var target = config["Proxy:TargetUrl"] ?? "https://httpbin.org/delay/1";
    var client = factory.CreateClient("proxy");
    using var response = await client.GetAsync(target, ct);
    return Results.Ok(new { target, status = (int)response.StatusCode });
});

// Slow endpoint for performance analysis and load tests.
app.MapGet("/slow", async (int? ms, CancellationToken ct) =>
{
    var delay = Math.Clamp(ms ?? 2000, 0, 30000);
    await Task.Delay(delay, ct);
    return Results.Ok(new { delayedMs = delay });
});

// Always throws: generates exceptions and failed requests for monitoring labs.
app.MapGet("/error", IResult () =>
    throw new InvalidOperationException("Simulated failure for monitoring labs."));

// Shows config without leaking secrets (Key Vault / Key Vault reference labs 4.3).
app.MapGet("/config", (IConfiguration config) => Results.Ok(new
{
    message = config["Settings:Message"],
    dbPasswordConfigured = !string.IsNullOrEmpty(config["DbPassword"])
}));

app.Run();

// Makes Program visible to WebApplicationFactory in the integration tests.
public partial class Program { }

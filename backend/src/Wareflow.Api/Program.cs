using Serilog;
using Wareflow.Api.Endpoints;
using Wareflow.Api.ErrorHandling;
using Wareflow.Application;
using Wareflow.Infrastructure;
using Wareflow.Api.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<TenantContextMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapGet("/api/foundation/status", () => Results.Ok(new
{
    Status = "ok",
    UtcTimestamp = DateTimeOffset.UtcNow
}))
.WithName("FoundationStatus")
.WithTags("Foundation");

app.MapFoundationEndpoints();

app.MapGet("/api/foundation/tenant-settings", async (
    Wareflow.Application.Tenancy.ITenantFoundationStore store,
    CancellationToken cancellationToken) =>
{
    var settings = await store.GetSettingsAsync(cancellationToken);
    return settings is null ? Results.NotFound() : Results.Ok(settings);
})
.WithName("GetTenantSettings")
.WithTags("Foundation");

app.Run();

public partial class Program;

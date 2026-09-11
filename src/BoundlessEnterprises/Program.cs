using BoundlessEnterprises.Api.Common;
using BoundlessEnterprises.Api.Extensions;
using BoundlessEnterprises.Api.Middleware;
using BoundlessEnterprises.Application;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Allow larger uploads (audio stems, art, project files) — up to 100 MB.
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 104_857_600);

const string CorsPolicy = "BoundlessWeb";

// ---- Services ----------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000" };
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Serialize/deserialize enums as their string names in JSON.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---- Pipeline ----------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitializeDatabaseAsync();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapEndpointModules();

app.Run();

/// <summary>Exposed so WebApplicationFactory can bootstrap the API in integration tests.</summary>
public partial class Program;

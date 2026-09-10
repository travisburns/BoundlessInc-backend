using BoundlessEnterprises.Api.Common;
using BoundlessEnterprises.Api.Extensions;
using BoundlessEnterprises.Api.Middleware;
using BoundlessEnterprises.Application;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

// Authentication schemes (JWT bearer) are configured in Phase 1; registering the
// core services here satisfies the auto-added auth middleware.
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

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
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapEndpointModules();

app.Run();

/// <summary>Exposed so WebApplicationFactory can bootstrap the API in integration tests.</summary>
public partial class Program;

using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WhatLeft.Api.Modules.Tasks;
using WhatLeft.Api.OpenApi;
using WhatLeft.Tasks.Application;
using WhatLeft.Tasks.Infrastructure;
using WhatLeft.Tasks.Infrastructure.Jobs;
using WhatLeft.Tasks.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Auth ─────────────────────────────────────────────────────────────────────
// JWT validation delegated entirely to Auth0 — no auth logic in the app.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
    });

builder.Services.AddAuthorization();

// ── JSON ──────────────────────────────────────────────────────────────────────
// Serialize/deserialize enums as strings (e.g. "Weekly" instead of 0)
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── Modules ───────────────────────────────────────────────────────────────────
// To add a new module: builder.Services.AddHabitsApplication(); etc.
builder.Services.AddTasksApplication();
builder.Services.AddTasksInfrastructure(builder.Configuration);

// ── Hangfire ──────────────────────────────────────────────────────────────────
// Uses the same PostgreSQL DB as the Tasks module (dedicated "hangfire" schema).
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Tasks"))));

builder.Services.AddHangfireServer();

// ── OpenAPI (native .NET 10 + Scalar UI) ─────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// ── DB: run pending migrations on startup ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    db.Database.Migrate();
}

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // OpenAPI spec: GET /openapi/v1.json
    app.MapOpenApi();
    // Scalar UI: http://localhost:5000/scalar/v1
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes("Auth0")
        .AddAuthorizationCodeFlow("Auth0", flow =>
        {
            flow.ClientId = builder.Configuration["Auth0:ClientId"] ?? string.Empty;
            flow.Pkce = Pkce.Sha256;
        })
        .WithPersistentAuthentication()
    );
    // Hangfire dashboard: http://localhost:5000/hangfire (dev only)
    app.MapHangfireDashboard("/hangfire");
}

// ── Hangfire recurring jobs ───────────────────────────────────────────────────
var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobs.AddOrUpdate<RecurringTaskJob>(
    RecurringTaskJob.JobId,
    job => job.ProcessAsync(),
    Cron.Daily()); // Chaque jour à 00:00 UTC — idempotent par période

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapGet("/", () => new { status = "ok", message = "WhatLeft API is running" });
app.MapTasksEndpoints();
app.MapRecurringTaskTemplatesEndpoints();
// To add a new module: app.MapHabitsEndpoints(); etc.

app.Run();

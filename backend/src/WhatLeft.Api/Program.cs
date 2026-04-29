using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WhatLeft.Api.Modules.Tasks;
using WhatLeft.Api.OpenApi;
using WhatLeft.Tasks.Application;
using WhatLeft.Tasks.Infrastructure;
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
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapGet("/", () => new { status = "ok", message = "WhatLeft API is running" });
app.MapTasksEndpoints();
// To add a new module: app.MapHabitsEndpoints(); etc.

app.Run();

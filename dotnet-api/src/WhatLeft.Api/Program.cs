using Microsoft.AspNetCore.Authentication.JwtBearer;
using WhatLeft.Api.Modules.Tasks;
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

// ── OpenAPI ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        [new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});

var app = builder.Build();

// ── DB: auto-create schema on startup (use EF migrations for production) ──────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    db.Database.EnsureCreated();
}

// ── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapGet("/", () => new { status = "ok", message = "WhatLeft API is running" });
app.MapTasksEndpoints();
// To add a new module: app.MapHabitsEndpoints(); etc.

app.Run();

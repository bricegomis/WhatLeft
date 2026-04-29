using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WhatLeft.Tasks.Domain.Entities;

namespace WhatLeft.Tasks.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext scoped to the Tasks module.
/// HasDefaultSchema("tasks") isolates this module's tables — transparent on SQLite,
/// enforced as a real schema on PostgreSQL when migrating to production.
/// </summary>
public sealed class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <summary>
    /// SQLite does not support DateTimeOffset natively in ORDER BY / WHERE clauses.
    /// This convention maps every DateTimeOffset property to a long (Unix ms UTC),
    /// which sorts correctly and is fully portable to PostgreSQL later.
    /// Remove this override once migrated to PostgreSQL.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // DateTimeOffset → long (Unix milliseconds, UTC)
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToTicksConverter>();

        // DateTimeOffset? → long?
        configurationBuilder
            .Properties<DateTimeOffset?>()
            .HaveConversion<DateTimeOffsetToTicksConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Schema isolation: each module owns its schema (no cross-module FK)
        modelBuilder.HasDefaultSchema("tasks");

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Duration).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();

            // Tags stored as comma-separated string (simple, avoids join table for now)
            entity.Property(t => t.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Length == 0
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // DomainEvents is a transient in-memory collection, never persisted
            entity.Ignore(t => t.DomainEvents);
        });
    }
}

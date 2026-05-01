using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<RecurringTemplate> RecurringTemplates => Set<RecurringTemplate>();

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
            entity.Property(t => t.CancelledAt);
            entity.Property(t => t.RecurringTemplateId);
            entity.Property(t => t.PeriodStart);

            // Tags stored as comma-separated string (simple, avoids join table for now)
            entity.Property(t => t.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Length == 0
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new ValueComparer<List<string>>(
                        (a, b) => a != null && b != null && a.SequenceEqual(b),
                        v => v.Aggregate(0, (a, s) => HashCode.Combine(a, s.GetHashCode())),
                        v => v.ToList()));

            // FK to RecurringTemplate (nullable — manual tasks have no template)
            entity.HasOne<RecurringTemplate>()
                .WithMany()
                .HasForeignKey(t => t.RecurringTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            // DomainEvents is a transient in-memory collection, never persisted
            entity.Ignore(t => t.DomainEvents);
        });

        modelBuilder.Entity<RecurringTemplate>(entity =>
        {
            entity.ToTable("recurring_templates");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).ValueGeneratedNever();
            entity.Property(t => t.Title).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Duration).IsRequired();
            entity.Property(t => t.RecurrenceType).HasConversion<string>().IsRequired();
            entity.Property(t => t.FrequencyPerPeriod).IsRequired();
            entity.Property(t => t.ResetHour).IsRequired();
            entity.Property(t => t.IsActive).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Ignore(t => t.Tags); // computed from TagsRaw
        });
    }
}

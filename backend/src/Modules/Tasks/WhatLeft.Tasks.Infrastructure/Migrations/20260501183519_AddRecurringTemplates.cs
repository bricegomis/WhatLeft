using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatLeft.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                schema: "tasks",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PeriodStart",
                schema: "tasks",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecurringTemplateId",
                schema: "tasks",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "recurring_templates",
                schema: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    TagsRaw = table.Column<string>(type: "text", nullable: true),
                    RecurrenceType = table.Column<string>(type: "text", nullable: false),
                    FrequencyPerPeriod = table.Column<int>(type: "integer", nullable: false),
                    ResetHour = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_templates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_RecurringTemplateId",
                schema: "tasks",
                table: "tasks",
                column: "RecurringTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_recurring_templates_RecurringTemplateId",
                schema: "tasks",
                table: "tasks",
                column: "RecurringTemplateId",
                principalSchema: "tasks",
                principalTable: "recurring_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_recurring_templates_RecurringTemplateId",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.DropTable(
                name: "recurring_templates",
                schema: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_RecurringTemplateId",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "RecurringTemplateId",
                schema: "tasks",
                table: "tasks");
        }
    }
}

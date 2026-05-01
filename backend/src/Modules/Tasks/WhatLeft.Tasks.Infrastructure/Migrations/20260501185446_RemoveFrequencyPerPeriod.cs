using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatLeft.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFrequencyPerPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrequencyPerPeriod",
                schema: "tasks",
                table: "recurring_templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FrequencyPerPeriod",
                schema: "tasks",
                table: "recurring_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

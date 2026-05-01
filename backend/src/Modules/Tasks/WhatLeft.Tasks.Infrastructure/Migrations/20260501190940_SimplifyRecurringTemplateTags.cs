using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatLeft.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRecurringTemplateTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagsRaw",
                schema: "tasks",
                table: "recurring_templates");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "tasks",
                table: "recurring_templates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "tasks",
                table: "recurring_templates");

            migrationBuilder.AddColumn<string>(
                name: "TagsRaw",
                schema: "tasks",
                table: "recurring_templates",
                type: "text",
                nullable: true);
        }
    }
}

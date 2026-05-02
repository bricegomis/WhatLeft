using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatLeft.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "tasks",
                table: "tasks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "tasks",
                table: "recurring_templates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "tasks",
                table: "recurring_templates");
        }
    }
}

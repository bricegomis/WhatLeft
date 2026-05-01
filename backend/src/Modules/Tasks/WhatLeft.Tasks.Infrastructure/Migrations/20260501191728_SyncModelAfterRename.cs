using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatLeft.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_recurring_templates_RecurringTemplateId",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "RecurringTemplateId",
                schema: "tasks",
                table: "tasks",
                newName: "RecurringTaskTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_RecurringTemplateId",
                schema: "tasks",
                table: "tasks",
                newName: "IX_tasks_RecurringTaskTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_recurring_templates_RecurringTaskTemplateId",
                schema: "tasks",
                table: "tasks",
                column: "RecurringTaskTemplateId",
                principalSchema: "tasks",
                principalTable: "recurring_templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_recurring_templates_RecurringTaskTemplateId",
                schema: "tasks",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "RecurringTaskTemplateId",
                schema: "tasks",
                table: "tasks",
                newName: "RecurringTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_RecurringTaskTemplateId",
                schema: "tasks",
                table: "tasks",
                newName: "IX_tasks_RecurringTemplateId");

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
    }
}

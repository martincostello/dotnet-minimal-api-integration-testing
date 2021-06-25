// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoApp.Data
{
    [DbContext(typeof(TodoContext))]
    [Migration(nameof(TodoMigration))]
    public sealed class TodoMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: nameof(TodoContext.Items),
                columns: (table) => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(nullable: true),
                },
                constraints: (table) => table.PrimaryKey($"PK_{nameof(TodoContext.Items)}", (p) => p.Id));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.DropTable(name: nameof(TodoContext.Items));

        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 128);
            modelBuilder.Entity(
                typeof(TodoItem).Name,
                b =>
                {
                    b.Property<Guid>(nameof(TodoItem.Id)).ValueGeneratedOnAdd();
                    b.Property<string>(nameof(TodoItem.UserId));
                    b.Property<string>(nameof(TodoItem.Text));
                    b.Property<DateTime>(nameof(TodoItem.CreatedAt));
                    b.Property<DateTime?>(nameof(TodoItem.CompletedAt));
                    b.HasKey(nameof(TodoItem.Id));
                    b.ToTable(nameof(TodoContext.Items));
                });
        }
    }
}

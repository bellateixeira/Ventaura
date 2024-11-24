﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ventaura_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedColtoUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaxDistance",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDistance",
                table: "Users");
        }
    }
}

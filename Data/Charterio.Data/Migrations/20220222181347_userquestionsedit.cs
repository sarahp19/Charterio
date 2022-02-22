﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charterio.Data.migrations
{
    public partial class userquestionsedit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "UserQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "UserQuestions");
        }
    }
}

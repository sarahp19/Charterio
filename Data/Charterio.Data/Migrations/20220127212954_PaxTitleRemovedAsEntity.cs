﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charterio.Data.Migrations
{
    public partial class PaxTitleRemovedAsEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketPassangers_PaxTitles_PaxTitleId",
                table: "TicketPassangers");

            migrationBuilder.DropTable(
                name: "PaxTitles");

            migrationBuilder.DropIndex(
                name: "IX_TicketPassangers_PaxTitleId",
                table: "TicketPassangers");

            migrationBuilder.DropColumn(
                name: "PaxTitleId",
                table: "TicketPassangers");

            migrationBuilder.AddColumn<string>(
                name: "PaxTitle",
                table: "TicketPassangers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaxTitle",
                table: "TicketPassangers");

            migrationBuilder.AddColumn<int>(
                name: "PaxTitleId",
                table: "TicketPassangers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaxTitles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaxTitles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketPassangers_PaxTitleId",
                table: "TicketPassangers",
                column: "PaxTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketPassangers_PaxTitles_PaxTitleId",
                table: "TicketPassangers",
                column: "PaxTitleId",
                principalTable: "PaxTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

﻿// <auto-generated/>
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charterio.Data.Migrations
{
    public partial class RenamingTableInDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketPassangers_Tickets_TicketId",
                table: "TicketPassangers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketPassangers",
                table: "TicketPassangers");

            migrationBuilder.RenameTable(
                name: "TicketPassangers",
                newName: "TicketPassengers");

            migrationBuilder.RenameIndex(
                name: "IX_TicketPassangers_TicketId",
                table: "TicketPassengers",
                newName: "IX_TicketPassengers_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketPassengers",
                table: "TicketPassengers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketPassengers_Tickets_TicketId",
                table: "TicketPassengers",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketPassengers_Tickets_TicketId",
                table: "TicketPassengers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketPassengers",
                table: "TicketPassengers");

            migrationBuilder.RenameTable(
                name: "TicketPassengers",
                newName: "TicketPassangers");

            migrationBuilder.RenameIndex(
                name: "IX_TicketPassengers_TicketId",
                table: "TicketPassangers",
                newName: "IX_TicketPassangers_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketPassangers",
                table: "TicketPassangers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketPassangers_Tickets_TicketId",
                table: "TicketPassangers",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

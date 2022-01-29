﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charterio.Data.Migrations
{
    public partial class TicketPaymentCanBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

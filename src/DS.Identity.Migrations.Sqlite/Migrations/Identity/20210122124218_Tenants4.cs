﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.Sqlite.Migrations.Identity
{
    public partial class Tenants4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "WebAuthnCredentials",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "WebAuthnCredentials",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.SqlServer.Migrations.Identity
{
    public partial class Tenants3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_WebAuthnCredentials_AdminKeyId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_AdminKeyId",
                table: "Tenants");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "WebAuthnCredentials",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminKeyId",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "WebAuthnCredentials",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AdminKeyId",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AdminKeyId",
                table: "Tenants",
                column: "AdminKeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_WebAuthnCredentials_AdminKeyId",
                table: "Tenants",
                column: "AdminKeyId",
                principalTable: "WebAuthnCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WebAuthnCredentials_Tenants_TenantId",
                table: "WebAuthnCredentials",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

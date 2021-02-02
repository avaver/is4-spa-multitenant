using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.Sqlite.Migrations.Identity
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

        protected override void Down(MigrationBuilder migrationBuilder)
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

using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.SqlServer.Migrations.Identity
{
    public partial class AuthMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetadataIcon",
                table: "WebAuthnCredentials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataName",
                table: "WebAuthnCredentials",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetadataIcon",
                table: "WebAuthnCredentials");

            migrationBuilder.DropColumn(
                name: "MetadataName",
                table: "WebAuthnCredentials");
        }
    }
}

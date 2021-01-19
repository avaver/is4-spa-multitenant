using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.SqlServer.Migrations.Identity
{
    public partial class ClinicAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClinicAdmin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClinicAdmin",
                table: "AspNetUsers");
        }
    }
}

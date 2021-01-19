using Microsoft.EntityFrameworkCore.Migrations;

namespace DS.Identity.Migrations.Sqlite.Migrations.Identity
{
    public partial class ClinicAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClinicAdmin",
                table: "AspNetUsers",
                type: "INTEGER",
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

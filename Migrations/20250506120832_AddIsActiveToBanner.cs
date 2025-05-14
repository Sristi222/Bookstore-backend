using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Try_application.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToBanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banners",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Banners");
        }
    }
}

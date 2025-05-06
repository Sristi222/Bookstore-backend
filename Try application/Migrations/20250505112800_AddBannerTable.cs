using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Try_application.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    SubTitle = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Author = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DiscountEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    DiscountStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Format = table.Column<string>(type: "text", nullable: true),
                    Genre = table.Column<string>(type: "text", nullable: true),
                    ISBN = table.Column<string>(type: "text", nullable: true),
                    IsAvailableInStore = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    OnSale = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    PublicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Publisher = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TotalSold = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });
        }
    }
}

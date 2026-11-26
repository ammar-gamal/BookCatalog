using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookCatalog.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverPhotoForBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverPhoto",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverPhoto",
                table: "Books");
        }
    }
}

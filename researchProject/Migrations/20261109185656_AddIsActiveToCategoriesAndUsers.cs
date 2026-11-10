using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace researchProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToCategoriesAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Researchers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ResearchCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CollegeCoAdmins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Researchers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ResearchCategories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CollegeCoAdmins");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}

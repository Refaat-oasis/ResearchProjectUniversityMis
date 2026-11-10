using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace researchProject.Migrations
{
    /// <inheritdoc />
    public partial class AddResearcherTypeToJudges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResearcherType",
                table: "Judges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearcherType",
                table: "Judges");
        }
    }
}

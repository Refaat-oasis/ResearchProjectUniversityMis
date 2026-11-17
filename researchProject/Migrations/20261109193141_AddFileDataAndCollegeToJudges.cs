using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace researchProject.Migrations
{
    /// <inheritdoc />
    public partial class AddFileDataAndCollegeToJudges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ResearchAttachments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "ResearchAttachments",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "Judges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "JudgeInvitations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Judges_CollegeId",
                table: "Judges",
                column: "CollegeId");

            migrationBuilder.CreateIndex(
                name: "IX_JudgeInvitations_CollegeId",
                table: "JudgeInvitations",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JudgeInvitations_Colleges_CollegeId",
                table: "JudgeInvitations",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Judges_Colleges_CollegeId",
                table: "Judges",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JudgeInvitations_Colleges_CollegeId",
                table: "JudgeInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Judges_Colleges_CollegeId",
                table: "Judges");

            migrationBuilder.DropIndex(
                name: "IX_Judges_CollegeId",
                table: "Judges");

            migrationBuilder.DropIndex(
                name: "IX_JudgeInvitations_CollegeId",
                table: "JudgeInvitations");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ResearchAttachments");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "ResearchAttachments");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "Judges");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "JudgeInvitations");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace researchProject.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalIdUniqueAndUserCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE AspNetUsers SET CreatedAt = GETUTCDATE() WHERE CreatedAt < '2000-01-01';");

            migrationBuilder.Sql(@"
                UPDATE Researchers
                SET NationalId = RIGHT(CONCAT('29900000000000', CAST(Id AS nvarchar(14))), 14)
                WHERE NationalId IS NULL OR LTRIM(RTRIM(NationalId)) = '';

                ;WITH DuplicateNationalIds AS (
                    SELECT Id, NationalId,
                           ROW_NUMBER() OVER (PARTITION BY NationalId ORDER BY Id) AS RowNum
                    FROM Researchers
                )
                UPDATE DuplicateNationalIds
                SET NationalId = RIGHT(CONCAT(LEFT(LTRIM(RTRIM(NationalId)), 10), RIGHT(CONCAT('0000', CAST(Id AS nvarchar(10))), 4)), 14)
                WHERE RowNum > 1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Researchers_NationalId",
                table: "Researchers",
                column: "NationalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Researchers_NationalId",
                table: "Researchers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");
        }
    }
}

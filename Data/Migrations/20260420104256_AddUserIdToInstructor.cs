using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ders_programi_yonetim_sistemi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Instructors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Instructors
                SET UserId = (
                    SELECT u.Id
                    FROM AspNetUsers u
                    WHERE u.InstructorId = Instructors.Id
                    ORDER BY u.Id
                    LIMIT 1
                )
                WHERE UserId IS NULL
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_UserId",
                table: "Instructors",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_AspNetUsers_UserId",
                table: "Instructors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_AspNetUsers_UserId",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_UserId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Instructors");
        }
    }
}

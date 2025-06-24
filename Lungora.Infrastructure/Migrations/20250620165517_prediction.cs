using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lungora.Migrations
{
    /// <inheritdoc />
    public partial class prediction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIResult",
                table: "UserAIResults");

            migrationBuilder.AlterColumn<int>(
                name: "Prediction",
                table: "UserAIResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Prediction",
                table: "UserAIResults",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AIResult",
                table: "UserAIResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

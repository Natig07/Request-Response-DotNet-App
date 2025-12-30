using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class AddReportAdditionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Communication",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRoutine",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RequestSender",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootCause",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SolmanReqNumber",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Communication",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsRoutine",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RequestSender",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RootCause",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "SolmanReqNumber",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reports");
        }
    }
}

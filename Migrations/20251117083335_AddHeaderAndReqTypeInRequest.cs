using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHeaderAndReqTypeInRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Header",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReqTypeId",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReqType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReqType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ReqTypeId",
                table: "Requests",
                column: "ReqTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_ReqType_ReqTypeId",
                table: "Requests",
                column: "ReqTypeId",
                principalTable: "ReqType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_ReqType_ReqTypeId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "ReqType");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ReqTypeId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Header",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ReqTypeId",
                table: "Requests");
        }
    }
}

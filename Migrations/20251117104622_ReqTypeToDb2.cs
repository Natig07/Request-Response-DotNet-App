using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class ReqTypeToDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_ReqType_ReqTypeId",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReqType",
                table: "ReqType");

            migrationBuilder.RenameTable(
                name: "ReqType",
                newName: "ReqTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReqTypes",
                table: "ReqTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_ReqTypes_ReqTypeId",
                table: "Requests",
                column: "ReqTypeId",
                principalTable: "ReqTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_ReqTypes_ReqTypeId",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReqTypes",
                table: "ReqTypes");

            migrationBuilder.RenameTable(
                name: "ReqTypes",
                newName: "ReqType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReqType",
                table: "ReqType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_ReqType_ReqTypeId",
                table: "Requests",
                column: "ReqTypeId",
                principalTable: "ReqType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateMg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Requests_RequestId",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "PlannedOperationTime",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReqPriorityId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReqTypeId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReqPriorityId",
                table: "Reports",
                column: "ReqPriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReqTypeId",
                table: "Reports",
                column: "ReqTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Priorities_ReqPriorityId",
                table: "Reports",
                column: "ReqPriorityId",
                principalTable: "Priorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReqTypes_ReqTypeId",
                table: "Reports",
                column: "ReqTypeId",
                principalTable: "ReqTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Requests_RequestId",
                table: "Reports",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Priorities_ReqPriorityId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReqTypes_ReqTypeId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Requests_RequestId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReqPriorityId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReqTypeId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PlannedOperationTime",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReqPriorityId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReqTypeId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Requests_RequestId",
                table: "Reports",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

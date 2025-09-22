using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovingFileFromRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_FileEntity_FileId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "FileEntity");

            migrationBuilder.DropIndex(
                name: "IX_Requests_FileId",
                table: "Requests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_FileId",
                table: "Requests",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_FileEntity_FileId",
                table: "Requests",
                column: "FileId",
                principalTable: "FileEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

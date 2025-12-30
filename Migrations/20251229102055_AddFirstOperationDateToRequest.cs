using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReqResApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstOperationDateToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstOperationDate",
                table: "Requests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstOperationDate",
                table: "Requests");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CargohubV2.Migrations
{
    /// <inheritdoc />
    public partial class database7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_Groups_ItemGroupId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_Lines_ItemLineId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_Types_ItemTypeId",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items_Types",
                table: "Items_Types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items_Lines",
                table: "Items_Lines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items_Groups",
                table: "Items_Groups");

            migrationBuilder.RenameTable(
                name: "Items_Types",
                newName: "ItemTypes");

            migrationBuilder.RenameTable(
                name: "Items_Lines",
                newName: "ItemLines");

            migrationBuilder.RenameTable(
                name: "Items_Groups",
                newName: "ItemGroups");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemTypes",
                table: "ItemTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemLines",
                table: "ItemLines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemGroups",
                table: "ItemGroups",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    User = table.Column<string>(type: "text", nullable: false),
                    Entity = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemGroups_ItemGroupId",
                table: "Items",
                column: "ItemGroupId",
                principalTable: "ItemGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemLines_ItemLineId",
                table: "Items",
                column: "ItemLineId",
                principalTable: "ItemLines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemTypes_ItemTypeId",
                table: "Items",
                column: "ItemTypeId",
                principalTable: "ItemTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemGroups_ItemGroupId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemLines_ItemLineId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemTypes_ItemTypeId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "LogEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemTypes",
                table: "ItemTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemLines",
                table: "ItemLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemGroups",
                table: "ItemGroups");

            migrationBuilder.RenameTable(
                name: "ItemTypes",
                newName: "Items_Types");

            migrationBuilder.RenameTable(
                name: "ItemLines",
                newName: "Items_Lines");

            migrationBuilder.RenameTable(
                name: "ItemGroups",
                newName: "Items_Groups");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items_Types",
                table: "Items_Types",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items_Lines",
                table: "Items_Lines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items_Groups",
                table: "Items_Groups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_Groups_ItemGroupId",
                table: "Items",
                column: "ItemGroupId",
                principalTable: "Items_Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_Lines_ItemLineId",
                table: "Items",
                column: "ItemLineId",
                principalTable: "Items_Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_Types_ItemTypeId",
                table: "Items",
                column: "ItemTypeId",
                principalTable: "Items_Types",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order_Disc.Migrations
{
    /// <inheritdoc />
    public partial class FixFileEntitySharedByUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileShares_Files_FileEntityId",
                table: "FileShares");

            migrationBuilder.DropIndex(
                name: "IX_FileShares_FileEntityId",
                table: "FileShares");

            migrationBuilder.DropColumn(
                name: "FileEntityId",
                table: "FileShares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileEntityId",
                table: "FileShares",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileShares_FileEntityId",
                table: "FileShares",
                column: "FileEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileShares_Files_FileEntityId",
                table: "FileShares",
                column: "FileEntityId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}

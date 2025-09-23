using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Api.Migrations
{
    /// <inheritdoc />
    public partial class addedrelationbetweenthewhoamiandusee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_WhoAmI_User_Id",
                table: "WhoAmI",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WhoAmI_User_Id",
                table: "WhoAmI");
        }
    }
}

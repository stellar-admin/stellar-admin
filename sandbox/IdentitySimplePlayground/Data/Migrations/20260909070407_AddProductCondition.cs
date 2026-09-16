using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentitySimplePlayground.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Products");
        }
    }
}

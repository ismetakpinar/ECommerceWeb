using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BackfillOrderDetailProductNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        UPDATE od
        SET od.ProductName = p.Name
        FROM OrderDetails od
        INNER JOIN Products p
            ON od.ProductId = p.ProductId
        WHERE od.ProductName = '';
    ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

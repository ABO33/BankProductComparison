using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Deposits",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Deposits",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Deposits",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.CreateTable(
                name: "DepositCatalogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BankName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DepositName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DepositType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractInterestDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InterestType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInterestCapitalized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InterestPayout = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsMonthlyCompounding = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Currency = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ForWho = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    MinAmountDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxTermMonths = table.Column<int>(type: "int", nullable: true),
                    MinTermMonths = table.Column<int>(type: "int", nullable: true),
                    ValidTermsDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OverdraftAllowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowsTopUp = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AdditionalConditions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositCatalogs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositCatalogs");

            migrationBuilder.InsertData(
                table: "Deposits",
                columns: new[] { "Id", "BankName", "Currency", "InterestRate", "MaxAmount", "MaxTermMonths", "MinAmount", "MinTermMonths", "Name", "TaxRate" },
                values: new object[,]
                {
                    { 1, "Bank A", "BGN", 1.5m, 100000m, 6, 5000m, 6, "Standard 6-Month", 10m },
                    { 2, "Bank B", "BGN", 2.0m, 50000m, 12, 1000m, 12, "Variable 12-Month", 10m },
                    { 3, "Bank C", "EUR", 1.8m, 200000m, 24, 2000m, 24, "Long-Term 24-Month", 10m }
                });
        }
    }
}

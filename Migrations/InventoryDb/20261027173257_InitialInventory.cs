using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.InventoryDb
{
    /// <inheritdoc />
    public partial class InitialInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    MinimumStock = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    PurchaseCost = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    SalePrice = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.CheckConstraint("CK_InventoryItems_AvailableQuantity_NonNegative", "[AvailableQuantity] >= 0");
                    table.CheckConstraint("CK_InventoryItems_ItemType", "[ItemType] IN ('Repuesto', 'Consumible')");
                    table.CheckConstraint("CK_InventoryItems_MinimumStock_NonNegative", "[MinimumStock] >= 0");
                    table.CheckConstraint("CK_InventoryItems_PurchaseCost_NonNegative", "[PurchaseCost] >= 0");
                    table.CheckConstraint("CK_InventoryItems_SalePrice_NonNegative", "[SalePrice] >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryItems_InventoryCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "InventoryCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentInventoryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(14,2)", nullable: false, computedColumnSql: "[Quantity] * [UnitPrice]", stored: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    InventoryProcessed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InventoryProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentInventoryItems", x => x.Id);
                    table.CheckConstraint("CK_AppointmentInventoryItems_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_AppointmentInventoryItems_UnitCost_NonNegative", "[UnitCost] >= 0");
                    table.CheckConstraint("CK_AppointmentInventoryItems_UnitPrice_NonNegative", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_AppointmentInventoryItems_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AppointmentInventoryItemId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.CheckConstraint("CK_InventoryMovements_MovementType", "[MovementType] IN ('Entrada', 'Salida', 'AjustePositivo', 'AjusteNegativo')");
                    table.CheckConstraint("CK_InventoryMovements_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_InventoryMovements_UnitCost_NonNegative", "[UnitCost] IS NULL OR [UnitCost] >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_AppointmentInventoryItems_AppointmentInventoryItemId",
                        column: x => x.AppointmentInventoryItemId,
                        principalTable: "AppointmentInventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInventoryItems_AppointmentId",
                table: "AppointmentInventoryItems",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInventoryItems_AppointmentId_InventoryProcessed",
                table: "AppointmentInventoryItems",
                columns: new[] { "AppointmentId", "InventoryProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInventoryItems_InventoryItemId",
                table: "AppointmentInventoryItems",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInventoryItems_UserId",
                table: "AppointmentInventoryItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_Name",
                table: "InventoryCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CategoryId",
                table: "InventoryItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Code",
                table: "InventoryItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Name",
                table: "InventoryItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_AppointmentId",
                table: "InventoryMovements",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_AppointmentInventoryItemId",
                table: "InventoryMovements",
                column: "AppointmentInventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryItemId",
                table: "InventoryMovements",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_MovementDate",
                table: "InventoryMovements",
                column: "MovementDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_UserId",
                table: "InventoryMovements",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "AppointmentInventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryCategories");
        }
    }
}

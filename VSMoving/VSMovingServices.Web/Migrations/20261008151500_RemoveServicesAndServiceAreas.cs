using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class RemoveServicesAndServiceAreas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ServiceAreas");

        migrationBuilder.DropTable(
            name: "Services");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ServiceAreas",
            columns: table => new
            {
                ServiceAreaId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AreaName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ServiceAreas", x => x.ServiceAreaId);
            });

        migrationBuilder.CreateTable(
            name: "Services",
            columns: table => new
            {
                ServiceId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "Moving"),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IconClass = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false, defaultValue: "fa-solid fa-house"),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                ServiceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ShowInFooter = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Services", x => x.ServiceId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ServiceAreas_IsActive_DisplayOrder",
            table: "ServiceAreas",
            columns: new[] { "IsActive", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Services_IsActive_Category_DisplayOrder",
            table: "Services",
            columns: new[] { "IsActive", "Category", "DisplayOrder" });
    }
}

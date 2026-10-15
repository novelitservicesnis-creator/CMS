using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class AddServicesAndServiceAreas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ServiceAreas",
            columns: table => new
            {
                ServiceAreaId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AreaName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
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
                ServiceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                IconClass = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false, defaultValue: "fa-solid fa-house"),
                Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "Moving"),
                DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
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

        migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM [Services])
            BEGIN
                INSERT INTO [Services] ([ServiceName], [ShortDescription], [IconClass], [Category], [DisplayOrder], [IsActive], [CreatedAt])
                VALUES
                    (N'Residential Moving', N'Apartments, condos, and family homes — packed and moved with care, top to bottom.', N'fa-solid fa-house', N'Moving', 1, 1, GETDATE()),
                    (N'Commercial Moving', N'Office and warehouse relocations planned around your business hours, not ours.', N'fa-solid fa-building', N'Moving', 2, 1, GETDATE()),
                    (N'Packing & Unpacking', N'Proper materials, labeled boxes, and a system so nothing gets lost in transit.', N'fa-solid fa-box-open', N'Moving', 3, 1, GETDATE()),
                    (N'Furniture Disassembly & Assembly', N'Beds, shelving, and awkward pieces taken apart and rebuilt at the new address.', N'fa-solid fa-screwdriver-wrench', N'Moving', 4, 1, GETDATE()),
                    (N'Loading & Unloading', N'Modern equipment and a trained crew make the heavy lifting look easy.', N'fa-solid fa-dolly', N'Moving', 5, 1, GETDATE()),
                    (N'Local Moving', N'Across town or across the Halifax area — one crew, one truck, one point of contact.', N'fa-solid fa-route', N'Moving', 6, 1, GETDATE()),
                    (N'Household Junk Removal', N'Old clutter, bags, boxes and general household items cleared out in one visit.', N'fa-solid fa-trash-can', N'Junk Removal', 1, 1, GETDATE()),
                    (N'Furniture & Appliance Removal', N'Couches, mattresses, tables, fridges, washers and other bulky items taken off your hands.', N'fa-solid fa-couch', N'Junk Removal', 2, 1, GETDATE()),
                    (N'Renovation & Construction Debris', N'Leftover materials and debris from renovations and small construction jobs.', N'fa-solid fa-hammer', N'Junk Removal', 3, 1, GETDATE()),
                    (N'Yard Waste & Outdoor Cleanup', N'Branches, brush, old fencing and outdoor clutter hauled away.', N'fa-solid fa-tree', N'Junk Removal', 4, 1, GETDATE()),
                    (N'Garage, Basement & Attic Cleanouts', N'We sort, load and clear the spaces that have been piling up for years.', N'fa-solid fa-warehouse', N'Junk Removal', 5, 1, GETDATE()),
                    (N'Estate & Property Cleanouts', N'Full-property cleanouts for move-outs, estates, landlords and businesses.', N'fa-solid fa-broom', N'Junk Removal', 6, 1, GETDATE());
            END

            IF NOT EXISTS (SELECT 1 FROM [ServiceAreas])
            BEGIN
                INSERT INTO [ServiceAreas] ([AreaName], [DisplayName], [Description], [DisplayOrder], [IsActive], [CreatedAt])
                VALUES
                    (N'Halifax, NS', N'Halifax, NS — HQ', N'Primary headquarters and dispatch hub in Halifax, Nova Scotia.', 1, 1, GETDATE()),
                    (N'Dartmouth & Cole Harbour', N'Dartmouth & Cole Harbour', N'Residential and commercial moving and junk removal across Dartmouth and Cole Harbour.', 2, 1, GETDATE()),
                    (N'Bedford & Sackville', N'Bedford & Sackville', N'Full moving, packing, and cleanout services across Bedford and Lower/Middle/Upper Sackville.', 3, 1, GETDATE()),
                    (N'Surrounding Areas', N'Surrounding Areas', N'Local moving and junk hauling throughout surrounding Halifax Regional Municipality communities.', 4, 1, GETDATE());
            END
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ServiceAreas");

        migrationBuilder.DropTable(
            name: "Services");
    }
}

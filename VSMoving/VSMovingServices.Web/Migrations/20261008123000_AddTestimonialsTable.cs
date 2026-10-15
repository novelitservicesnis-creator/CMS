using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class AddTestimonialsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Testimonials",
            columns: table => new
            {
                TestimonialId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Location = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                ReviewText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Rating = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Testimonials", x => x.TestimonialId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Testimonials_IsPublished_DisplayOrder_CreatedAt",
            table: "Testimonials",
            columns: new[] { "IsPublished", "DisplayOrder", "CreatedAt" });

        migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM [Testimonials])
            BEGIN
                INSERT INTO [Testimonials] ([CustomerName], [Location], [ReviewText], [Rating], [DisplayOrder], [IsPublished], [CreatedAt])
                VALUES
                    (N'Priya S', N'Dartmouth, NS', N'They packed our entire condo in a morning and nothing arrived damaged. Genuinely stress-free.', 5, 1, 1, DATEADD(day, -5, GETDATE())),
                    (N'Marc D', N'Halifax, NS', N'On time, careful with every box, and the price matched the quote exactly. Would book again.', 5, 2, 1, DATEADD(day, -3, GETDATE())),
                    (N'Aisha K', N'Bedford, NS', N'They cleared out our garage and old furniture in one afternoon. Quick, polite and fairly priced.', 5, 3, 1, DATEADD(day, -1, GETDATE()));
            END
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Testimonials");
    }
}

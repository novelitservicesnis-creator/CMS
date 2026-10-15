using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class AddShowInFooterToServices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "ShowInFooter",
            table: "Services",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.Sql("UPDATE [Services] SET [ShowInFooter] = 1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ShowInFooter",
            table: "Services");
    }
}

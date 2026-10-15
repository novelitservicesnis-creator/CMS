using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class AddWebsiteAndSocialSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BusinessAddress",
            table: "SystemEmailSettings",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FacebookUrl",
            table: "SystemEmailSettings",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "InstagramUrl",
            table: "SystemEmailSettings",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PrimaryPhone",
            table: "SystemEmailSettings",
            type: "nvarchar(40)",
            maxLength: 40,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SecondaryPhone",
            table: "SystemEmailSettings",
            type: "nvarchar(40)",
            maxLength: 40,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WebsiteEmail",
            table: "SystemEmailSettings",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WhatsAppNumber",
            table: "SystemEmailSettings",
            type: "nvarchar(40)",
            maxLength: 40,
            nullable: true);

        migrationBuilder.Sql(@"
            UPDATE [SystemEmailSettings]
            SET
                [WhatsAppNumber] = COALESCE([WhatsAppNumber], N'15149999790'),
                [WebsiteEmail] = COALESCE([WebsiteEmail], N'vsshippingservicespvt@gmail.com'),
                [PrimaryPhone] = COALESCE([PrimaryPhone], N'514 999 9790'),
                [SecondaryPhone] = COALESCE([SecondaryPhone], N'438 990 8055'),
                [BusinessAddress] = COALESCE([BusinessAddress], N'Halifax, Nova Scotia');
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "BusinessAddress",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "FacebookUrl",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "InstagramUrl",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "PrimaryPhone",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "SecondaryPhone",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "WebsiteEmail",
            table: "SystemEmailSettings");

        migrationBuilder.DropColumn(
            name: "WhatsAppNumber",
            table: "SystemEmailSettings");
    }
}

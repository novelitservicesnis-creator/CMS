using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSMovingServices.Web.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Leads",
            columns: table => new
            {
                LeadId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "VS Moving Services Website"),
                IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Leads", x => x.LeadId);
            });

        migrationBuilder.CreateTable(
            name: "SystemEmailSettings",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SmtpHost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SmtpPort = table.Column<int>(type: "int", nullable: false, defaultValue: 587),
                SenderEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                EncryptedSmtpPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FromName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                FromEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                ReceiverEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                EnableSsl = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                ConfigurationChanged = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastTestSucceeded = table.Column<bool>(type: "bit", nullable: true),
                LastTestMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SystemEmailSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Agent"),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Leads_CreatedAt",
            table: "Leads",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Leads_IsRead",
            table: "Leads",
            column: "IsRead");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Leads");
        migrationBuilder.DropTable(name: "SystemEmailSettings");
        migrationBuilder.DropTable(name: "Users");
    }
}

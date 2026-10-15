using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Data;

public static class DatabaseSeeder
{
    // Pre-computed ASP.NET Core Identity V3 (PasswordHasher<User>) hashes for initial development accounts.
    private const string DefaultAdminPasswordHash = "AQAAAAIAAYagAAAAEBYln4Cy6ujOtfh2iztbhyjoTPU1ZDJ580cZV1ZxdtyZQNduogm4bRnSeBnAS3+W5w==";
    private const string DefaultAgentPasswordHash = "AQAAAAIAAYagAAAAEN1H3Ny8M7TSThhucLN5XaIcvt0YCDs6v8LQch6lO6zlwTHtF5lbf21K4D9iAWLVwQ==";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VSMovingDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        try
        {
            if (await db.Database.CanConnectAsync())
            {
                try
                {
                    _ = await db.Leads.AnyAsync();
                }
                catch
                {
                    logger.LogInformation("Legacy VSMovingServices schema detected. Recreating clean 3-table database schema...");
                    await db.Database.EnsureDeletedAsync();
                }
            }

            await db.Database.MigrateAsync();

            const string targetAdminEmail = "admin@vsmovingservices.com";
            var customAdminPassword = FirstNonEmpty(
                Environment.GetEnvironmentVariable("VSMOVING_ADMIN_PASSWORD"),
                config["InitialSetup:AdminPassword"]);

            var existingAdmin = await db.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == targetAdminEmail);

            if (existingAdmin == null)
            {
                var newAdmin = new User
                {
                    FullName = "VS Moving Admin",
                    Email = targetAdminEmail,
                    PhoneNumber = "514 999 9790",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                newAdmin.PasswordHash = !string.IsNullOrWhiteSpace(customAdminPassword)
                    ? passwordHasher.HashPassword(newAdmin, customAdminPassword)
                    : DefaultAdminPasswordHash;

                db.Users.Add(newAdmin);
                await db.SaveChangesAsync();
                logger.LogInformation("Created Admin user {Email}.", targetAdminEmail);
            }
            else
            {
                bool needsUpdate = false;

                if (existingAdmin.Email != targetAdminEmail)
                {
                    existingAdmin.Email = targetAdminEmail;
                    needsUpdate = true;
                }

                if (existingAdmin.Role != "Admin")
                {
                    existingAdmin.Role = "Admin";
                    needsUpdate = true;
                }

                if (!existingAdmin.IsActive)
                {
                    existingAdmin.IsActive = true;
                    needsUpdate = true;
                }

                if (!string.IsNullOrWhiteSpace(customAdminPassword))
                {
                    if (passwordHasher.VerifyHashedPassword(existingAdmin, existingAdmin.PasswordHash, customAdminPassword) == PasswordVerificationResult.Failed)
                    {
                        existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, customAdminPassword);
                        needsUpdate = true;
                    }
                }
                else if (string.IsNullOrWhiteSpace(existingAdmin.PasswordHash) || !existingAdmin.PasswordHash.StartsWith("AQAAAA", StringComparison.Ordinal))
                {
                    existingAdmin.PasswordHash = DefaultAdminPasswordHash;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    existingAdmin.UpdatedAt = DateTime.Now;
                    await db.SaveChangesAsync();
                    logger.LogInformation("Updated Admin user {Email} hash and active Admin role.", targetAdminEmail);
                }
            }

            const string targetAgentEmail = "agent@vsmovingservices.com";
            var existingAgent = await db.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == targetAgentEmail);

            if (existingAgent == null)
            {
                var customAgentPassword = FirstNonEmpty(
                    Environment.GetEnvironmentVariable("VSMOVING_AGENT_PASSWORD"),
                    config["InitialSetup:AgentPassword"]);

                var newAgent = new User
                {
                    FullName = "Halifax Dispatch Agent",
                    Email = targetAgentEmail,
                    PhoneNumber = "438 990 8055",
                    Role = "Agent",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                newAgent.PasswordHash = !string.IsNullOrWhiteSpace(customAgentPassword)
                    ? passwordHasher.HashPassword(newAgent, customAgentPassword)
                    : DefaultAgentPasswordHash;

                db.Users.Add(newAgent);
                await db.SaveChangesAsync();
            }

            if (!await db.SystemEmailSettings.AnyAsync())
            {
                db.SystemEmailSettings.Add(new SystemEmailSetting
                {
                    SmtpHost = "smtp.gmail.com",
                    SmtpPort = 587,
                    SenderEmail = "vsshippingservicespvt@gmail.com",
                    EncryptedSmtpPassword = null,
                    FromName = "VS Moving Services",
                    FromEmail = "vsshippingservicespvt@gmail.com",
                    ReceiverEmail = "vsshippingservicespvt@gmail.com",
                    EnableSsl = true,
                    ConfigurationChanged = false,
                    LastTestedAt = null,
                    LastTestSucceeded = null,
                    LastTestMessage = null,
                    FacebookUrl = null,
                    InstagramUrl = null,
                    WhatsAppNumber = "15149999790",
                    WebsiteEmail = "vsshippingservicespvt@gmail.com",
                    PrimaryPhone = "514 999 9790",
                    SecondaryPhone = "438 990 8055",
                    BusinessAddress = "Halifax, Nova Scotia",
                    UpdatedAt = DateTime.Now
                });
                await db.SaveChangesAsync();
            }

            if (!await db.Leads.AnyAsync())
            {
                db.Leads.AddRange(
                    new Lead
                    {
                        FullName = "Priya Sharma",
                        PhoneNumber = "902 555 0142",
                        Email = "priya.sharma@example.com",
                        Message = "Looking for a quote to move a 2-bedroom condo from Downtown Halifax to Dartmouth on the 15th of next month. Need packing assistance as well.",
                        Source = "VS Moving Services Website",
                        IsRead = true,
                        CreatedAt = DateTime.Now.AddDays(-2).AddHours(-3),
                        UpdatedAt = DateTime.Now.AddDays(-2)
                    },
                    new Lead
                    {
                        FullName = "Marc Desjardins",
                        PhoneNumber = "902 555 0198",
                        Email = "marc.d@example.com",
                        Message = "Need commercial office moving for 8 desks and filing cabinets within Bedford.",
                        Source = "VS Moving Services Website",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddDays(-1).AddHours(-5)
                    },
                    new Lead
                    {
                        FullName = "Aisha Khan",
                        PhoneNumber = "902 555 0175",
                        Email = "aisha.khan@example.com",
                        Message = "Garage and basement cleanout in Sackville — old sectional couch, broken freezer, and about 15 bags of renovation debris.",
                        Source = "VS Moving Services Website",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-2)
                    }
                );
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the VSMovingServices database.");
            throw;
        }
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        }
        return string.Empty;
    }
}

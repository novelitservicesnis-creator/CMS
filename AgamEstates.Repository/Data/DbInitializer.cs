using AgamEstates.Core;
using AgamEstates.Core.Models;
using AgamEstates.Repository.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AgamEntities context)
        {
            try
            {
                // Ensure tables exist
                await context.Database.EnsureCreatedAsync();

                // 1. Seed Users if not present
                if (!await context.Users.AnyAsync())
                {
                    var admin = new User
                    {
                        FullName = "Amit Sharma",
                        Email = "admin@agamestates.com",
                        PhoneNumber = "+91 98765 43210",
                        PasswordHash = EncryptionService.HashPassword("Admin@123"),
                        Role = "Admin",
                        IsVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3)
                    };

                    var agent1 = new User
                    {
                        FullName = "Priya Verma",
                        Email = "priya@agamestates.com",
                        PhoneNumber = "+91 98123 45678",
                        PasswordHash = EncryptionService.HashPassword("Agent@123"),
                        Role = "Agent",
                        IsVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2)
                    };

                    var agent2 = new User
                    {
                        FullName = "Rohit Mehta",
                        Email = "rohit@agamestates.com",
                        PhoneNumber = "+91 98555 12345",
                        PasswordHash = EncryptionService.HashPassword("Agent@123"),
                        Role = "Agent",
                        IsVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2)
                    };

                    context.Users.AddRange(admin, agent1, agent2);
                    await context.SaveChangesAsync();
                }

                // 2. Seed LeadStatus if not present
                if (!await context.LeadStatus.AnyAsync())
                {
                    var statuses = new[]
                    {
                        new LeadStatus { StatusName = "New", Description = "Freshly received lead awaiting initial contact", IsActive = true },
                        new LeadStatus { StatusName = "Contacted", Description = "Initial outreach or discovery call completed", IsActive = true },
                        new LeadStatus { StatusName = "Interested", Description = "Customer actively reviewing residences and pricing", IsActive = true },
                        new LeadStatus { StatusName = "Site Visit Scheduled", Description = "Site visit booked to view sample flats & campus", IsActive = true },
                        new LeadStatus { StatusName = "Negotiation", Description = "Pricing, unit inventory & payment plans under discussion", IsActive = true },
                        new LeadStatus { StatusName = "Closed Won", Description = "Booking token received & unit confirmed", IsActive = true },
                        new LeadStatus { StatusName = "Closed Lost", Description = "Prospect dropped out or requirements did not align", IsActive = true }
                    };

                    context.LeadStatus.AddRange(statuses);
                    await context.SaveChangesAsync();
                }

                // 3. Seed Leads if not present
                if (!await context.Leads.AnyAsync())
                {
                    var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@agamestates.com");
                    var agent1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "priya@agamestates.com");
                    var agent2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "rohit@agamestates.com");

                    var statusNew = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "New");
                    var statusContacted = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Contacted");
                    var statusInterested = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Interested");
                    var statusSiteVisit = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Site Visit Scheduled");
                    var statusNegotiation = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Negotiation");
                    var statusWon = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Closed Won");
                    var statusLost = await context.LeadStatus.FirstOrDefaultAsync(s => s.StatusName == "Closed Lost");

                    var lead1 = new Lead
                    {
                        Name = "Rajesh Malhotra",
                        PhoneNumber = "+91 98721 23456",
                        Email = "rajesh.malhotra@gmail.com",
                        PropertyInterest = "4 BHK Luxury Residence (Tower B)",
                        Budget = 16500000m,
                        City = "Panchkula",
                        Source = "Website",
                        Priority = "High",
                        StatusId = statusSiteVisit?.StatusId ?? 1,
                        AssignedTo = adminUser?.UserId,
                        Notes = "Looking for higher floor (8th+) facing the central green spine. Wants sunrise orientation.",
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    };

                    var lead2 = new Lead
                    {
                        Name = "Simran Kaur",
                        PhoneNumber = "+91 98150 98765",
                        Email = "simran.kaur@outlook.com",
                        PropertyInterest = "3 BHK Premium Apartment",
                        Budget = 9500000m,
                        City = "Zirakpur",
                        Source = "Google Ads",
                        Priority = "High",
                        StatusId = statusInterested?.StatusId ?? 1,
                        AssignedTo = agent1?.UserId,
                        Notes = "Possession required within 6 months. Husband works in Chandigarh IT Park. Loan pre-approved with HDFC.",
                        CreatedAt = DateTime.UtcNow.AddDays(-8)
                    };

                    var lead3 = new Lead
                    {
                        Name = "Col. Harpreet Singh",
                        PhoneNumber = "+91 94170 11223",
                        Email = "harpreet.singh@army.gov.in",
                        PropertyInterest = "Penthouse with Private Terrace",
                        Budget = 28500000m,
                        City = "Panchkula",
                        Source = "Referral",
                        Priority = "High",
                        StatusId = statusNegotiation?.StatusId ?? 1,
                        AssignedTo = adminUser?.UserId,
                        Notes = "Retired army officer. Wants quiet corner unit with double height ceiling. Discussion on payment schedule ongoing.",
                        CreatedAt = DateTime.UtcNow.AddDays(-20)
                    };

                    var lead4 = new Lead
                    {
                        Name = "Dr. Vikram Sethi",
                        PhoneNumber = "+91 98888 44556",
                        Email = "dr.sethi@fortis.com",
                        PropertyInterest = "3 BHK Residence",
                        Budget = 8500000m,
                        City = "Chandigarh",
                        Source = "Walk-in",
                        Priority = "Medium",
                        StatusId = statusContacted?.StatusId ?? 1,
                        AssignedTo = agent1?.UserId,
                        Notes = "Cardiologist at Fortis Mohali. Prefers tower away from main entrance for peace. Requested floor plans.",
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    };

                    var lead5 = new Lead
                    {
                        Name = "Anita Sharma",
                        PhoneNumber = "+91 97800 66778",
                        Email = "anita.s@techmahindra.com",
                        PropertyInterest = "3 BHK + Study",
                        Budget = 11000000m,
                        City = "Mohali",
                        Source = "Website",
                        Priority = "Medium",
                        StatusId = statusNew?.StatusId ?? 1,
                        AssignedTo = agent2?.UserId,
                        Notes = "Submitted enquiry through online contact form. Interested in club amenities & sports facilities for kids.",
                        CreatedAt = DateTime.UtcNow.AddHours(-18)
                    };

                    var lead6 = new Lead
                    {
                        Name = "Sunil Kapoor",
                        PhoneNumber = "+91 98760 99887",
                        Email = "sunil.kapoor@kapoorjewellers.in",
                        PropertyInterest = "4 BHK Luxury Residence",
                        Budget = 18000000m,
                        City = "Panchkula",
                        Source = "Direct",
                        Priority = "High",
                        StatusId = statusWon?.StatusId ?? 1,
                        AssignedTo = adminUser?.UserId,
                        Notes = "Booking token of ₹ 5,00,000 cleared for Tower A - Unit 1102. Sale deed drafting in progress.",
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    };

                    var lead7 = new Lead
                    {
                        Name = "Gurinder Gill",
                        PhoneNumber = "+91 98141 33221",
                        Email = "ggill@gilltransport.com",
                        PropertyInterest = "Commercial SCO / Plot",
                        Budget = 32000000m,
                        City = "Zirakpur",
                        Source = "Billboard",
                        Priority = "Low",
                        StatusId = statusLost?.StatusId ?? 1,
                        AssignedTo = agent2?.UserId,
                        Notes = "Looking purely for highway commercial frontage. Current residential phase does not fit his requirements.",
                        CreatedAt = DateTime.UtcNow.AddDays(-25)
                    };

                    context.Leads.AddRange(lead1, lead2, lead3, lead4, lead5, lead6, lead7);
                    await context.SaveChangesAsync();

                    // 4. Seed Communications
                    var comm1 = new LeadCommunication
                    {
                        LeadId = lead1.LeadId,
                        UserId = adminUser?.UserId ?? 1,
                        CommunicationType = "Call",
                        Message = "Discussed 4 BHK floor plans and orientation. Client confirmed preference for morning site visit.",
                        FollowUpDate = DateTime.Today.AddDays(1).AddHours(11), // Upcoming tomorrow
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    };

                    var comm2 = new LeadCommunication
                    {
                        LeadId = lead1.LeadId,
                        UserId = adminUser?.UserId ?? 1,
                        CommunicationType = "WhatsApp",
                        Message = "Shared high-resolution brochure, site master plan, and video walkthrough of sample unit.",
                        FollowUpDate = DateTime.Today.AddHours(16), // Today follow-up!
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    };

                    var comm3 = new LeadCommunication
                    {
                        LeadId = lead2.LeadId,
                        UserId = agent1?.UserId ?? 1,
                        CommunicationType = "Call",
                        Message = "Discovery call done. Client requested HDFC bank loan repayment schedule and possession timeline details.",
                        FollowUpDate = DateTime.Today.AddDays(3).AddHours(14), // Upcoming
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    };

                    var comm4 = new LeadCommunication
                    {
                        LeadId = lead3.LeadId,
                        UserId = adminUser?.UserId ?? 1,
                        CommunicationType = "Call",
                        Message = "In-depth negotiation meeting regarding terrace penthouse. Discussed 10-90 subvention scheme.",
                        FollowUpDate = DateTime.Today.AddDays(-1).AddHours(10), // Overdue!
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    };

                    var comm5 = new LeadCommunication
                    {
                        LeadId = lead4.LeadId,
                        UserId = agent1?.UserId ?? 1,
                        CommunicationType = "Email",
                        Message = "Sent formal quotation and payment schedule for Tower C 3 BHK unit to official Fortis email.",
                        FollowUpDate = DateTime.Today.AddDays(2).AddHours(15), // Upcoming
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    };

                    context.LeadCommunication.AddRange(comm1, comm2, comm3, comm4, comm5);
                    await context.SaveChangesAsync();
                }

                // 4. Seed SystemSettings if not present
                if (!await context.SystemSettings.AnyAsync())
                {
                    var defaultSetting = new SystemSetting
                    {
                        CompanyName = "Agam Estates",
                        LogoPath = "/images/logo.png",
                        Email = "chandanyt20314@gmail.com",
                        AddressLine1 = "Sector 20, Panchkula Extension",
                        AddressLine2 = "Tdi South Extension II",
                        City = "Panchkula",
                        State = "Haryana",
                        PostalCode = "134116",
                        Country = "India",
                        FacebookUrl = "https://facebook.com/agamestates",
                        InstagramUrl = "https://instagram.com/agamestates",
                        YouTubeUrl = "https://youtube.com/agamestates",
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.SystemSettings.Add(defaultSetting);
                    await context.SaveChangesAsync();

                    // Seed Contact Numbers
                    var p1 = new SystemContactNumber
                    {
                        SettingId = defaultSetting.SettingId,
                        Label = "Sales",
                        PhoneNumber = "+91 98765 43210",
                        IsPrimary = true,
                        IsActive = true,
                        SortOrder = 1
                    };
                    var p2 = new SystemContactNumber
                    {
                        SettingId = defaultSetting.SettingId,
                        Label = "Site Office",
                        PhoneNumber = "+91 98880 12345",
                        IsPrimary = false,
                        IsActive = true,
                        SortOrder = 2
                    };
                    context.SystemContactNumbers.AddRange(p1, p2);

                    // Seed Business Hours: Mon (1) to Sun (7)
                    for (byte i = 1; i <= 7; i++)
                    {
                        context.SystemBusinessHours.Add(new SystemBusinessHour
                        {
                            SettingId = defaultSetting.SettingId,
                            DayOfWeek = i,
                            IsOpen = true,
                            OpenTime = new TimeSpan(9, 0, 0),
                            CloseTime = new TimeSpan(19, 0, 0),
                            DisplayOrder = i
                        });
                    }

                    await context.SaveChangesAsync();
                }
                else
                {
                    var activeSetting = await context.SystemSettings.FirstOrDefaultAsync(s => s.IsActive);
                    if (activeSetting != null &&
                        (string.IsNullOrWhiteSpace(activeSetting.Email) ||
                         string.Equals(activeSetting.Email.Trim(), "sales@agamestates.in", StringComparison.OrdinalIgnoreCase)))
                    {
                        activeSetting.Email = "chandanyt20314@gmail.com";
                        activeSetting.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                }

                // 5. Seed SystemAnnouncements if not present
                if (!await context.SystemAnnouncements.AnyAsync())
                {
                    var announcement = new SystemAnnouncement
                    {
                        Title = "Site Visit Update",
                        Message = "Site visits are open this weekend. Schedule your private walkthrough.",
                        LinkText = "Request a Visit",
                        LinkUrl = "/#contact",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.SystemAnnouncements.Add(announcement);
                    await context.SaveChangesAsync();
                }

                // 6. Ensure BlogCategories and BlogPosts tables exist
                var blogTablesSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogCategories')
BEGIN
    CREATE TABLE dbo.BlogCategories (
        BlogCategoryId INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        Slug NVARCHAR(120) NOT NULL CONSTRAINT UQ_BlogCategories_Slug UNIQUE,
        Description NVARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogPosts')
BEGIN
    CREATE TABLE dbo.BlogPosts (
        BlogPostId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(220) NOT NULL CONSTRAINT UQ_BlogPosts_Slug UNIQUE,
        ShortDescription NVARCHAR(600) NULL,
        Content NVARCHAR(MAX) NOT NULL,
        FeaturedImage NVARCHAR(500) NULL,
        BlogCategoryId INT NULL,
        AuthorUserId INT NULL,
        AuthorName NVARCHAR(150) NULL,
        IsPublished BIT NOT NULL DEFAULT 0,
        PublishedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CONSTRAINT FK_BlogPosts_BlogCategories FOREIGN KEY (BlogCategoryId)
            REFERENCES dbo.BlogCategories(BlogCategoryId) ON DELETE NO ACTION,
        CONSTRAINT FK_BlogPosts_Users FOREIGN KEY (AuthorUserId)
            REFERENCES dbo.Users(UserId) ON DELETE SET NULL
    );
    CREATE INDEX IX_BlogPosts_BlogCategoryId ON dbo.BlogPosts(BlogCategoryId);
    CREATE INDEX IX_BlogPosts_IsPublished_PublishedAt ON dbo.BlogPosts(IsPublished, PublishedAt DESC);
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BlogPosts') AND name = 'AuthorName')
BEGIN
    ALTER TABLE dbo.BlogPosts ADD AuthorName NVARCHAR(150) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemEmailSettings')
BEGIN
    CREATE TABLE dbo.SystemEmailSettings (
        EmailSettingId INT IDENTITY(1,1) PRIMARY KEY,
        SmtpHost NVARCHAR(200) NOT NULL,
        SmtpPort INT NOT NULL,
        SmtpUsername NVARCHAR(200) NOT NULL,
        EncryptedPassword NVARCHAR(MAX) NOT NULL,
        FromEmail NVARCHAR(200) NOT NULL,
        FromName NVARCHAR(150) NULL,
        ReceiverEmail NVARCHAR(200) NOT NULL,
        EnableSsl BIT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        LastTestedAt DATETIME2 NULL,
        LastTestSucceeded BIT NULL,
        LastTestMessage NVARCHAR(500) NULL,
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedBy INT NULL
    );
END";
                await context.Database.ExecuteSqlRawAsync(blogTablesSql);

                // Seed single active SystemEmailSettings row if not present
                if (!await context.SystemEmailSettings.AnyAsync())
                {
                    var defaultEmailSetting = new SystemEmailSetting
                    {
                        SmtpHost = "smtp.gmail.com",
                        SmtpPort = 587,
                        SmtpUsername = "chandandas.nis@gmail.com",
                        EncryptedPassword = string.Empty,
                        FromEmail = "chandandas.nis@gmail.com",
                        FromName = "Agam Estates",
                        ReceiverEmail = "chandanyt20314@gmail.com",
                        EnableSsl = true,
                        IsActive = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.SystemEmailSettings.Add(defaultEmailSetting);
                    await context.SaveChangesAsync();
                }

                // 7. Seed default BlogCategories if not present
                if (!await context.BlogCategories.AnyAsync())
                {
                    var catMarket = new BlogCategory
                    {
                        CategoryName = "Market Insights",
                        Slug = "market-insights",
                        Description = "Regional real estate trends, infrastructure growth, and investment perspectives across the Tricity.",
                        IsActive = true,
                        SortOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    };
                    var catProject = new BlogCategory
                    {
                        CategoryName = "Project Updates",
                        Slug = "project-updates",
                        Description = "Construction milestones, architectural highlights, and campus developments at Agam Estates.",
                        IsActive = true,
                        SortOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    };
                    var catGuides = new BlogCategory
                    {
                        CategoryName = "Buyer Guides",
                        Slug = "buyer-guides",
                        Description = "Thoughtful guidance on selecting floor plans, RERA due diligence, and home planning.",
                        IsActive = true,
                        SortOrder = 3,
                        CreatedAt = DateTime.UtcNow
                    };
                    var catDesign = new BlogCategory
                    {
                        CategoryName = "Architecture & Living",
                        Slug = "architecture-and-living",
                        Description = "Exploring natural light, low-density planning, and refined residential design.",
                        IsActive = true,
                        SortOrder = 4,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.BlogCategories.AddRange(catMarket, catProject, catGuides, catDesign);
                    await context.SaveChangesAsync();

                    // Seed minimal editorial articles if BlogPosts is empty
                    if (!await context.BlogPosts.AnyAsync())
                    {
                        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
                        var authorId = adminUser?.UserId;

                        var post1 = new BlogPost
                        {
                            Title = "Understanding Property Investment in the Tricity Corridor",
                            Slug = "understanding-property-investment-in-the-tricity-corridor",
                            ShortDescription = "A thoughtful look at connectivity, low-density planning, and why Sector 20 Panchkula Extension continues to command enduring residential demand.",
                            Content = "<p>Selecting a residential address in the Chandigarh–Panchkula–Mohali Tricity requires looking beyond short-term market cycles. Discerning homeowners and long-term investors increasingly prioritize livability, arterial connectivity, and low-density master planning.</p><h2>The Shift Toward Low-Density Living</h2><p>Over the past decade, families across the region have gravitated toward communities that balance privacy with immediate highway access. Developments designed with generous building setbacks, dedicated green spines, and well-proportioned floor plates consistently retain stronger long-term value.</p><blockquote>True residential luxury is measured in natural light, acoustic calm, and the permanence of construction quality.</blockquote><h3>Key Fundamentals to Evaluate</h3><ul><li>Direct connectivity to the Zirakpur–Panchkula–Shimla highway corridor</li><li>RERA registration transparency and clear title documentation</li><li>Usable carpet area efficiency and cross-ventilation</li><li>Dedicated vehicular-free podiums and landscaped open spaces</li></ul><p>At Agam Estates, every planning decision begins with these enduring fundamentals—ensuring each residence serves both as a serene family sanctuary and a resilient generational asset.</p>",
                            BlogCategoryId = catMarket.BlogCategoryId,
                            AuthorUserId = authorId,
                            IsPublished = true,
                            PublishedAt = DateTime.UtcNow.AddDays(-5),
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            CreatedBy = authorId
                        };

                        var post2 = new BlogPost
                        {
                            Title = "Designing Around Light, Space and Cross-Ventilation",
                            Slug = "designing-around-light-space-and-cross-ventilation",
                            ShortDescription = "How daylight orientation, generous balcony depths, and thoughtful room proportions shape everyday comfort in modern North Indian homes.",
                            Content = "<p>Architecture succeeds when a home feels effortlessly bright and breathable throughout changing seasons. In North India's composite climate, orientation and window placement play a decisive role in both thermal comfort and spatial character.</p><h2>Daylight Without Glare</h2><p>By pairing deep recessed balconies with expansive glazing, residences can welcome soft morning and afternoon daylight while shielding interiors from harsh summer heat.</p><h3>Principles Behind Our Floor Plates</h3><ol><li>Dual-aspect living and dining zones that encourage natural airflow</li><li>Clear separation between private family bedrooms and entertaining areas</li><li>Generous ceiling heights that amplify spatial openness</li></ol><p>When evaluating a new residence, visiting the site during different hours of the day reveals how thoughtfully the architecture responds to sun and wind.</p>",
                            BlogCategoryId = catDesign.BlogCategoryId,
                            AuthorUserId = authorId,
                            IsPublished = true,
                            PublishedAt = DateTime.UtcNow.AddDays(-2),
                            CreatedAt = DateTime.UtcNow.AddDays(-2),
                            CreatedBy = authorId
                        };

                        context.BlogPosts.AddRange(post1, post2);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle if DB is offline or restricted
            }
        }
    }
}

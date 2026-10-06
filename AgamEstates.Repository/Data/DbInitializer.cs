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
                        Email = "sales@agamestates.in",
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
            }
            catch (Exception)
            {
                // Silently handle if DB is offline or restricted
            }
        }
    }
}

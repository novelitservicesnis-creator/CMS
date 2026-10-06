using AgamEstates.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AgamEstates.Core
{
    public class AgamEntities : DbContext
    {
        public AgamEntities()
        {
        }

        public AgamEntities(DbContextOptions<AgamEntities> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<LeadStatus> LeadStatus { get; set; } = null!;
        public virtual DbSet<Lead> Leads { get; set; } = null!;
        public virtual DbSet<LeadCommunication> LeadCommunication { get; set; } = null!;
        public virtual DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public virtual DbSet<SystemContactNumber> SystemContactNumbers { get; set; } = null!;
        public virtual DbSet<SystemBusinessHour> SystemBusinessHours { get; set; } = null!;
        public virtual DbSet<SystemAnnouncement> SystemAnnouncements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users table configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.OTP)
                    .HasMaxLength(10);

                entity.Property(e => e.OTPExpiry);

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasDefaultValue("Agent");

                entity.Property(e => e.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });

            // LeadStatus table configuration
            modelBuilder.Entity<LeadStatus>(entity =>
            {
                entity.ToTable("LeadStatus");

                entity.HasKey(e => e.StatusId);

                entity.Property(e => e.StatusId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.StatusName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(200);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });

            // Leads table configuration
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.ToTable("Leads");

                entity.HasKey(e => e.LeadId);

                entity.Property(e => e.LeadId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .HasMaxLength(150);

                entity.Property(e => e.PropertyInterest)
                    .HasMaxLength(200);

                entity.Property(e => e.Budget)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.City)
                    .HasMaxLength(100);

                entity.Property(e => e.Source)
                    .HasMaxLength(100);

                entity.Property(e => e.Priority)
                    .HasMaxLength(20)
                    .HasDefaultValue("Medium");

                entity.Property(e => e.Notes);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.StatusId, "IX_Leads_StatusId");
                entity.HasIndex(e => e.AssignedTo, "IX_Leads_AssignedTo");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Leads)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AssignedUser)
                    .WithMany(p => p.AssignedLeads)
                    .HasForeignKey(d => d.AssignedTo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LeadCommunication table configuration
            modelBuilder.Entity<LeadCommunication>(entity =>
            {
                entity.ToTable("LeadCommunication");

                entity.HasKey(e => e.CommunicationId);

                entity.Property(e => e.CommunicationId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Message);

                entity.Property(e => e.CommunicationType)
                    .HasMaxLength(50);

                entity.Property(e => e.FollowUpDate);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.LeadId, "IX_LeadCommunication_LeadId");

                entity.HasOne(d => d.Lead)
                    .WithMany(p => p.Communications)
                    .HasForeignKey(d => d.LeadId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Communications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SystemSettings table configuration
            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.ToTable("SystemSettings");
                entity.HasKey(e => e.SettingId);
                entity.Property(e => e.SettingId).ValueGeneratedOnAdd();
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.LogoPath).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.AddressLine1).HasMaxLength(250);
                entity.Property(e => e.AddressLine2).HasMaxLength(250);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.FacebookUrl).HasMaxLength(500);
                entity.Property(e => e.InstagramUrl).HasMaxLength(500);
                entity.Property(e => e.YouTubeUrl).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });

            // SystemContactNumbers table configuration
            modelBuilder.Entity<SystemContactNumber>(entity =>
            {
                entity.ToTable("SystemContactNumbers");
                entity.HasKey(e => e.ContactNumberId);
                entity.Property(e => e.ContactNumberId).ValueGeneratedOnAdd();
                entity.Property(e => e.Label).HasMaxLength(80);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(30);
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(0);

                entity.HasOne(d => d.Setting)
                    .WithMany(p => p.ContactNumbers)
                    .HasForeignKey(d => d.SettingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SystemBusinessHours table configuration
            modelBuilder.Entity<SystemBusinessHour>(entity =>
            {
                entity.ToTable("SystemBusinessHours");
                entity.HasKey(e => e.BusinessHourId);
                entity.Property(e => e.BusinessHourId).ValueGeneratedOnAdd();
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.IsOpen).HasDefaultValue(true);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

                entity.HasOne(d => d.Setting)
                    .WithMany(p => p.BusinessHours)
                    .HasForeignKey(d => d.SettingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SystemAnnouncements table configuration
            modelBuilder.Entity<SystemAnnouncement>(entity =>
            {
                entity.ToTable("SystemAnnouncements");
                entity.HasKey(e => e.AnnouncementId);
                entity.Property(e => e.AnnouncementId).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasMaxLength(150);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LinkText).HasMaxLength(100);
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            });
        }
    }
}

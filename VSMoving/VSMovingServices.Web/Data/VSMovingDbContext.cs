using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Data;

public class VSMovingDbContext : DbContext
{
    public VSMovingDbContext(DbContextOptions<VSMovingDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<SystemEmailSetting> SystemEmailSettings => Set<SystemEmailSetting>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(120);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PhoneNumber).HasMaxLength(30);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Agent");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(l => l.LeadId);
            entity.HasIndex(l => l.CreatedAt);
            entity.HasIndex(l => l.IsRead);
            entity.Property(l => l.FullName).IsRequired().HasMaxLength(100);
            entity.Property(l => l.PhoneNumber).HasMaxLength(30);
            entity.Property(l => l.Email).HasMaxLength(150);
            entity.Property(l => l.Message).HasMaxLength(1000);
            entity.Property(l => l.Source).IsRequired().HasMaxLength(100).HasDefaultValue("VS Moving Services Website");
            entity.Property(l => l.IsRead).HasDefaultValue(false);
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<SystemEmailSetting>(entity =>
        {
            entity.ToTable("SystemEmailSettings");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SmtpHost).IsRequired().HasMaxLength(200);
            entity.Property(s => s.SmtpPort).HasDefaultValue(587);
            entity.Property(s => s.SenderEmail).IsRequired().HasMaxLength(150);
            entity.Property(s => s.FromName).HasMaxLength(120);
            entity.Property(s => s.FromEmail).HasMaxLength(150);
            entity.Property(s => s.ReceiverEmail).IsRequired().HasMaxLength(150);
            entity.Property(s => s.EnableSsl).HasDefaultValue(true);
            entity.Property(s => s.ConfigurationChanged).HasDefaultValue(false);
            entity.Property(s => s.LastTestMessage).HasMaxLength(500);
            entity.Property(s => s.FacebookUrl).HasMaxLength(300);
            entity.Property(s => s.InstagramUrl).HasMaxLength(300);
            entity.Property(s => s.WhatsAppNumber).HasMaxLength(40);
            entity.Property(s => s.WebsiteEmail).HasMaxLength(150);
            entity.Property(s => s.PrimaryPhone).HasMaxLength(40);
            entity.Property(s => s.SecondaryPhone).HasMaxLength(40);
            entity.Property(s => s.BusinessAddress).HasMaxLength(250);
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.ToTable("Testimonials");
            entity.HasKey(t => t.TestimonialId);
            entity.HasIndex(t => new { t.IsPublished, t.DisplayOrder, t.CreatedAt });
            entity.Property(t => t.CustomerName).IsRequired().HasMaxLength(120);
            entity.Property(t => t.Location).HasMaxLength(120);
            entity.Property(t => t.ReviewText).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.Rating).HasDefaultValue(5);
            entity.Property(t => t.DisplayOrder).HasDefaultValue(1);
            entity.Property(t => t.IsPublished).HasDefaultValue(true);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
    }
}

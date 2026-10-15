using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using VSMovingServices.Web.Data;

#nullable disable

namespace VSMovingServices.Web.Migrations;

[DbContext(typeof(VSMovingDbContext))]
[Migration("20261008151500_RemoveServicesAndServiceAreas")]
partial class RemoveServicesAndServiceAreas
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.12")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("VSMovingServices.Web.Models.Entities.Lead", b =>
        {
            b.Property<int>("LeadId")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LeadId"));
            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            b.Property<string>("Email")
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");
            b.Property<bool>("IsRead")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(false);
            b.Property<string>("Message")
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");
            b.Property<string>("PhoneNumber")
                .HasMaxLength(30)
                .HasColumnType("nvarchar(30)");
            b.Property<string>("Source")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .HasDefaultValue("VS Moving Services Website");
            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("datetime2");
            b.HasKey("LeadId");
            b.HasIndex("CreatedAt");
            b.HasIndex("IsRead");
            b.ToTable("Leads");
        });

        modelBuilder.Entity("VSMovingServices.Web.Models.Entities.SystemEmailSetting", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
            b.Property<string>("BusinessAddress")
                .HasMaxLength(250)
                .HasColumnType("nvarchar(250)");
            b.Property<bool>("ConfigurationChanged")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(false);
            b.Property<bool>("EnableSsl")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);
            b.Property<string>("EncryptedSmtpPassword")
                .HasColumnType("nvarchar(max)");
            b.Property<string>("FacebookUrl")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");
            b.Property<string>("FromEmail")
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("FromName")
                .HasMaxLength(120)
                .HasColumnType("nvarchar(120)");
            b.Property<string>("InstagramUrl")
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");
            b.Property<string>("LastTestMessage")
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");
            b.Property<bool?>("LastTestSucceeded")
                .HasColumnType("bit");
            b.Property<DateTime?>("LastTestedAt")
                .HasColumnType("datetime2");
            b.Property<string>("PrimaryPhone")
                .HasMaxLength(40)
                .HasColumnType("nvarchar(40)");
            b.Property<string>("ReceiverEmail")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("SecondaryPhone")
                .HasMaxLength(40)
                .HasColumnType("nvarchar(40)");
            b.Property<string>("SenderEmail")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("SmtpHost")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");
            b.Property<int>("SmtpPort")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasDefaultValue(587);
            b.Property<DateTime>("UpdatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            b.Property<string>("WebsiteEmail")
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("WhatsAppNumber")
                .HasMaxLength(40)
                .HasColumnType("nvarchar(40)");
            b.HasKey("Id");
            b.ToTable("SystemEmailSettings");
        });

        modelBuilder.Entity("VSMovingServices.Web.Models.Entities.Testimonial", b =>
        {
            b.Property<int>("TestimonialId")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TestimonialId"));
            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            b.Property<string>("CustomerName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("nvarchar(120)");
            b.Property<int>("DisplayOrder")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasDefaultValue(1);
            b.Property<bool>("IsPublished")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);
            b.Property<string>("Location")
                .HasMaxLength(120)
                .HasColumnType("nvarchar(120)");
            b.Property<int>("Rating")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasDefaultValue(5);
            b.Property<string>("ReviewText")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("nvarchar(1000)");
            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("datetime2");
            b.HasKey("TestimonialId");
            b.HasIndex("IsPublished", "DisplayOrder", "CreatedAt");
            b.ToTable("Testimonials");
        });

        modelBuilder.Entity("VSMovingServices.Web.Models.Entities.User", b =>
        {
            b.Property<int>("UserId")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));
            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");
            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("nvarchar(150)");
            b.Property<string>("FullName")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("nvarchar(120)");
            b.Property<bool>("IsActive")
                .ValueGeneratedOnAdd()
                .HasColumnType("bit")
                .HasDefaultValue(true);
            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            b.Property<string>("PhoneNumber")
                .HasMaxLength(30)
                .HasColumnType("nvarchar(30)");
            b.Property<string>("Role")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)")
                .HasDefaultValue("Agent");
            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("datetime2");
            b.HasKey("UserId");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });
#pragma warning restore 612, 618
    }
}

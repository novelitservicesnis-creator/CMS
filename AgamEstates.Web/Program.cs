using AgamEstates.Core;
using AgamEstates.Repository;
using AgamEstates.Repository.Data;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers and views
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// EF Core DB Context (scoped)
builder.Services.AddDbContext<AgamEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AgamEstatesConnection")));

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AgamEstates.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// Memory cache, HttpContextAccessor, Logging
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();

// Register Repositories (Interface to Implementation)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeadStatusRepository, LeadStatusRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ILeadCommunicationRepository, LeadCommunicationRepository>();
builder.Services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<AgamEstates.Web.Services.ISystemSettingsService, AgamEstates.Web.Services.SystemSettingsService>();
builder.Services.AddScoped<AgamEstates.Web.Services.IFileStorageService, AgamEstates.Web.Services.FileStorageService>();

// Also register concrete types for direct injection or UnitOfWork resolution
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<LeadStatusRepository>();
builder.Services.AddScoped<LeadRepository>();
builder.Services.AddScoped<LeadCommunicationRepository>();
builder.Services.AddScoped<SystemSettingsRepository>();
builder.Services.AddScoped<BlogRepository>();

// Register AgamUnitOfWork
builder.Services.AddScoped<AgamUnitOfWork>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Run DB Initialization & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AgamEntities>();
        await DbInitializer.InitializeAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Production external upload storage mapping (only when Storage:UploadRoot is configured)
var configuredUploadRoot = builder.Configuration["Storage:UploadRoot"];
if (!string.IsNullOrWhiteSpace(configuredUploadRoot))
{
    var fullUploadRoot = Path.GetFullPath(configuredUploadRoot.Trim());
    try
    {
        Directory.CreateDirectory(fullUploadRoot);
        Directory.CreateDirectory(Path.Combine(fullUploadRoot, "system"));
        Directory.CreateDirectory(Path.Combine(fullUploadRoot, "blog"));

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(fullUploadRoot),
            RequestPath = "/uploads"
        });
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to initialize external static upload directory at '{UploadRoot}'. Ensure the IIS Application Pool identity has Read, Write, and Modify permissions.", fullUploadRoot);
    }
}

// Default static files middleware for wwwroot (/css, /js, /images, /assets, and local wwwroot/uploads)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route API attribute controllers
app.MapControllers();

// Route MVC conventional controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using AgamEstates.Core;
using AgamEstates.Core.Models;
using AgamEstates.Repository.Base;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Repository
{
    public class SystemSettingsRepository : RepositoryBase<AgamEntities>, ISystemSettingsRepository
    {
        private static readonly string[] DayNames = new[]
        {
            "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public SystemSettingsRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public async Task<SystemSetting?> GetActiveSettingsAsync()
        {
            var setting = await DataContext.SystemSettings
                .Include(s => s.ContactNumbers.OrderBy(c => c.SortOrder))
                .Include(s => s.BusinessHours.OrderBy(b => b.DisplayOrder))
                .FirstOrDefaultAsync(s => s.IsActive);

            if (setting == null)
            {
                await EnsureDefaultSettingsSeededAsync();
                setting = await DataContext.SystemSettings
                    .Include(s => s.ContactNumbers.OrderBy(c => c.SortOrder))
                    .Include(s => s.BusinessHours.OrderBy(b => b.DisplayOrder))
                    .FirstOrDefaultAsync(s => s.IsActive);
            }

            return setting;
        }

        public async Task<SystemSettingDto?> GetSettingsDtoAsync()
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return null;

            return new SystemSettingDto
            {
                SettingId = setting.SettingId,
                CompanyName = setting.CompanyName,
                LogoPath = setting.LogoPath,
                Email = setting.Email,
                AddressLine1 = setting.AddressLine1,
                AddressLine2 = setting.AddressLine2,
                City = setting.City,
                State = setting.State,
                PostalCode = setting.PostalCode,
                Country = setting.Country,
                FacebookUrl = setting.FacebookUrl,
                InstagramUrl = setting.InstagramUrl,
                YouTubeUrl = setting.YouTubeUrl,
                IsActive = setting.IsActive,
                UpdatedAt = setting.UpdatedAt,
                UpdatedBy = setting.UpdatedBy,
                ContactNumbers = setting.ContactNumbers.Select(c => new SystemContactNumberDto
                {
                    ContactNumberId = c.ContactNumberId,
                    SettingId = c.SettingId,
                    Label = c.Label,
                    PhoneNumber = c.PhoneNumber,
                    IsPrimary = c.IsPrimary,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder
                }).ToList(),
                BusinessHours = setting.BusinessHours.Select(b => new SystemBusinessHourDto
                {
                    BusinessHourId = b.BusinessHourId,
                    SettingId = b.SettingId,
                    DayOfWeek = b.DayOfWeek,
                    DayName = b.DayOfWeek >= 1 && b.DayOfWeek <= 7 ? DayNames[b.DayOfWeek] : $"Day {b.DayOfWeek}",
                    IsOpen = b.IsOpen,
                    OpenTime = b.OpenTime,
                    CloseTime = b.CloseTime,
                    OpenTimeString = FormatTime(b.OpenTime),
                    CloseTimeString = FormatTime(b.CloseTime),
                    DisplayOrder = b.DisplayOrder
                }).ToList()
            };
        }

        public async Task<PublicSiteSettingsViewModel> GetPublicSiteSettingsAsync()
        {
            var setting = await GetActiveSettingsAsync();
            var activeAnnouncement = await GetActivePublicAnnouncementAsync();

            var vm = new PublicSiteSettingsViewModel
            {
                CompanyName = setting?.CompanyName ?? "Agam Estates",
                LogoPath = string.IsNullOrWhiteSpace(setting?.LogoPath) ? "/images/logo.png" : setting.LogoPath,
                Email = setting?.Email,
                AddressLine1 = setting?.AddressLine1,
                AddressLine2 = setting?.AddressLine2,
                City = setting?.City,
                State = setting?.State,
                PostalCode = setting?.PostalCode,
                Country = setting?.Country,
                FacebookUrl = setting?.FacebookUrl,
                InstagramUrl = setting?.InstagramUrl,
                YouTubeUrl = setting?.YouTubeUrl,
                ActiveAnnouncement = activeAnnouncement
            };

            if (setting != null)
            {
                vm.ActivePhones = setting.ContactNumbers
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new SystemContactNumberDto
                    {
                        ContactNumberId = c.ContactNumberId,
                        SettingId = c.SettingId,
                        Label = c.Label,
                        PhoneNumber = c.PhoneNumber,
                        IsPrimary = c.IsPrimary,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder
                    }).ToList();

                vm.PrimaryPhone = vm.ActivePhones.FirstOrDefault(c => c.IsPrimary) 
                                  ?? vm.ActivePhones.FirstOrDefault();

                vm.BusinessHours = setting.BusinessHours
                    .OrderBy(b => b.DisplayOrder)
                    .Select(b => new SystemBusinessHourDto
                    {
                        BusinessHourId = b.BusinessHourId,
                        SettingId = b.SettingId,
                        DayOfWeek = b.DayOfWeek,
                        DayName = b.DayOfWeek >= 1 && b.DayOfWeek <= 7 ? DayNames[b.DayOfWeek] : $"Day {b.DayOfWeek}",
                        IsOpen = b.IsOpen,
                        OpenTime = b.OpenTime,
                        CloseTime = b.CloseTime,
                        OpenTimeString = FormatTime(b.OpenTime),
                        CloseTimeString = FormatTime(b.CloseTime),
                        DisplayOrder = b.DisplayOrder
                    }).ToList();

                var groupedSchedules = GroupConsecutiveBusinessHours(vm.BusinessHours);
                vm.AllGroupedBusinessHours = groupedSchedules;
                vm.GroupedBusinessHours = groupedSchedules
                    .Where(x => x.IsOpen)
                    .ToList();
                vm.BusinessHoursFormatted = FormatBusinessHoursSummary(vm.GroupedBusinessHours);
            }

            return vm;
        }

        public async Task<bool> UpdateGeneralSettingsAsync(string companyName, string? logoPath, int? updatedBy)
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return false;

            setting.CompanyName = companyName.Trim();
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                setting.LogoPath = logoPath.Trim();
            }
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateContactInfoAsync(string? addressLine1, string? addressLine2, string? city, string? state, string? postalCode, string? country, string? email, int? updatedBy)
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return false;

            setting.AddressLine1 = addressLine1?.Trim();
            setting.AddressLine2 = addressLine2?.Trim();
            setting.City = city?.Trim();
            setting.State = state?.Trim();
            setting.PostalCode = postalCode?.Trim();
            setting.Country = country?.Trim();
            setting.Email = email?.Trim();
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSocialLinksAsync(string? facebookUrl, string? instagramUrl, string? youtubeUrl, int? updatedBy)
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return false;

            setting.FacebookUrl = facebookUrl?.Trim();
            setting.InstagramUrl = instagramUrl?.Trim();
            setting.YouTubeUrl = youtubeUrl?.Trim();
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<SystemContactNumberDto>> GetContactNumbersAsync()
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return new List<SystemContactNumberDto>();

            return await DataContext.SystemContactNumbers
                .Where(c => c.SettingId == setting.SettingId)
                .OrderBy(c => c.SortOrder)
                .Select(c => new SystemContactNumberDto
                {
                    ContactNumberId = c.ContactNumberId,
                    SettingId = c.SettingId,
                    Label = c.Label,
                    PhoneNumber = c.PhoneNumber,
                    IsPrimary = c.IsPrimary,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder
                }).ToListAsync();
        }

        public async Task<SystemContactNumberDto?> AddContactNumberAsync(string? label, string phoneNumber, bool isPrimary, bool isActive, int sortOrder)
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return null;

            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                if (isPrimary && isActive)
                {
                    var existingPrimaries = await DataContext.SystemContactNumbers
                        .Where(c => c.SettingId == setting.SettingId && c.IsPrimary)
                        .ToListAsync();
                    foreach (var p in existingPrimaries)
                    {
                        p.IsPrimary = false;
                    }
                }

                var entity = new SystemContactNumber
                {
                    SettingId = setting.SettingId,
                    Label = label?.Trim(),
                    PhoneNumber = phoneNumber.Trim(),
                    IsPrimary = isPrimary,
                    IsActive = isActive,
                    SortOrder = sortOrder
                };

                DataContext.SystemContactNumbers.Add(entity);
                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SystemContactNumberDto
                {
                    ContactNumberId = entity.ContactNumberId,
                    SettingId = entity.SettingId,
                    Label = entity.Label,
                    PhoneNumber = entity.PhoneNumber,
                    IsPrimary = entity.IsPrimary,
                    IsActive = entity.IsActive,
                    SortOrder = entity.SortOrder
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateContactNumberAsync(int contactNumberId, string? label, string phoneNumber, bool isPrimary, bool isActive, int sortOrder)
        {
            var entity = await DataContext.SystemContactNumbers.FindAsync(contactNumberId);
            if (entity == null) return false;

            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                if (isPrimary && isActive)
                {
                    var otherPrimaries = await DataContext.SystemContactNumbers
                        .Where(c => c.SettingId == entity.SettingId && c.ContactNumberId != contactNumberId && c.IsPrimary)
                        .ToListAsync();
                    foreach (var p in otherPrimaries)
                    {
                        p.IsPrimary = false;
                    }
                }

                entity.Label = label?.Trim();
                entity.PhoneNumber = phoneNumber.Trim();
                entity.IsPrimary = isPrimary;
                entity.IsActive = isActive;
                entity.SortOrder = sortOrder;

                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteContactNumberAsync(int contactNumberId)
        {
            var entity = await DataContext.SystemContactNumbers.FindAsync(contactNumberId);
            if (entity == null) return false;

            DataContext.SystemContactNumbers.Remove(entity);
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPrimaryContactNumberAsync(int contactNumberId)
        {
            var entity = await DataContext.SystemContactNumbers.FindAsync(contactNumberId);
            if (entity == null) return false;

            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                var allNumbers = await DataContext.SystemContactNumbers
                    .Where(c => c.SettingId == entity.SettingId)
                    .ToListAsync();

                foreach (var num in allNumbers)
                {
                    num.IsPrimary = (num.ContactNumberId == contactNumberId);
                    if (num.ContactNumberId == contactNumberId)
                    {
                        num.IsActive = true; // Primary must be active
                    }
                }

                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ToggleContactNumberStatusAsync(int contactNumberId)
        {
            var entity = await DataContext.SystemContactNumbers.FindAsync(contactNumberId);
            if (entity == null) return false;

            entity.IsActive = !entity.IsActive;
            if (!entity.IsActive && entity.IsPrimary)
            {
                entity.IsPrimary = false;
            }

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<SystemBusinessHourDto>> GetBusinessHoursAsync()
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null) return new List<SystemBusinessHourDto>();

            return await DataContext.SystemBusinessHours
                .Where(b => b.SettingId == setting.SettingId)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => new SystemBusinessHourDto
                {
                    BusinessHourId = b.BusinessHourId,
                    SettingId = b.SettingId,
                    DayOfWeek = b.DayOfWeek,
                    DayName = b.DayOfWeek >= 1 && b.DayOfWeek <= 7 ? DayNames[b.DayOfWeek] : $"Day {b.DayOfWeek}",
                    IsOpen = b.IsOpen,
                    OpenTime = b.OpenTime,
                    CloseTime = b.CloseTime,
                    OpenTimeString = FormatTime(b.OpenTime),
                    CloseTimeString = FormatTime(b.CloseTime),
                    DisplayOrder = b.DisplayOrder
                }).ToListAsync();
        }

        public async Task<bool> UpdateBusinessHoursAsync(List<SystemBusinessHourDto> hours)
        {
            var setting = await GetActiveSettingsAsync();
            if (setting == null || hours == null) return false;

            var existingHours = await DataContext.SystemBusinessHours
                .Where(b => b.SettingId == setting.SettingId)
                .ToListAsync();

            foreach (var h in hours)
            {
                var record = existingHours.FirstOrDefault(b => b.DayOfWeek == h.DayOfWeek);
                if (record != null)
                {
                    record.IsOpen = h.IsOpen;
                    record.OpenTime = h.IsOpen ? h.OpenTime : null;
                    record.CloseTime = h.IsOpen ? h.CloseTime : null;
                }
            }

            setting.UpdatedAt = DateTime.UtcNow;
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<SystemAnnouncementDto>> GetAllAnnouncementsAsync()
        {
            return await DataContext.SystemAnnouncements
                .OrderByDescending(a => a.IsActive)
                .ThenByDescending(a => a.CreatedAt)
                .Select(a => new SystemAnnouncementDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Message = a.Message,
                    LinkText = a.LinkText,
                    LinkUrl = a.LinkUrl,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    CreatedBy = a.CreatedBy,
                    UpdatedBy = a.UpdatedBy
                }).ToListAsync();
        }

        public async Task<SystemAnnouncementDto?> GetAnnouncementByIdAsync(int announcementId)
        {
            var a = await DataContext.SystemAnnouncements.FindAsync(announcementId);
            if (a == null) return null;

            return new SystemAnnouncementDto
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Message = a.Message,
                LinkText = a.LinkText,
                LinkUrl = a.LinkUrl,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                CreatedBy = a.CreatedBy,
                UpdatedBy = a.UpdatedBy
            };
        }

        public async Task<SystemAnnouncementDto?> GetActivePublicAnnouncementAsync()
        {
            var now = DateTime.UtcNow;
            var active = await DataContext.SystemAnnouncements
                .Where(a => a.IsActive)
                .FirstOrDefaultAsync();

            if (active == null) return null;

            // Date validation check
            if (active.StartDate.HasValue && now < active.StartDate.Value) return null;
            if (active.EndDate.HasValue && now > active.EndDate.Value) return null;

            return new SystemAnnouncementDto
            {
                AnnouncementId = active.AnnouncementId,
                Title = active.Title,
                Message = active.Message,
                LinkText = active.LinkText,
                LinkUrl = active.LinkUrl,
                StartDate = active.StartDate,
                EndDate = active.EndDate,
                IsActive = active.IsActive,
                CreatedAt = active.CreatedAt,
                UpdatedAt = active.UpdatedAt,
                CreatedBy = active.CreatedBy,
                UpdatedBy = active.UpdatedBy
            };
        }

        public async Task<int> CreateAnnouncementAsync(string? title, string message, string? linkText, string? linkUrl, DateTime? startDate, DateTime? endDate, bool isActive, int? createdBy)
        {
            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                if (isActive)
                {
                    var existingActive = await DataContext.SystemAnnouncements
                        .Where(a => a.IsActive)
                        .ToListAsync();
                    foreach (var a in existingActive)
                    {
                        a.IsActive = false;
                        a.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var entity = new SystemAnnouncement
                {
                    Title = title?.Trim(),
                    Message = message.Trim(),
                    LinkText = linkText?.Trim(),
                    LinkUrl = linkUrl?.Trim(),
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = isActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                DataContext.SystemAnnouncements.Add(entity);
                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return entity.AnnouncementId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAnnouncementAsync(int announcementId, string? title, string message, string? linkText, string? linkUrl, DateTime? startDate, DateTime? endDate, bool isActive, int? updatedBy)
        {
            var entity = await DataContext.SystemAnnouncements.FindAsync(announcementId);
            if (entity == null) return false;

            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                if (isActive)
                {
                    var otherActive = await DataContext.SystemAnnouncements
                        .Where(a => a.IsActive && a.AnnouncementId != announcementId)
                        .ToListAsync();
                    foreach (var a in otherActive)
                    {
                        a.IsActive = false;
                        a.UpdatedAt = DateTime.UtcNow;
                    }
                }

                entity.Title = title?.Trim();
                entity.Message = message.Trim();
                entity.LinkText = linkText?.Trim();
                entity.LinkUrl = linkUrl?.Trim();
                entity.StartDate = startDate;
                entity.EndDate = endDate;
                entity.IsActive = isActive;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = updatedBy;

                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ActivateAnnouncementAsync(int announcementId, int? updatedBy)
        {
            var entity = await DataContext.SystemAnnouncements.FindAsync(announcementId);
            if (entity == null) return false;

            using var transaction = await DataContext.Database.BeginTransactionAsync();
            try
            {
                var allActive = await DataContext.SystemAnnouncements
                    .Where(a => a.IsActive)
                    .ToListAsync();

                foreach (var a in allActive)
                {
                    a.IsActive = false;
                    a.UpdatedAt = DateTime.UtcNow;
                    a.UpdatedBy = updatedBy;
                }

                entity.IsActive = true;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = updatedBy;

                await DataContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeactivateAnnouncementAsync(int announcementId, int? updatedBy)
        {
            var entity = await DataContext.SystemAnnouncements.FindAsync(announcementId);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAnnouncementAsync(int announcementId)
        {
            var entity = await DataContext.SystemAnnouncements.FindAsync(announcementId);
            if (entity == null) return false;

            DataContext.SystemAnnouncements.Remove(entity);
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task EnsureDefaultSettingsSeededAsync()
        {
            if (!await DataContext.SystemSettings.AnyAsync())
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

                DataContext.SystemSettings.Add(defaultSetting);
                await DataContext.SaveChangesAsync();

                // Contact Numbers
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
                DataContext.SystemContactNumbers.AddRange(p1, p2);

                // Business Hours: Monday (1) to Sunday (7) - 9:00 AM to 7:00 PM
                for (byte i = 1; i <= 7; i++)
                {
                    DataContext.SystemBusinessHours.Add(new SystemBusinessHour
                    {
                        SettingId = defaultSetting.SettingId,
                        DayOfWeek = i,
                        IsOpen = true,
                        OpenTime = new TimeSpan(9, 0, 0),
                        CloseTime = new TimeSpan(19, 0, 0),
                        DisplayOrder = i
                    });
                }

                await DataContext.SaveChangesAsync();
            }

            if (!await DataContext.SystemAnnouncements.AnyAsync())
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
                DataContext.SystemAnnouncements.Add(announcement);
                await DataContext.SaveChangesAsync();
            }

            if (!await DataContext.SystemEmailSettings.AnyAsync())
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
                DataContext.SystemEmailSettings.Add(defaultEmailSetting);
                await DataContext.SaveChangesAsync();
            }
        }

        public async Task<SystemEmailSetting?> GetActiveEmailSettingEntityAsync()
        {
            var emailSetting = await DataContext.SystemEmailSettings
                .OrderByDescending(e => e.IsActive)
                .ThenBy(e => e.EmailSettingId)
                .FirstOrDefaultAsync();

            if (emailSetting == null)
            {
                await EnsureDefaultSettingsSeededAsync();
                emailSetting = await DataContext.SystemEmailSettings
                    .OrderByDescending(e => e.IsActive)
                    .ThenBy(e => e.EmailSettingId)
                    .FirstOrDefaultAsync();
            }

            return emailSetting;
        }

        public async Task<SystemEmailSettingDto> GetEmailSettingsDtoAsync()
        {
            var entity = await GetActiveEmailSettingEntityAsync();
            if (entity == null)
            {
                return new SystemEmailSettingDto();
            }

            return new SystemEmailSettingDto
            {
                EmailSettingId = entity.EmailSettingId,
                SmtpHost = entity.SmtpHost,
                SmtpPort = entity.SmtpPort,
                SmtpUsername = entity.SmtpUsername,
                HasSavedPassword = !string.IsNullOrWhiteSpace(entity.EncryptedPassword),
                FromEmail = string.IsNullOrWhiteSpace(entity.FromEmail) ? entity.SmtpUsername : entity.FromEmail,
                FromName = string.IsNullOrWhiteSpace(entity.FromName) ? "Agam Estates" : entity.FromName,
                ReceiverEmail = entity.ReceiverEmail,
                EnableSsl = entity.EnableSsl,
                IsActive = entity.IsActive,
                LastTestedAt = entity.LastTestedAt,
                LastTestSucceeded = entity.LastTestSucceeded,
                LastTestMessage = entity.LastTestMessage,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = entity.UpdatedBy
            };
        }

        public async Task<bool> SaveEmailSettingsAsync(
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string? newEncryptedPassword,
            string fromEmail,
            string? fromName,
            string receiverEmail,
            bool enableSsl,
            int? updatedBy)
        {
            var entity = await GetActiveEmailSettingEntityAsync();
            if (entity == null)
            {
                entity = new SystemEmailSetting
                {
                    SmtpHost = smtpHost.Trim(),
                    SmtpPort = smtpPort,
                    SmtpUsername = smtpUsername.Trim(),
                    EncryptedPassword = newEncryptedPassword ?? string.Empty,
                    FromEmail = fromEmail.Trim(),
                    FromName = string.IsNullOrWhiteSpace(fromName) ? "Agam Estates" : fromName.Trim(),
                    ReceiverEmail = receiverEmail.Trim(),
                    EnableSsl = enableSsl,
                    IsActive = true,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = updatedBy
                };
                DataContext.SystemEmailSettings.Add(entity);
                await DataContext.SaveChangesAsync();
                return true;
            }

            var cleanHost = smtpHost.Trim();
            var cleanUsername = smtpUsername.Trim();
            var cleanFromEmail = fromEmail.Trim();
            var cleanFromName = string.IsNullOrWhiteSpace(fromName) ? "Agam Estates" : fromName.Trim();
            var cleanReceiver = receiverEmail.Trim();

            bool connectionChanged =
                !string.Equals(entity.SmtpHost, cleanHost, StringComparison.OrdinalIgnoreCase) ||
                entity.SmtpPort != smtpPort ||
                !string.Equals(entity.SmtpUsername, cleanUsername, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(entity.FromEmail, cleanFromEmail, StringComparison.OrdinalIgnoreCase) ||
                entity.EnableSsl != enableSsl ||
                !string.IsNullOrWhiteSpace(newEncryptedPassword);

            entity.SmtpHost = cleanHost;
            entity.SmtpPort = smtpPort;
            entity.SmtpUsername = cleanUsername;
            if (!string.IsNullOrWhiteSpace(newEncryptedPassword))
            {
                entity.EncryptedPassword = newEncryptedPassword;
            }
            entity.FromEmail = cleanFromEmail;
            entity.FromName = cleanFromName;
            entity.ReceiverEmail = cleanReceiver;
            entity.EnableSsl = enableSsl;
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            if (connectionChanged)
            {
                entity.LastTestedAt = null;
                entity.LastTestSucceeded = null;
                entity.LastTestMessage = null;
            }

            // Ensure any duplicate legacy rows (if any ever existed) are deactivated/removed so only ONE active record exists
            var extraRows = await DataContext.SystemEmailSettings
                .Where(e => e.EmailSettingId != entity.EmailSettingId)
                .ToListAsync();
            if (extraRows.Any())
            {
                DataContext.SystemEmailSettings.RemoveRange(extraRows);
            }

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmailTestStatusAsync(bool succeeded, string? message, int? updatedBy)
        {
            var entity = await GetActiveEmailSettingEntityAsync();
            if (entity == null) return false;

            entity.LastTestedAt = DateTime.UtcNow;
            entity.LastTestSucceeded = succeeded;
            entity.LastTestMessage = message?.Length > 500 ? message.Substring(0, 500) : message;
            entity.UpdatedAt = DateTime.UtcNow;
            if (updatedBy.HasValue)
            {
                entity.UpdatedBy = updatedBy;
            }

            await DataContext.SaveChangesAsync();
            return true;
        }

        private static string FormatTime(TimeSpan? time)
        {
            if (!time.HasValue) return "";
            var dt = DateTime.Today.Add(time.Value);
            return dt.ToString("h:mm tt"); // "9:00 AM", not "09:00 AM"
        }

        private static string FullDayName(byte day) => day switch
        {
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            7 => "Sunday",
            _ => $"Day {day}"
        };

        private static List<GroupedBusinessHourItem> GroupConsecutiveBusinessHours(List<SystemBusinessHourDto> hours)
        {
            var result = new List<GroupedBusinessHourItem>();
            if (hours == null || hours.Count == 0)
            {
                result.Add(new GroupedBusinessHourItem { Days = "Monday – Sunday", Hours = "9:00 AM – 7:00 PM", IsClosed = false });
                return result;
            }

            var ordered = hours.OrderBy(h => h.DayOfWeek).ToList();
            if (ordered.Count != 7)
            {
                foreach (var h in ordered)
                {
                    result.Add(new GroupedBusinessHourItem
                    {
                        Days = FullDayName(h.DayOfWeek),
                        Hours = h.IsOpen ? $"{FormatTime(h.OpenTime)} – {FormatTime(h.CloseTime)}" : "Closed",
                        IsClosed = !h.IsOpen
                    });
                }
                return result;
            }

            // Case A: All 7 days are identical
            bool allSame = true;
            for (int i = 1; i < 7; i++)
            {
                if (ordered[i].IsOpen != ordered[0].IsOpen ||
                    ordered[i].OpenTime != ordered[0].OpenTime ||
                    ordered[i].CloseTime != ordered[0].CloseTime)
                {
                    allSame = false;
                    break;
                }
            }

            if (allSame)
            {
                if (ordered[0].IsOpen)
                {
                    result.Add(new GroupedBusinessHourItem
                    {
                        Days = "Monday – Sunday",
                        Hours = $"{FormatTime(ordered[0].OpenTime)} – {FormatTime(ordered[0].CloseTime)}",
                        IsClosed = false
                    });
                }
                else
                {
                    result.Add(new GroupedBusinessHourItem
                    {
                        Days = "Monday – Sunday",
                        Hours = "Closed",
                        IsClosed = true
                    });
                }
                return result;
            }

            // Case B: Group strictly consecutive matching days
            int start = 0;
            while (start < 7)
            {
                int end = start;
                while (end + 1 < 7 &&
                       ordered[end + 1].IsOpen == ordered[start].IsOpen &&
                       ordered[end + 1].OpenTime == ordered[start].OpenTime &&
                       ordered[end + 1].CloseTime == ordered[start].CloseTime)
                {
                    end++;
                }

                string daysLabel = (start == end)
                    ? FullDayName(ordered[start].DayOfWeek)
                    : $"{FullDayName(ordered[start].DayOfWeek)} – {FullDayName(ordered[end].DayOfWeek)}";

                bool isClosed = !ordered[start].IsOpen;
                string hoursLabel = ordered[start].IsOpen
                    ? $"{FormatTime(ordered[start].OpenTime)} – {FormatTime(ordered[start].CloseTime)}"
                    : "Closed";

                result.Add(new GroupedBusinessHourItem
                {
                    Days = daysLabel,
                    Hours = hoursLabel,
                    IsClosed = isClosed
                });

                start = end + 1;
            }

            return result;
        }

        private static string FormatBusinessHoursSummary(List<GroupedBusinessHourItem> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                return string.Empty;
            }

            if (groups.Count == 1 && (groups[0].Days == "Monday – Sunday" || groups[0].Days == "All seven days"))
            {
                return $"Monday – Sunday: {groups[0].Hours}";
            }

            return string.Join(" · ", groups.Select(g => $"{g.Days}: {g.Hours}"));
        }
    }
}

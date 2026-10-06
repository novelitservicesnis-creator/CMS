using AgamEstates.Core;
using AgamEstates.Core.Models;
using AgamEstates.Repository.Base;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.Service;
using AgamEstates.Repository.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Repository
{
    public class UserRepository : RepositoryBase<AgamEntities>, IUserRepository
    {
        public UserRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await DataContext.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Role = u.Role,
                    IsVerified = u.IsVerified,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await DataContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await DataContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<int> CreateUserAsync(CreateUserDto model)
        {
            var user = new User
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = EncryptionService.HashPassword(model.Password),
                Role = string.IsNullOrWhiteSpace(model.Role) ? "Agent" : model.Role.Trim(),
                IsVerified = false,
                IsActive = true,
                CreatedAt = GetIstTimeNow()
            };

            DataContext.Users.Add(user);
            await DataContext.SaveChangesAsync();

            return user.UserId;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto model)
        {
            var user = await DataContext.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return false;

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Email = model.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                user.Role = model.Role.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = EncryptionService.HashPassword(model.Password);
            }

            if (model.IsVerified.HasValue)
            {
                user.IsVerified = model.IsVerified.Value;
            }

            if (model.IsActive.HasValue)
            {
                user.IsActive = model.IsActive.Value;
            }

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await DataContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return false;

            // Soft-delete / deactivate
            user.IsActive = false;
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidatePasswordAsync(int userId, string password)
        {
            var user = await DataContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            return EncryptionService.VerifyPassword(password, user.PasswordHash);
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await DataContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.PasswordHash = EncryptionService.HashPassword(newPassword);
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveStatusAsync(int userId)
        {
            var user = await DataContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            await DataContext.SaveChangesAsync();
            return user.IsActive;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await DataContext.Users.AnyAsync(u => u.UserId == id && u.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            var normalizedEmail = email.Trim().ToLower();
            if (excludeUserId.HasValue)
            {
                return await DataContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.UserId != excludeUserId.Value);
            }
            return await DataContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        }
    }
}

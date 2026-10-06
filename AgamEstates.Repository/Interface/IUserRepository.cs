using AgamEstates.Repository.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface IUserRepository
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<int> CreateUserAsync(CreateUserDto model);
        Task<bool> UpdateUserAsync(UpdateUserDto model);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ValidatePasswordAsync(int userId, string password);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<bool> ToggleActiveStatusAsync(int userId);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    }
}

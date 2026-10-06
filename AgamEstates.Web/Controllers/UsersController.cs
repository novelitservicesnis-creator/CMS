using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public UsersController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<UsersController> logger,
            IUserRepository userRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _userRepository = userRepository;
        }

        [HttpGet("")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return JsonSuccess(users, "Users retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve users.", ex.Message, 500);
            }
        }

        [HttpGet("{id:int}")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return JsonError($"User with ID {id} was not found.", null, 404);
                }

                return JsonSuccess(user, "User retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve user.", ex.Message, 500);
            }
        }

        [HttpPost("")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid user data.");
                }

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    return JsonError("Full name is required.");
                }

                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    return JsonError("Phone number is required.");
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return JsonError("Email address is required.");
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return JsonError("Password is required.");
                }

                var emailExists = await _userRepository.EmailExistsAsync(model.Email);
                if (emailExists)
                {
                    return JsonError($"Email '{model.Email}' is already in use by another user.", null, 409);
                }

                var userId = await _userRepository.CreateUserAsync(model);
                var createdUser = await _userRepository.GetUserByIdAsync(userId);

                return JsonSuccess(createdUser, "User created successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to create user.", ex.Message, 500);
            }
        }

        [HttpPut("{id:int}")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid user data.");
                }

                model.UserId = id;

                var existing = await _userRepository.GetUserByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"User with ID {id} was not found.", null, 404);
                }

                if (!string.IsNullOrWhiteSpace(model.Email) && !string.Equals(model.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userRepository.EmailExistsAsync(model.Email, id);
                    if (emailExists)
                    {
                        return JsonError($"Email '{model.Email}' is already registered to another account.", null, 409);
                    }
                }

                var updated = await _userRepository.UpdateUserAsync(model);
                if (!updated)
                {
                    return JsonError("Failed to update user details.");
                }

                var updatedUser = await _userRepository.GetUserByIdAsync(id);
                return JsonSuccess(updatedUser, "User updated successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to update user.", ex.Message, 500);
            }
        }

        [HttpDelete("{id:int}")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _userRepository.GetUserByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"User with ID {id} was not found.", null, 404);
                }

                var deleted = await _userRepository.DeleteUserAsync(id);
                if (!deleted)
                {
                    return JsonError("Failed to deactivate user.");
                }

                return JsonSuccess(new { userId = id }, "User deactivated successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to delete user.", ex.Message, 500);
            }
        }
    }
}

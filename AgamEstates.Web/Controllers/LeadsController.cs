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
    public class LeadsController : BaseController
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ILeadStatusRepository _leadStatusRepository;
        private readonly IUserRepository _userRepository;

        public LeadsController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<LeadsController> logger,
            ILeadRepository leadRepository,
            ILeadStatusRepository leadStatusRepository,
            IUserRepository userRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _leadRepository = leadRepository;
            _leadStatusRepository = leadStatusRepository;
            _userRepository = userRepository;
        }

        [HttpGet("")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var leads = await _leadRepository.GetAllLeadsAsync();
                return JsonSuccess(leads, "Leads retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve leads.", ex.Message, 500);
            }
        }

        [HttpGet("{id:int}")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(id);
                if (lead == null)
                {
                    return JsonError($"Lead with ID {id} was not found.", null, 404);
                }

                return JsonSuccess(lead, "Lead retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve lead.", ex.Message, 500);
            }
        }

        [HttpGet("status/{statusId:int}")]
        [HttpGet("GetByStatus/{statusId:int}")]
        public async Task<IActionResult> GetByStatus(int statusId)
        {
            try
            {
                var leads = await _leadRepository.GetLeadsByStatusAsync(statusId);
                return JsonSuccess(leads, $"Leads with status {statusId} retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve leads by status.", ex.Message, 500);
            }
        }

        [HttpGet("assigned/{userId:int}")]
        [HttpGet("GetByAssignedUser/{userId:int}")]
        public async Task<IActionResult> GetByAssignedUser(int userId)
        {
            try
            {
                var leads = await _leadRepository.GetLeadsByAssignedUserAsync(userId);
                return JsonSuccess(leads, $"Leads assigned to user {userId} retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve leads by assigned user.", ex.Message, 500);
            }
        }

        [HttpPost("")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeadDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid lead data.");
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return JsonError("Lead name is required.");
                }

                if (model.StatusId.HasValue)
                {
                    var statusExists = await _leadStatusRepository.StatusExistsAsync(model.StatusId.Value);
                    if (!statusExists)
                    {
                        return JsonError($"Status ID {model.StatusId.Value} does not exist.", null, 400);
                    }
                }

                if (model.AssignedTo.HasValue)
                {
                    var userExists = await _userRepository.UserExistsAsync(model.AssignedTo.Value);
                    if (!userExists)
                    {
                        return JsonError($"Assigned user ID {model.AssignedTo.Value} does not exist or is inactive.", null, 400);
                    }
                }

                var leadId = await _leadRepository.CreateLeadAsync(model);
                var createdLead = await _leadRepository.GetLeadByIdAsync(leadId);

                return JsonSuccess(createdLead, "Lead created successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to create lead.", ex.Message, 500);
            }
        }

        [HttpPut("{id:int}")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid lead data.");
                }

                model.LeadId = id;

                var existing = await _leadRepository.GetLeadByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"Lead with ID {id} was not found.", null, 404);
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return JsonError("Lead name is required.");
                }

                if (model.StatusId.HasValue)
                {
                    var statusExists = await _leadStatusRepository.StatusExistsAsync(model.StatusId.Value);
                    if (!statusExists)
                    {
                        return JsonError($"Status ID {model.StatusId.Value} does not exist.", null, 400);
                    }
                }

                if (model.AssignedTo.HasValue)
                {
                    var userExists = await _userRepository.UserExistsAsync(model.AssignedTo.Value);
                    if (!userExists)
                    {
                        return JsonError($"Assigned user ID {model.AssignedTo.Value} does not exist or is inactive.", null, 400);
                    }
                }

                var updated = await _leadRepository.UpdateLeadAsync(model);
                if (!updated)
                {
                    return JsonError("Failed to update lead.");
                }

                var updatedLead = await _leadRepository.GetLeadByIdAsync(id);
                return JsonSuccess(updatedLead, "Lead updated successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to update lead.", ex.Message, 500);
            }
        }

        [HttpDelete("{id:int}")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _leadRepository.GetLeadByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"Lead with ID {id} was not found.", null, 404);
                }

                var deleted = await _leadRepository.DeleteLeadAsync(id);
                if (!deleted)
                {
                    return JsonError("Failed to delete lead.");
                }

                return JsonSuccess(new { leadId = id }, "Lead deleted successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to delete lead.", ex.Message, 500);
            }
        }
    }
}

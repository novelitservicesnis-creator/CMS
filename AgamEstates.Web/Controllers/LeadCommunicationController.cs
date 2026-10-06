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
    public class LeadCommunicationController : BaseController
    {
        private readonly ILeadCommunicationRepository _commRepository;
        private readonly ILeadRepository _leadRepository;
        private readonly IUserRepository _userRepository;

        public LeadCommunicationController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<LeadCommunicationController> logger,
            ILeadCommunicationRepository commRepository,
            ILeadRepository leadRepository,
            IUserRepository userRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _commRepository = commRepository;
            _leadRepository = leadRepository;
            _userRepository = userRepository;
        }

        [HttpGet("lead/{leadId:int}")]
        [HttpGet("GetByLead/{leadId:int}")]
        public async Task<IActionResult> GetByLead(int leadId)
        {
            try
            {
                var comms = await _commRepository.GetCommunicationsByLeadIdAsync(leadId);
                return JsonSuccess(comms, $"Communications for lead {leadId} retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve communications.", ex.Message, 500);
            }
        }

        [HttpGet("{id:int}")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var comm = await _commRepository.GetCommunicationByIdAsync(id);
                if (comm == null)
                {
                    return JsonError($"Communication with ID {id} was not found.", null, 404);
                }

                return JsonSuccess(comm, "Communication retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve communication.", ex.Message, 500);
            }
        }

        [HttpPost("")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeadCommunicationDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid communication data.");
                }

                var leadExists = await _leadRepository.LeadExistsAsync(model.LeadId);
                if (!leadExists)
                {
                    return JsonError($"Lead with ID {model.LeadId} does not exist.", null, 400);
                }

                var userExists = await _userRepository.UserExistsAsync(model.UserId);
                if (!userExists)
                {
                    return JsonError($"User with ID {model.UserId} does not exist or is inactive.", null, 400);
                }

                var commId = await _commRepository.CreateCommunicationAsync(model);
                var createdComm = await _commRepository.GetCommunicationByIdAsync(commId);

                return JsonSuccess(createdComm, "Communication logged successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to log communication.", ex.Message, 500);
            }
        }

        [HttpPut("{id:int}")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadCommunicationDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid communication data.");
                }

                model.CommunicationId = id;

                var existing = await _commRepository.GetCommunicationByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"Communication with ID {id} was not found.", null, 404);
                }

                var updated = await _commRepository.UpdateCommunicationAsync(model);
                if (!updated)
                {
                    return JsonError("Failed to update communication.");
                }

                var updatedComm = await _commRepository.GetCommunicationByIdAsync(id);
                return JsonSuccess(updatedComm, "Communication updated successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to update communication.", ex.Message, 500);
            }
        }

        [HttpDelete("{id:int}")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _commRepository.GetCommunicationByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"Communication with ID {id} was not found.", null, 404);
                }

                var deleted = await _commRepository.DeleteCommunicationAsync(id);
                if (!deleted)
                {
                    return JsonError("Failed to delete communication.");
                }

                return JsonSuccess(new { communicationId = id }, "Communication deleted successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to delete communication.", ex.Message, 500);
            }
        }
    }
}

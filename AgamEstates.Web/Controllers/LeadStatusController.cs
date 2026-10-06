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
    public class LeadStatusController : BaseController
    {
        private readonly ILeadStatusRepository _leadStatusRepository;

        public LeadStatusController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<LeadStatusController> logger,
            ILeadStatusRepository leadStatusRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _leadStatusRepository = leadStatusRepository;
        }

        [HttpGet("")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var statuses = await _leadStatusRepository.GetAllStatusesAsync();
                return JsonSuccess(statuses, "Lead statuses retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve lead statuses.", ex.Message, 500);
            }
        }

        [HttpGet("{id:int}")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var status = await _leadStatusRepository.GetStatusByIdAsync(id);
                if (status == null)
                {
                    return JsonError($"Lead status with ID {id} was not found.", null, 404);
                }

                return JsonSuccess(status, "Lead status retrieved successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to retrieve lead status.", ex.Message, 500);
            }
        }

        [HttpPost("")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateLeadStatusDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid lead status data.");
                }

                if (string.IsNullOrWhiteSpace(model.StatusName))
                {
                    return JsonError("Status name is required.");
                }

                var statusId = await _leadStatusRepository.CreateStatusAsync(model);
                var createdStatus = await _leadStatusRepository.GetStatusByIdAsync(statusId);

                return JsonSuccess(createdStatus, "Lead status created successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to create lead status.", ex.Message, 500);
            }
        }

        [HttpPut("{id:int}")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadStatusDto model)
        {
            try
            {
                if (model == null)
                {
                    return JsonError("Invalid lead status data.");
                }

                model.StatusId = id;

                var existing = await _leadStatusRepository.GetStatusByIdAsync(id);
                if (existing == null)
                {
                    return JsonError($"Lead status with ID {id} was not found.", null, 404);
                }

                if (string.IsNullOrWhiteSpace(model.StatusName))
                {
                    return JsonError("Status name is required.");
                }

                var updated = await _leadStatusRepository.UpdateStatusAsync(model);
                if (!updated)
                {
                    return JsonError("Failed to update lead status.");
                }

                var updatedStatus = await _leadStatusRepository.GetStatusByIdAsync(id);
                return JsonSuccess(updatedStatus, "Lead status updated successfully.");
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return JsonError("Failed to update lead status.", ex.Message, 500);
            }
        }
    }
}

using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ILeadStatusRepository _leadStatusRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILeadCommunicationRepository _leadCommunicationRepository;

        public AdminController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<AdminController> logger,
            ILeadRepository leadRepository,
            ILeadStatusRepository leadStatusRepository,
            IUserRepository userRepository,
            ILeadCommunicationRepository leadCommunicationRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _leadRepository = leadRepository;
            _leadStatusRepository = leadStatusRepository;
            _userRepository = userRepository;
            _leadCommunicationRepository = leadCommunicationRepository;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int id))
            {
                return id;
            }
            return 1;
        }

        [HttpGet("/AdminPanel")]
        [HttpGet("/Admin/Dashboard")]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel();

            try
            {
                List<LeadDto> allLeads;
                if (User.IsInRole("Admin"))
                {
                    allLeads = await _leadRepository.GetAllLeadsAsync();
                }
                else
                {
                    var currentUserId = GetCurrentUserId();
                    allLeads = await _leadRepository.GetLeadsByAssignedUserAsync(currentUserId);
                }

                var allStatuses = await _leadStatusRepository.GetAllStatusesAsync();

                model.TotalLeads = allLeads.Count;

                // Status counts
                model.NewLeads = allLeads.Count(l => (l.StatusName ?? "").Equals("New", StringComparison.OrdinalIgnoreCase));
                model.InterestedLeads = allLeads.Count(l => (l.StatusName ?? "").Equals("Interested", StringComparison.OrdinalIgnoreCase));
                model.SiteVisitsScheduled = allLeads.Count(l => (l.StatusName ?? "").Equals("Site Visit Scheduled", StringComparison.OrdinalIgnoreCase));
                model.NegotiationLeads = allLeads.Count(l => (l.StatusName ?? "").Equals("Negotiation", StringComparison.OrdinalIgnoreCase));
                model.ClosedWonLeads = allLeads.Count(l => (l.StatusName ?? "").Equals("Closed Won", StringComparison.OrdinalIgnoreCase) || (l.StatusName ?? "").Equals("Booked", StringComparison.OrdinalIgnoreCase));

                // Pipeline funnel counts
                var standardStatuses = new[] { "New", "Contacted", "Interested", "Site Visit Scheduled", "Negotiation", "Closed Won", "Closed Lost" };
                foreach (var st in standardStatuses)
                {
                    model.PipelineCounts[st] = allLeads.Count(l => (l.StatusName ?? "").Equals(st, StringComparison.OrdinalIgnoreCase));
                }

                // Recent Leads (top 10 by ID / CreatedAt desc)
                model.RecentLeads = allLeads.OrderByDescending(l => l.LeadId).Take(10).ToList();

                // Upcoming Follow-ups across scoped leads
                var allComms = new List<LeadCommunicationDto>();
                foreach (var lead in allLeads)
                {
                    var comms = await _leadCommunicationRepository.GetCommunicationsByLeadIdAsync(lead.LeadId);
                    allComms.AddRange(comms.Where(c => c.FollowUpDate.HasValue));
                }

                model.UpcomingFollowUps = allComms
                    .OrderBy(c => c.FollowUpDate)
                    .Take(10)
                    .ToList();

                model.FollowUpsDue = allComms.Count(c => c.FollowUpDate.HasValue && c.FollowUpDate.Value <= DateTime.Today.AddDays(1).AddSeconds(-1));
            }
            catch (Exception ex)
            {
                await LogException(ex);
            }

            return View(model);
        }

        [HttpGet("/Leads")]
        [Authorize]
        public async Task<IActionResult> Leads(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] int? assignedTo,
            [FromQuery] string? source)
        {
            var model = new LeadsPageViewModel
            {
                Search = search,
                StatusFilter = status,
                PriorityFilter = priority,
                AssignedToFilter = assignedTo,
                SourceFilter = source
            };

            try
            {
                List<LeadDto> allLeads;
                var isAdmin = User.IsInRole("Admin");
                var currentUserId = GetCurrentUserId();

                if (isAdmin)
                {
                    allLeads = await _leadRepository.GetAllLeadsAsync();
                }
                else
                {
                    allLeads = await _leadRepository.GetLeadsByAssignedUserAsync(currentUserId);
                    model.AssignedToFilter = currentUserId;
                }

                model.Statuses = await _leadStatusRepository.GetAllStatusesAsync();
                model.Users = await _userRepository.GetAllUsersAsync();

                var query = allLeads.AsEnumerable();

                // Filter by search keyword
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(l =>
                        (l.Name != null && l.Name.ToLower().Contains(term)) ||
                        (l.PhoneNumber != null && l.PhoneNumber.ToLower().Contains(term)) ||
                        (l.Email != null && l.Email.ToLower().Contains(term)) ||
                        (l.PropertyInterest != null && l.PropertyInterest.ToLower().Contains(term)) ||
                        (l.City != null && l.City.ToLower().Contains(term)));
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(l => (l.StatusName ?? "").Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                // Filter by priority
                if (!string.IsNullOrWhiteSpace(priority))
                {
                    query = query.Where(l => (l.Priority ?? "").Equals(priority.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                // Filter by assigned user (Admin only)
                if (isAdmin && assignedTo.HasValue && assignedTo.Value > 0)
                {
                    query = query.Where(l => l.AssignedTo == assignedTo.Value);
                }

                // Filter by source
                if (!string.IsNullOrWhiteSpace(source))
                {
                    query = query.Where(l => (l.Source ?? "").Equals(source.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                model.Leads = query.OrderByDescending(l => l.LeadId).ToList();
            }
            catch (Exception ex)
            {
                await LogException(ex);
            }

            return View(model);
        }

        [HttpGet("/LeadDetails/{id:int}")]
        public async Task<IActionResult> LeadDetails(int id)
        {
            var model = new LeadDetailsPageViewModel();

            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(id);
                if (lead == null)
                {
                    ViewBag.NotFoundId = id;
                    return View("LeadNotFound");
                }

                // Non-admin agents can only view leads assigned to them
                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = GetCurrentUserId();
                    if (lead.AssignedTo != currentUserId)
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                model.Lead = lead;
                model.Communications = await _leadCommunicationRepository.GetCommunicationsByLeadIdAsync(id);
                model.Statuses = await _leadStatusRepository.GetAllStatusesAsync();
                model.Users = await _userRepository.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return View("Error");
            }

            return View(model);
        }

        [HttpPost("/Admin/UpdateLeadStatus")]
        public async Task<IActionResult> UpdateLeadStatus([FromBody] UpdateStatusRequest request)
        {
            if (request == null || request.LeadId <= 0 || request.StatusId <= 0)
            {
                return Json(new { success = false, message = "Invalid lead or status ID." });
            }

            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(request.LeadId);
                if (lead == null)
                {
                    return Json(new { success = false, message = "Lead not found." });
                }

                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = GetCurrentUserId();
                    if (lead.AssignedTo != currentUserId)
                    {
                        return Json(new { success = false, message = "Unauthorized: You can only update leads assigned to you." });
                    }
                }

                var success = await _leadRepository.UpdateLeadAsync(new UpdateLeadDto
                {
                    LeadId = lead.LeadId,
                    Name = lead.Name,
                    PhoneNumber = lead.PhoneNumber,
                    Email = lead.Email,
                    PropertyInterest = lead.PropertyInterest,
                    Budget = lead.Budget,
                    City = lead.City,
                    Source = lead.Source,
                    StatusId = request.StatusId,
                    AssignedTo = lead.AssignedTo,
                    Priority = lead.Priority,
                    Notes = lead.Notes
                });

                var statusObj = await _leadStatusRepository.GetStatusByIdAsync(request.StatusId);

                return Json(new
                {
                    success = success,
                    statusName = statusObj?.StatusName ?? "Updated",
                    message = "Status updated successfully."
                });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to update status." });
            }
        }

        [HttpPost("/Admin/AddLead")]
        public async Task<IActionResult> AddLead([FromBody] CreateLeadDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Lead name is required." });
            }

            if (model.Budget.HasValue && model.Budget.Value < 0)
            {
                return Json(new { success = false, message = "Budget cannot be negative." });
            }

            try
            {
                if (!User.IsInRole("Admin"))
                {
                    model.AssignedTo = GetCurrentUserId();
                }

                if (model.StatusId <= 0)
                {
                    var statuses = await _leadStatusRepository.GetAllStatusesAsync();
                    model.StatusId = statuses.FirstOrDefault()?.StatusId ?? 1;
                }

                var leadId = await _leadRepository.CreateLeadAsync(model);

                if (leadId > 0 && !string.IsNullOrWhiteSpace(model.Notes))
                {
                    await _leadCommunicationRepository.CreateCommunicationAsync(new CreateLeadCommunicationDto
                    {
                        LeadId = leadId,
                        UserId = GetCurrentUserId(),
                        CommunicationType = "Note",
                        Message = model.Notes.Trim()
                    });
                }

                return Json(new { success = true, leadId, message = "Lead created successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to create lead." });
            }
        }

        [HttpPost("/Admin/UpdateLead")]
        public async Task<IActionResult> UpdateLead([FromBody] UpdateLeadDto model)
        {
            if (model == null || model.LeadId <= 0 || string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Valid lead details and name are required." });
            }

            if (model.Budget.HasValue && model.Budget.Value < 0)
            {
                return Json(new { success = false, message = "Budget cannot be negative." });
            }

            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(model.LeadId);
                if (lead == null)
                {
                    return Json(new { success = false, message = "Lead not found." });
                }

                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = GetCurrentUserId();
                    if (lead.AssignedTo != currentUserId)
                    {
                        return Json(new { success = false, message = "Unauthorized: You can only edit leads assigned to you." });
                    }
                    model.AssignedTo = lead.AssignedTo;
                }

                var success = await _leadRepository.UpdateLeadAsync(model);
                return Json(new { success = success, message = "Lead details updated successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to update lead." });
            }
        }

        [HttpPost("/Admin/AddCommunication")]
        public async Task<IActionResult> AddCommunication([FromBody] AddCommunicationRequest request)
        {
            if (request == null || request.LeadId <= 0 || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, message = "Lead ID and communication message are required." });
            }

            try
            {
                var lead = await _leadRepository.GetLeadByIdAsync(request.LeadId);
                if (lead == null)
                {
                    return Json(new { success = false, message = "Lead not found." });
                }

                if (!User.IsInRole("Admin"))
                {
                    var currentUserId = GetCurrentUserId();
                    if (lead.AssignedTo != currentUserId)
                    {
                        return Json(new { success = false, message = "Unauthorized: You can only log communications for leads assigned to you." });
                    }
                }

                DateTime? followUpDateTime = null;
                if (!string.IsNullOrWhiteSpace(request.FollowUpDate))
                {
                    if (DateTime.TryParse(request.FollowUpDate, out DateTime parsedDate))
                    {
                        if (!string.IsNullOrWhiteSpace(request.FollowUpTime) && TimeSpan.TryParse(request.FollowUpTime, out TimeSpan parsedTime))
                        {
                            followUpDateTime = parsedDate.Date.Add(parsedTime);
                        }
                        else
                        {
                            followUpDateTime = parsedDate;
                        }
                    }
                }

                var commId = await _leadCommunicationRepository.CreateCommunicationAsync(new CreateLeadCommunicationDto
                {
                    LeadId = request.LeadId,
                    UserId = GetCurrentUserId(),
                    CommunicationType = string.IsNullOrWhiteSpace(request.CommunicationType) ? "Call" : request.CommunicationType.Trim(),
                    Message = request.Message.Trim(),
                    FollowUpDate = followUpDateTime
                });

                return Json(new { success = true, communicationId = commId, message = "Communication logged successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to log communication." });
            }
        }

        [HttpPost("/Admin/UpdateCommunication")]
        public async Task<IActionResult> UpdateCommunication([FromBody] UpdateCommunicationRequest request)
        {
            if (request == null || request.CommunicationId <= 0 || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, message = "Communication ID and message are required." });
            }

            try
            {
                var comm = await _leadCommunicationRepository.GetCommunicationByIdAsync(request.CommunicationId);
                if (comm == null)
                {
                    return Json(new { success = false, message = "Communication not found." });
                }

                if (!User.IsInRole("Admin"))
                {
                    var lead = await _leadRepository.GetLeadByIdAsync(comm.LeadId);
                    if (lead == null || lead.AssignedTo != GetCurrentUserId())
                    {
                        return Json(new { success = false, message = "Unauthorized: You can only modify communications for leads assigned to you." });
                    }
                }

                DateTime? followUpDateTime = null;
                if (!string.IsNullOrWhiteSpace(request.FollowUpDate))
                {
                    if (DateTime.TryParse(request.FollowUpDate, out DateTime parsedDate))
                    {
                        if (!string.IsNullOrWhiteSpace(request.FollowUpTime) && TimeSpan.TryParse(request.FollowUpTime, out TimeSpan parsedTime))
                        {
                            followUpDateTime = parsedDate.Date.Add(parsedTime);
                        }
                        else
                        {
                            followUpDateTime = parsedDate;
                        }
                    }
                }

                var success = await _leadCommunicationRepository.UpdateCommunicationAsync(new UpdateLeadCommunicationDto
                {
                    CommunicationId = request.CommunicationId,
                    CommunicationType = request.CommunicationType?.Trim(),
                    Message = request.Message.Trim(),
                    FollowUpDate = followUpDateTime
                });

                return Json(new { success = success, message = "Communication updated successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to update communication." });
            }
        }

        [HttpPost("/Admin/DeleteCommunication")]
        public async Task<IActionResult> DeleteCommunication([FromBody] DeleteCommunicationRequest request)
        {
            if (request == null || request.CommunicationId <= 0)
            {
                return Json(new { success = false, message = "Valid communication ID is required." });
            }

            try
            {
                var comm = await _leadCommunicationRepository.GetCommunicationByIdAsync(request.CommunicationId);
                if (comm == null)
                {
                    return Json(new { success = false, message = "Communication not found." });
                }

                if (!User.IsInRole("Admin"))
                {
                    var lead = await _leadRepository.GetLeadByIdAsync(comm.LeadId);
                    if (lead == null || lead.AssignedTo != GetCurrentUserId())
                    {
                        return Json(new { success = false, message = "Unauthorized: You can only delete communications for leads assigned to you." });
                    }
                }

                var success = await _leadCommunicationRepository.DeleteCommunicationAsync(request.CommunicationId);
                return Json(new { success = success, message = "Communication deleted successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to delete communication." });
            }
        }

        #region User Management (Admin Only)

        [HttpGet("/Admin/Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] bool? active)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var model = new UsersPageViewModel
            {
                Search = search,
                RoleFilter = role,
                ActiveFilter = active
            };

            try
            {
                var allUsers = await _userRepository.GetAllUsersAsync();
                var allLeads = await _leadRepository.GetAllLeadsAsync();

                // Exclude current authenticated user from manageable users
                var manageableUsers = allUsers.Where(u => u.UserId != currentUserId).ToList();

                model.TotalManageableCount = manageableUsers.Count;
                model.ActiveManageableCount = manageableUsers.Count(u => u.IsActive);
                model.InactiveManageableCount = manageableUsers.Count(u => !u.IsActive);

                // Compute lead counts per manageable user
                foreach (var u in manageableUsers)
                {
                    model.UserLeadCounts[u.UserId] = allLeads.Count(l => l.AssignedTo == u.UserId);
                }

                var query = manageableUsers.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(u =>
                        (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                        (u.Email != null && u.Email.ToLower().Contains(term)) ||
                        (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(u => (u.Role ?? "").Equals(role.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (active.HasValue)
                {
                    query = query.Where(u => u.IsActive == active.Value);
                }

                model.Users = query.OrderByDescending(u => u.UserId).ToList();
            }
            catch (Exception ex)
            {
                await LogException(ex);
            }

            return View(model);
        }

        [HttpPost("/Admin/Users/Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Full name, email, and password are required." });
            }

            if (model.Password.Length < 6)
            {
                return Json(new { success = false, message = "Password must be at least 6 characters long." });
            }

            try
            {
                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "An account with this email already exists." });
                }

                var userId = await _userRepository.CreateUserAsync(model);
                return Json(new { success = true, userId, message = "Agent created successfully." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to create agent." });
            }
        }

        [HttpPost("/Admin/Users/Update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto model)
        {
            if (model == null || model.UserId <= 0 || string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, message = "User ID, full name, and email are required." });
            }

            try
            {
                if (await _userRepository.EmailExistsAsync(model.Email, model.UserId))
                {
                    return Json(new { success = false, message = "Another account with this email already exists." });
                }

                var success = await _userRepository.UpdateUserAsync(model);
                return Json(new { success, message = success ? "Agent details updated successfully." : "Agent not found." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to update agent." });
            }
        }

        [HttpPost("/Admin/Users/ToggleStatus/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Invalid user ID." });
            }

            if (id == GetCurrentUserId())
            {
                return Json(new { success = false, message = "You cannot deactivate your own administrative account." });
            }

            try
            {
                var newStatus = await _userRepository.ToggleActiveStatusAsync(id);
                return Json(new { success = true, isActive = newStatus, message = $"Account status changed to {(newStatus ? "Active" : "Inactive")}." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to update account status." });
            }
        }

        [HttpPost("/Admin/Users/ResetPassword")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (model == null || model.UserId <= 0 || string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters long." });
            }

            try
            {
                var success = await _userRepository.ResetPasswordAsync(model.UserId, model.NewPassword);
                return Json(new { success, message = success ? "Password reset successfully." : "User not found." });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to reset password." });
            }
        }

        [HttpGet("/Admin/Users/Details/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var allLeads = await _leadRepository.GetAllLeadsAsync();
                var userLeads = allLeads.Where(l => l.AssignedTo == id).ToList();

                return Json(new
                {
                    success = true,
                    user = new
                    {
                        user.UserId,
                        user.FullName,
                        user.PhoneNumber,
                        user.Email,
                        user.Role,
                        user.IsActive,
                        CreatedAt = user.CreatedAt.ToString("MMM dd, yyyy"),
                        AssignedLeadCount = userLeads.Count,
                        ActiveLeadCount = userLeads.Count(l => (l.StatusName ?? "") != "Closed Lost" && (l.StatusName ?? "") != "Closed Won"),
                        ClosedWonCount = userLeads.Count(l => (l.StatusName ?? "").Equals("Closed Won", StringComparison.OrdinalIgnoreCase) || (l.StatusName ?? "").Equals("Booked", StringComparison.OrdinalIgnoreCase))
                    }
                });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new { success = false, message = "Failed to fetch user details." });
            }
        }

        #endregion
    }

    public class UpdateStatusRequest
    {
        public int LeadId { get; set; }
        public int StatusId { get; set; }
    }

    public class AddCommunicationRequest
    {
        public int LeadId { get; set; }
        public string CommunicationType { get; set; } = "Call";
        public string Message { get; set; } = string.Empty;
        public string? FollowUpDate { get; set; }
        public string? FollowUpTime { get; set; }
    }

    public class UpdateCommunicationRequest
    {
        public int CommunicationId { get; set; }
        public string? CommunicationType { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FollowUpDate { get; set; }
        public string? FollowUpTime { get; set; }
    }

    public class DeleteCommunicationRequest
    {
        public int CommunicationId { get; set; }
    }
}

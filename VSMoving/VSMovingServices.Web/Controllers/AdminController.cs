using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;

namespace VSMovingServices.Web.Controllers;

[Authorize(Roles = "Admin,Agent")]
public class AdminController : Controller
{
    private readonly VSMovingDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(VSMovingDbContext db, ILogger<AdminController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("/AdminPanel")]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;

        var totalLeads = await _db.Leads.CountAsync();
        var newLeads = await _db.Leads.CountAsync(l => !l.IsRead);
        var readLeads = await _db.Leads.CountAsync(l => l.IsRead);
        var todaysLeads = await _db.Leads.CountAsync(l => l.CreatedAt >= today);

        var recentLeads = await _db.Leads
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.LeadId)
            .Take(8)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalLeads = totalLeads,
            NewLeads = newLeads,
            ReadLeads = readLeads,
            TodaysLeads = todaysLeads,
            RecentLeads = recentLeads
        };

        return View(vm);
    }

    [HttpGet("/Leads")]
    public async Task<IActionResult> Leads(
        string? search = null,
        string? status = null,
        int page = 1,
        int pageSize = 10)
    {
        var allCount = await _db.Leads.CountAsync();
        var newCount = await _db.Leads.CountAsync(l => !l.IsRead);
        var readCount = await _db.Leads.CountAsync(l => l.IsRead);

        var query = _db.Leads.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, "new", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => !l.IsRead);
            }
            else if (string.Equals(status, "read", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.IsRead);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var lower = term.ToLower();
            var numericTerm = term.TrimStart('#');
            if (numericTerm.StartsWith("LD-", StringComparison.OrdinalIgnoreCase))
            {
                numericTerm = numericTerm.Substring(3).TrimStart('0');
            }
            bool isIdSearch = int.TryParse(numericTerm, out int parsedId);

            query = query.Where(l =>
                l.FullName.ToLower().Contains(lower) ||
                (l.PhoneNumber != null && l.PhoneNumber.ToLower().Contains(lower)) ||
                (l.Email != null && l.Email.ToLower().Contains(lower)) ||
                (isIdSearch && l.LeadId == parsedId));
        }

        var totalCount = await query.CountAsync();
        if (page < 1) page = 1;
        if (pageSize < 5) pageSize = 10;

        var leads = await query
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.LeadId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new LeadsListViewModel
        {
            Leads = leads,
            Search = search,
            StatusFilter = status,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            AllCount = allCount,
            NewCount = newCount,
            ReadCount = readCount
        };

        return View(vm);
    }

    [HttpGet("/LeadDetails/{id:int}")]
    public async Task<IActionResult> LeadDetails(int id)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.LeadId == id);
        if (lead == null)
        {
            Response.StatusCode = 404;
            return View("LeadNotFound", id);
        }

        // Server-side Mark Lead as Read when opened by an authorized user
        if (!lead.IsRead)
        {
            lead.IsRead = true;
            lead.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        return View(lead);
    }
}

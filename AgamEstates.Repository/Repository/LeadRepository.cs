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
    public class LeadRepository : RepositoryBase<AgamEntities>, ILeadRepository
    {
        public LeadRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public async Task<List<LeadDto>> GetAllLeadsAsync()
        {
            return await DataContext.Leads
                .AsNoTracking()
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .Select(l => new LeadDto
                {
                    LeadId = l.LeadId,
                    Name = l.Name,
                    PhoneNumber = l.PhoneNumber,
                    Email = l.Email,
                    PropertyInterest = l.PropertyInterest,
                    Budget = l.Budget,
                    City = l.City,
                    Source = l.Source,
                    StatusId = l.StatusId,
                    StatusName = l.Status != null ? l.Status.StatusName : null,
                    AssignedTo = l.AssignedTo,
                    AssignedToName = l.AssignedUser != null ? l.AssignedUser.FullName : null,
                    Priority = l.Priority,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<(List<LeadDto> Leads, int TotalCount, int CurrentPage, int TotalPages)> GetPagedLeadsAsync(
            string? search,
            string? status,
            string? priority,
            int? assignedTo,
            string? source,
            bool isAdmin,
            int currentUserId,
            int page = 1,
            int pageSize = 10)
        {
            if (pageSize < 1)
            {
                pageSize = 10;
            }

            var query = DataContext.Leads
                .AsNoTracking()
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(l => l.AssignedTo == currentUserId);
            }
            else if (assignedTo.HasValue && assignedTo.Value > 0)
            {
                query = query.Where(l => l.AssignedTo == assignedTo.Value);
            }

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

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusTerm = status.Trim().ToLower();
                query = query.Where(l => l.Status != null && l.Status.StatusName != null && l.Status.StatusName.ToLower() == statusTerm);
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                var priorityTerm = priority.Trim().ToLower();
                query = query.Where(l => l.Priority != null && l.Priority.ToLower() == priorityTerm);
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                var sourceTerm = source.Trim().ToLower();
                query = query.Where(l => l.Source != null && l.Source.ToLower() == sourceTerm);
            }

            var totalCount = await query.CountAsync();
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages && totalPages > 0)
            {
                page = totalPages;
            }

            var pagedLeads = await query
                .OrderByDescending(l => l.LeadId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeadDto
                {
                    LeadId = l.LeadId,
                    Name = l.Name,
                    PhoneNumber = l.PhoneNumber,
                    Email = l.Email,
                    PropertyInterest = l.PropertyInterest,
                    Budget = l.Budget,
                    City = l.City,
                    Source = l.Source,
                    StatusId = l.StatusId,
                    StatusName = l.Status != null ? l.Status.StatusName : null,
                    AssignedTo = l.AssignedTo,
                    AssignedToName = l.AssignedUser != null ? l.AssignedUser.FullName : null,
                    Priority = l.Priority,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return (pagedLeads, totalCount, page, totalPages);
        }

        public async Task<LeadDto?> GetLeadByIdAsync(int id)
        {
            var lead = await DataContext.Leads
                .AsNoTracking()
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return null;

            return new LeadDto
            {
                LeadId = lead.LeadId,
                Name = lead.Name,
                PhoneNumber = lead.PhoneNumber,
                Email = lead.Email,
                PropertyInterest = lead.PropertyInterest,
                Budget = lead.Budget,
                City = lead.City,
                Source = lead.Source,
                StatusId = lead.StatusId,
                StatusName = lead.Status?.StatusName,
                AssignedTo = lead.AssignedTo,
                AssignedToName = lead.AssignedUser?.FullName,
                Priority = lead.Priority,
                Notes = lead.Notes,
                CreatedAt = lead.CreatedAt
            };
        }

        public async Task<List<LeadDto>> GetLeadsByStatusAsync(int statusId)
        {
            return await DataContext.Leads
                .AsNoTracking()
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .Where(l => l.StatusId == statusId)
                .Select(l => new LeadDto
                {
                    LeadId = l.LeadId,
                    Name = l.Name,
                    PhoneNumber = l.PhoneNumber,
                    Email = l.Email,
                    PropertyInterest = l.PropertyInterest,
                    Budget = l.Budget,
                    City = l.City,
                    Source = l.Source,
                    StatusId = l.StatusId,
                    StatusName = l.Status != null ? l.Status.StatusName : null,
                    AssignedTo = l.AssignedTo,
                    AssignedToName = l.AssignedUser != null ? l.AssignedUser.FullName : null,
                    Priority = l.Priority,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<LeadDto>> GetLeadsByAssignedUserAsync(int userId)
        {
            return await DataContext.Leads
                .AsNoTracking()
                .Include(l => l.Status)
                .Include(l => l.AssignedUser)
                .Where(l => l.AssignedTo == userId)
                .Select(l => new LeadDto
                {
                    LeadId = l.LeadId,
                    Name = l.Name,
                    PhoneNumber = l.PhoneNumber,
                    Email = l.Email,
                    PropertyInterest = l.PropertyInterest,
                    Budget = l.Budget,
                    City = l.City,
                    Source = l.Source,
                    StatusId = l.StatusId,
                    StatusName = l.Status != null ? l.Status.StatusName : null,
                    AssignedTo = l.AssignedTo,
                    AssignedToName = l.AssignedUser != null ? l.AssignedUser.FullName : null,
                    Priority = l.Priority,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> CreateLeadAsync(CreateLeadDto model)
        {
            var lead = new Lead
            {
                Name = model.Name.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                Email = model.Email?.Trim().ToLower(),
                PropertyInterest = model.PropertyInterest?.Trim(),
                Budget = model.Budget,
                City = model.City?.Trim(),
                Source = model.Source?.Trim(),
                StatusId = model.StatusId,
                AssignedTo = model.AssignedTo,
                Priority = string.IsNullOrWhiteSpace(model.Priority) ? "Medium" : model.Priority.Trim(),
                Notes = model.Notes?.Trim(),
                CreatedAt = GetIstTimeNow()
            };

            DataContext.Leads.Add(lead);
            await DataContext.SaveChangesAsync();

            return lead.LeadId;
        }

        public async Task<bool> UpdateLeadAsync(UpdateLeadDto model)
        {
            var lead = await DataContext.Leads.FirstOrDefaultAsync(l => l.LeadId == model.LeadId);
            if (lead == null) return false;

            lead.Name = model.Name.Trim();
            lead.PhoneNumber = model.PhoneNumber?.Trim();
            lead.Email = model.Email?.Trim().ToLower();
            lead.PropertyInterest = model.PropertyInterest?.Trim();
            lead.Budget = model.Budget;
            lead.City = model.City?.Trim();
            lead.Source = model.Source?.Trim();
            lead.StatusId = model.StatusId;
            lead.AssignedTo = model.AssignedTo;
            if (!string.IsNullOrWhiteSpace(model.Priority))
            {
                lead.Priority = model.Priority.Trim();
            }
            lead.Notes = model.Notes?.Trim();

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLeadAsync(int id)
        {
            var lead = await DataContext.Leads.FirstOrDefaultAsync(l => l.LeadId == id);
            if (lead == null) return false;

            DataContext.Leads.Remove(lead);
            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeadExistsAsync(int id)
        {
            return await DataContext.Leads.AnyAsync(l => l.LeadId == id);
        }
    }
}

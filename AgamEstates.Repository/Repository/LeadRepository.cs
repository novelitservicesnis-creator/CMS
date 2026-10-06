using AgamEstates.Core;
using AgamEstates.Core.Models;
using AgamEstates.Repository.Base;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using Microsoft.EntityFrameworkCore;
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

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
    public class LeadStatusRepository : RepositoryBase<AgamEntities>, ILeadStatusRepository
    {
        public LeadStatusRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public async Task<List<LeadStatusDto>> GetAllStatusesAsync()
        {
            return await DataContext.LeadStatus
                .AsNoTracking()
                .Select(s => new LeadStatusDto
                {
                    StatusId = s.StatusId,
                    StatusName = s.StatusName,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        public async Task<LeadStatusDto?> GetStatusByIdAsync(int id)
        {
            var status = await DataContext.LeadStatus
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatusId == id);

            if (status == null) return null;

            return new LeadStatusDto
            {
                StatusId = status.StatusId,
                StatusName = status.StatusName,
                Description = status.Description,
                IsActive = status.IsActive
            };
        }

        public async Task<int> CreateStatusAsync(CreateLeadStatusDto model)
        {
            var status = new LeadStatus
            {
                StatusName = model.StatusName.Trim(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive
            };

            DataContext.LeadStatus.Add(status);
            await DataContext.SaveChangesAsync();

            return status.StatusId;
        }

        public async Task<bool> UpdateStatusAsync(UpdateLeadStatusDto model)
        {
            var status = await DataContext.LeadStatus.FirstOrDefaultAsync(s => s.StatusId == model.StatusId);
            if (status == null) return false;

            status.StatusName = model.StatusName.Trim();
            status.Description = model.Description?.Trim();
            status.IsActive = model.IsActive;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StatusExistsAsync(int id)
        {
            return await DataContext.LeadStatus.AnyAsync(s => s.StatusId == id && s.IsActive);
        }
    }
}

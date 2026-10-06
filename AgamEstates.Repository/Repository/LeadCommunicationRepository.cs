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
    public class LeadCommunicationRepository : RepositoryBase<AgamEntities>, ILeadCommunicationRepository
    {
        public LeadCommunicationRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public async Task<List<LeadCommunicationDto>> GetCommunicationsByLeadIdAsync(int leadId)
        {
            return await DataContext.LeadCommunication
                .AsNoTracking()
                .Include(c => c.Lead)
                .Include(c => c.User)
                .Where(c => c.LeadId == leadId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new LeadCommunicationDto
                {
                    CommunicationId = c.CommunicationId,
                    LeadId = c.LeadId,
                    LeadName = c.Lead != null ? c.Lead.Name : null,
                    UserId = c.UserId,
                    UserName = c.User != null ? c.User.FullName : null,
                    Message = c.Message,
                    CommunicationType = c.CommunicationType,
                    FollowUpDate = c.FollowUpDate,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<LeadCommunicationDto?> GetCommunicationByIdAsync(int id)
        {
            var comm = await DataContext.LeadCommunication
                .AsNoTracking()
                .Include(c => c.Lead)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommunicationId == id);

            if (comm == null) return null;

            return new LeadCommunicationDto
            {
                CommunicationId = comm.CommunicationId,
                LeadId = comm.LeadId,
                LeadName = comm.Lead?.Name,
                UserId = comm.UserId,
                UserName = comm.User?.FullName,
                Message = comm.Message,
                CommunicationType = comm.CommunicationType,
                FollowUpDate = comm.FollowUpDate,
                CreatedAt = comm.CreatedAt
            };
        }

        public async Task<int> CreateCommunicationAsync(CreateLeadCommunicationDto model)
        {
            var comm = new LeadCommunication
            {
                LeadId = model.LeadId,
                UserId = model.UserId,
                Message = model.Message?.Trim(),
                CommunicationType = model.CommunicationType?.Trim(),
                FollowUpDate = model.FollowUpDate,
                CreatedAt = GetIstTimeNow()
            };

            DataContext.LeadCommunication.Add(comm);
            await DataContext.SaveChangesAsync();

            return comm.CommunicationId;
        }

        public async Task<bool> UpdateCommunicationAsync(UpdateLeadCommunicationDto model)
        {
            var comm = await DataContext.LeadCommunication.FirstOrDefaultAsync(c => c.CommunicationId == model.CommunicationId);
            if (comm == null) return false;

            comm.Message = model.Message?.Trim();
            comm.CommunicationType = model.CommunicationType?.Trim();
            comm.FollowUpDate = model.FollowUpDate;

            await DataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommunicationAsync(int id)
        {
            var comm = await DataContext.LeadCommunication.FirstOrDefaultAsync(c => c.CommunicationId == id);
            if (comm == null) return false;

            DataContext.LeadCommunication.Remove(comm);
            await DataContext.SaveChangesAsync();
            return true;
        }
    }
}

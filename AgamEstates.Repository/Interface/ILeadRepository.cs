using AgamEstates.Repository.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface ILeadRepository
    {
        Task<List<LeadDto>> GetAllLeadsAsync();
        Task<LeadDto?> GetLeadByIdAsync(int id);
        Task<List<LeadDto>> GetLeadsByStatusAsync(int statusId);
        Task<List<LeadDto>> GetLeadsByAssignedUserAsync(int userId);
        Task<int> CreateLeadAsync(CreateLeadDto model);
        Task<bool> UpdateLeadAsync(UpdateLeadDto model);
        Task<bool> DeleteLeadAsync(int id);
        Task<bool> LeadExistsAsync(int id);
    }
}

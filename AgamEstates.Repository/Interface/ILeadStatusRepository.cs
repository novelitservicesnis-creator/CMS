using AgamEstates.Repository.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface ILeadStatusRepository
    {
        Task<List<LeadStatusDto>> GetAllStatusesAsync();
        Task<LeadStatusDto?> GetStatusByIdAsync(int id);
        Task<int> CreateStatusAsync(CreateLeadStatusDto model);
        Task<bool> UpdateStatusAsync(UpdateLeadStatusDto model);
        Task<bool> StatusExistsAsync(int id);
    }
}

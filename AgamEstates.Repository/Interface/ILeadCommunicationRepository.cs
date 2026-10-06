using AgamEstates.Repository.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface ILeadCommunicationRepository
    {
        Task<List<LeadCommunicationDto>> GetCommunicationsByLeadIdAsync(int leadId);
        Task<LeadCommunicationDto?> GetCommunicationByIdAsync(int id);
        Task<int> CreateCommunicationAsync(CreateLeadCommunicationDto model);
        Task<bool> UpdateCommunicationAsync(UpdateLeadCommunicationDto model);
        Task<bool> DeleteCommunicationAsync(int id);
    }
}

using AgamEstates.Repository.ViewModel;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public interface ISystemSettingsService
    {
        Task<PublicSiteSettingsViewModel> GetPublicSettingsAsync();
        void InvalidateCache();
    }
}

using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemSettingsRepository _repository;
        private readonly IMemoryCache _cache;
        public const string CacheKey = "AgamEstates_Public_SystemSettings";

        public SystemSettingsService(ISystemSettingsRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<PublicSiteSettingsViewModel> GetPublicSettingsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out PublicSiteSettingsViewModel? settings) || settings == null)
            {
                settings = await _repository.GetPublicSiteSettingsAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(4));
                _cache.Set(CacheKey, settings, cacheOptions);
            }
            return settings;
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}

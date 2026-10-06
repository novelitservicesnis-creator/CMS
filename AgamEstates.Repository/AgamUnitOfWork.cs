using AgamEstates.Core;
using AgamEstates.Repository.Base;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.Repository;
using Microsoft.Extensions.Configuration;

namespace AgamEstates.Repository
{
    public class AgamUnitOfWork : UnitOfWorkBase<AgamEntities>
    {
        public readonly AgamEntities Entity;
        public readonly IConfiguration? Configuration;

        public AgamUnitOfWork(AgamEntities entity, IConfiguration? configuration = null)
        {
            this.Entity = entity;
            this.Configuration = configuration;
        }

        private IUserRepository? _userRepository;
        private ILeadStatusRepository? _leadStatusRepository;
        private ILeadRepository? _leadRepository;
        private ILeadCommunicationRepository? _leadCommunicationRepository;
        private ISystemSettingsRepository? _systemSettingsRepository;

        public IUserRepository UserRepository => _userRepository ??= new UserRepository(Entity);
        public ILeadStatusRepository LeadStatusRepository => _leadStatusRepository ??= new LeadStatusRepository(Entity);
        public ILeadRepository LeadRepository => _leadRepository ??= new LeadRepository(Entity);
        public ILeadCommunicationRepository LeadCommunicationRepository => _leadCommunicationRepository ??= new LeadCommunicationRepository(Entity);
        public ISystemSettingsRepository SystemSettingsRepository => _systemSettingsRepository ??= new SystemSettingsRepository(Entity);
    }
}

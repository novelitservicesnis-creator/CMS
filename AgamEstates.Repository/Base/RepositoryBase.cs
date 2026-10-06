using Microsoft.EntityFrameworkCore;
using System;

namespace AgamEstates.Repository.Base
{
    public class RepositoryBase<C> : IDisposable where C : DbContext, new()
    {
        protected C? _DataContext;

        protected DateTime GetIstTimeNow()
        {
            TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indianZone);
            return indianTime;
        }

        public virtual C DataContext
        {
            get
            {
                if (_DataContext == null)
                {
                    _DataContext = new C();
                }
                return _DataContext;
            }
            set { _DataContext = value; }
        }

        public RepositoryBase()
        {
        }

        public RepositoryBase(C dataContext)
        {
            _DataContext = dataContext;
        }

        public void Dispose()
        {
            if (DataContext != null)
            {
                DataContext.Dispose();
            }
        }
    }
}

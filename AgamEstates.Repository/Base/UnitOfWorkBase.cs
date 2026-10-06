using Microsoft.EntityFrameworkCore;
using System;

namespace AgamEstates.Repository.Base
{
    public abstract class UnitOfWorkBase<C> : IDisposable where C : DbContext, new()
    {
        private bool disposed = false;
        protected C DataContext = new C();

        public UnitOfWorkBase()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DataContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBClassLibrary.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void Save();
        Task SaveAsync();
        IRepository<T> Repository<T>() where T : class;
    }
}

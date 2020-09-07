using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DBClassLibrary.Interfaces
{
    public interface IRepository<TEntity> //:IDisposable 
        where TEntity : class
    {
        TEntity GetById(object[] Id);
        Task<TEntity> GetByIdAsync(object[] id);
        TEntity Get(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> GetAll();
        void Create(TEntity instance);
        void Update(TEntity instance);
        void Delete(TEntity instance);
      
        void SaveChanges();
    }
}

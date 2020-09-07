using DBClassLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DBClassLibrary
{
    public class SqlRepository<T> : IRepository<T> where T : class
    {
        BloggingContext _db;
        public SqlRepository(BloggingContext db)
        {
            this._db = db;
        }
        public void Create(T instance)
        {
            _db.Set<T>().Add(instance);
        }

        public void Delete(T instance)
        {
            _db.Entry<T>(instance).State = EntityState.Deleted;
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return _db.Set<T>().Where(predicate).FirstOrDefault();
        }

        public IQueryable<T> GetAll()
        {
            return _db.Set<T>().AsQueryable();
        }

        public T GetById(object[] Id)
        {
            return _db.Set<T>().Find(Id);
        }

        public void Update(T instance)
        {
            _db.Entry<T>(instance).State = EntityState.Modified;
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public async Task<T> GetByIdAsync(object[] id)
        {
            return await _db.Set<T>().FindAsync(id);
        }
    }
}

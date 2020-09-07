using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IClassAllBLL
    {
        Task<ClassAll> GetByIdAsync(string userId, string classId);
        IQueryable<ClassAll> GetAll();
        void Create(ClassAll instance);
        void Update(ClassAll instance);
        void Delete(ClassAll instance);
        Task SaveAsync();
        Task<string> GetMaxClassID(string userId);
        bool ClassIdExists(string userId, string classId);
    }
}

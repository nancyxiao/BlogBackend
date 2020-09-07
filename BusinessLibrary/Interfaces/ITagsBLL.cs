using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
   public  interface ITagsBLL
    {
        Task<Tags> GetByIdAsync(string userId, string tagId);
        IQueryable<Tags> GetAll();
        void Create(Tags instance);
        void Update(Tags instance);
        void Delete(Tags instance);
        Task SaveAsync();
        Task<string> GetMaxTagID(string userId);
        bool TagIdExists(string userId, string tagId);
    }
}

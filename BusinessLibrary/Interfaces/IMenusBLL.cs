using BlogViewModels;
using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IMenusBLL
    {
        Menus GetById(string menuID);
        Task<Menus> GetByIdAsync(string menuID);
        IQueryable<Menus> GetAll();
        void Create(Menus instance);
        void Update(Menus instance);
        void Delete(Menus instance);
        void Save();
        Task SaveAsync();
        Task<string> GetMaxMenuID();
        bool MenuExists(string menuid);
    }
}

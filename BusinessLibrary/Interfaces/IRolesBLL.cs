using BlogViewModels;
using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IRolesBLL
    {
        Task<Roles> GetByIdAsync(string roleID);
        IQueryable<Roles> GetAll();
        void Create(Roles instance);
        void CreateMenus(List<RoleMenuMapping> menus);
        void Update(Roles instance);
        void Delete(Roles instance);
        void DeleteMenus(List<RoleMenuMapping> menus);
        IQueryable<RoleMenuMapping> GetMenuMapping(string roleID);
        Task SaveAsync();
        Task<string> GetMaxRoleID();
        bool RoleExists(string roleid);
    }
}

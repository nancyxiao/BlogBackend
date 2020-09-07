using BlogViewModels;
using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessLibrary
{
    public class RolesService : IRolesBLL
    {
        private readonly ILogger<RolesService> _logger;
        private IUnitOfWork _unitOfWork;


        public RolesService(ILogger<RolesService> logger, IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }
        public void Create(Roles instance)
        {
            this._unitOfWork.Repository<Roles>().Create(instance);
        }

        public void CreateMenus(List<RoleMenuMapping> menus)
        {
            foreach(var menu in menus)
            {
                this._unitOfWork.Repository<RoleMenuMapping>().Create(menu);
            }
        }

        public void Delete(Roles instance)
        {
            this._unitOfWork.Repository<Roles>().Delete(instance);
        }

        public void DeleteMenus(List<RoleMenuMapping> menus)
        {
            foreach (var menu in menus)
            {
                this._unitOfWork.Repository<RoleMenuMapping>().Delete(menu);
            }
        }

        public IQueryable<Roles> GetAll()
        {
            return this._unitOfWork.Repository<Roles>().GetAll();
        }

        public async Task<Roles> GetByIdAsync(string roleID)
        {
            return await this._unitOfWork.Repository<Roles>().GetByIdAsync(new object[] { roleID });
        }

        public async Task<string> GetMaxRoleID()
        {
            string currentMaxRoleID = await this._unitOfWork.Repository<Roles>()
                                                          .GetAll()
                                                          .Where(x => x.RoleId != "999" && x.RoleId != "998")
                                                          .MaxAsync(x => x.RoleId);
            int addOne = 0;
            int.TryParse(currentMaxRoleID, out addOne);
            return string.Format("{0:000}", addOne + 1);
        }

        public IQueryable<RoleMenuMapping> GetMenuMapping(string roleID)
        {
             return this._unitOfWork.Repository<RoleMenuMapping>().GetAll()
                .Where(m => m.RoleId == roleID);
        }

        public bool RoleExists(string roleid)
        {
            return this._unitOfWork.Repository<Roles>().GetAll().Any(x => x.RoleId == roleid);
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public void Update(Roles instance)
        {
            this._unitOfWork.Repository<Roles>().Update(instance);
        }
    }
}

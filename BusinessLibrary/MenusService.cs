using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class MenusService : IMenusBLL
    {
        private readonly ILogger<MenusService> _logger;
        private IUnitOfWork _unitOfWork;

        public MenusService(ILogger<MenusService> logger, IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }
        public void Create(Menus instance)
        {
            this._unitOfWork.Repository<Menus>().Create(instance);
        }

        public void Delete(Menus instance)
        {
            this._unitOfWork.Repository<Menus>().Delete(instance);
        }

        public IQueryable<Menus> GetAll()
        {
            return this._unitOfWork.Repository<Menus>().GetAll();
        }

        public Menus GetById(string menuID)
        {
            throw new NotImplementedException();
        }

        public async Task<Menus> GetByIdAsync(string menuID)
        {
            return await this._unitOfWork.Repository<Menus>().GetByIdAsync(new object[] { menuID });
        }

        public async Task<string> GetMaxMenuID()
        {
           string currentMaxMenuID =  await this._unitOfWork.Repository<Menus>()
                                                            .GetAll()
                                                            .Where(x => x.MenuId != "999")
                                                            .MaxAsync(x => x.MenuId);
            int addOne = 0;
            int.TryParse(currentMaxMenuID, out addOne);
            return string.Format("{0:000}", addOne + 1);
        }

        public bool MenuExists(string menuid)
        {
            return this._unitOfWork.Repository<Menus>().GetAll().Any(x => x.MenuId == menuid);
        }

        public void Save()
        {
            this._unitOfWork.Save();
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public void Update(Menus instance)
        {
            this._unitOfWork.Repository<Menus>().Update(instance);
        }

    }
}

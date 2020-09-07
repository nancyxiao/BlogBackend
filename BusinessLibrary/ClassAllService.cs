using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class ClassAllService : IClassAllBLL
    {
        private readonly ILogger<ClassAllService> _logger;
        private IUnitOfWork _unitOfWork;

        public ClassAllService(ILogger<ClassAllService> logger, IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }

        public bool ClassIdExists(string userId, string classId)
        {
            return this._unitOfWork.Repository<ClassAll>().GetAll().Any(x => x.UserId == userId && x.ClassId == classId);
        }

        public void Create(ClassAll instance)
        {
            this._unitOfWork.Repository<ClassAll>().Create(instance);
        }

        public void Delete(ClassAll instance)
        {
            this._unitOfWork.Repository<ClassAll>().Delete(instance);
        }

        public IQueryable<ClassAll> GetAll()
        {
            return this._unitOfWork.Repository<ClassAll>().GetAll();
        }

        public async Task<ClassAll> GetByIdAsync(string userId,string classId)
        {
            return await this._unitOfWork.Repository<ClassAll>().GetByIdAsync(new object[] {  userId, classId  });
        }

        public async Task<string> GetMaxClassID(string userId)
        {
            string currentMaxID = await this._unitOfWork.Repository<ClassAll>()
                                                          .GetAll()
                                                          .Where(x=>x.UserId == userId)
                                                          .MaxAsync(x => x.ClassId);
            int addOne = 0;
            int.TryParse(currentMaxID, out addOne);
            return string.Format("{0:000}", addOne + 1);
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public void Update(ClassAll instance)
        {
            this._unitOfWork.Repository<ClassAll>().Update(instance);
        }
    }
}

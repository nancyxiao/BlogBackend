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

    public class TagsService : ITagsBLL
    {
        private readonly ILogger<ClassAllService> _logger;
        private IUnitOfWork _unitOfWork;
        public TagsService(ILogger<ClassAllService> logger, IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }
        public bool TagIdExists(string userId, string tagId)
        {
            return this._unitOfWork.Repository<Tags>().GetAll().Any(x => x.UserId == userId && x.TagId == tagId);
        }

        public void Create(Tags instance)
        {
            this._unitOfWork.Repository<Tags>().Create(instance);
        }

        public void Delete(Tags instance)
        {
            this._unitOfWork.Repository<Tags>().Delete(instance);
        }

        public IQueryable<Tags> GetAll()
        {
            return this._unitOfWork.Repository<Tags>().GetAll();
        }

        public async Task<Tags> GetByIdAsync(string userId, string tagId)
        {
            return await this._unitOfWork.Repository<Tags>().GetByIdAsync(new object[] { userId, tagId });
        }

        public async Task<string> GetMaxTagID(string userId)
        {
            string currentMaxID = await this._unitOfWork.Repository<Tags>()
                                                       .GetAll()
                                                       .Where(x => x.UserId == userId)
                                                       .MaxAsync(x => x.TagId);
            int addOne = 0;
            int.TryParse(currentMaxID, out addOne);
            return string.Format("{0:000}", addOne + 1);
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public void Update(Tags instance)
        {
            this._unitOfWork.Repository<Tags>().Update(instance);
        }
    }
}

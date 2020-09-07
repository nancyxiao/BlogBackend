using BlogViewModels;
using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IUserIntroBLL
    {
        Task<UserIntroduction> GetByIdAsync(string userID);
        IQueryable<UserIntroduction> GetAll();

        void Create(UserIntroViewModel instance);
        void Update(UserIntroViewModel instance);
        void Delete(UserIntroViewModel instance);

        bool UserIntroExists(string userid);

        Task SaveAsync();
    }
}

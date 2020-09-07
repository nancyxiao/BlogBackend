using BlogViewModels;
using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IMembersBLL
    {
        Task<Members> GetByIdAsync(string userID);
        Task<MembersViewModel> GetViewByIdAsync(Members member, IRolesBLL _rolesBLL, string state);
        Members GetById(string userID);
        Members Get(Expression<Func<Members, bool>> predicate);
        IQueryable<MembersViewModel> GetAll(IRolesBLL _rolesBLL);
        void Create(MembersViewModel instance);
        void Update(MembersViewModel instance);
        void Delete(Members instance);
        bool IsOkForUsers(ref Members member, ref string errmsg);

        List<Claim> GetClaims(Members member);

        List<RoleSelect> GetRoleSelect(IRolesBLL _rolesBLL);

        bool UserExists(string userID);
        bool EmailExists(string userID, string userEmail);
        Task SaveAsync();
    }
}

using DBClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BusinessLibrary
{
    public interface ILoginBLL
    {
        LoginRecords Get(Expression<Func<LoginRecords, bool>> predicate);
        IQueryable<LoginRecords> GetAll();
        void CreateByParam(string userId, Enum loginState, DateTime utcDateTime);
        void Create(LoginRecords instance);

    }
}

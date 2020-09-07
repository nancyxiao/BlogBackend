using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BusinessLibrary
{
    public class LoginRecordService : ILoginBLL
    {
        private readonly ILogger<LoginRecordService> _logger;
        private IUnitOfWork _unitOfWork;
        public LoginRecordService(ILogger<LoginRecordService> logger,
            IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }
        public void CreateByParam(string userId, Enum loginState, DateTime utcDateTime)
        {
            this.Create(new LoginRecords
            {
                 UserId = userId,
                 LoginState = loginState.ToString(),
                 LoginTime = utcDateTime
            });
        }
        public void Create(LoginRecords instance)
        {
            try
            {
                this._unitOfWork.Repository<LoginRecords>().Create(instance);
                this._unitOfWork.Save();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.Create() .", ex);
            }
        }

        public LoginRecords Get(Expression<Func<LoginRecords, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<LoginRecords> GetAll()
        {
            throw new NotImplementedException();
        }
    }

    public enum LoginState
    {
        LoginSuccess,
        LoginFaild
    }
}


using BlogViewModels;
using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class MembersService : IMembersBLL
    {
        private readonly ILogger<MembersService> _logger;
        private readonly IConfiguration _Configuration;
        private IUnitOfWork _unitOfWork;
        private readonly IRolesBLL _rolesBLL;


        public MembersService(ILogger<MembersService> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IRolesBLL rolesBLL)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
            this._Configuration = configuration;
            this._rolesBLL = rolesBLL;
        }

        public void Create(MembersViewModel instance)
        {
            Members model = new Members();
            model.UserId = instance.UserId;
            model.UserName = instance.UserName;
            model.UserEmail = instance.UserEmail;

            if (instance.isUpdatePwd == true)
            {
                var loginKey = this._Configuration["Blog:LoginKey"];
                var encryptPwd = StringEncrypt.aesEncryptBase64(instance.UserPwd, loginKey);

                model.UserPwd = encryptPwd;
            }

            model.RoleId = instance.RoleID;
            model.IsEmailValid = instance.IsEmailValid;
            model.IsThirdLogin = instance.IsThirdLogin;

            this._unitOfWork.Repository<Members>().Create(model);
        }
        public void Update(MembersViewModel instance)
        {
            var model = this.GetByIdAsync(instance.UserId).GetAwaiter().GetResult();
            //Members model = new Members();
            //model.UserId = instance.UserId;
            model.UserName = instance.UserName;
            model.UserEmail = instance.UserEmail;

            if (instance.isUpdatePwd == true)
            {
                var loginKey = this._Configuration["Blog:LoginKey"];
                var encryptPwd = StringEncrypt.aesEncryptBase64(instance.UserPwd, loginKey);

                model.UserPwd = encryptPwd;
            }

            model.RoleId = instance.RoleID;
            model.IsEmailValid = instance.IsEmailValid;
            model.IsThirdLogin = instance.IsThirdLogin;

            this._unitOfWork.Repository<Members>().Update(model);
        }
        public void Delete(Members instance)
        {
            this._unitOfWork.Repository<Members>().Delete(instance);
        }

        public Members Get(Expression<Func<Members, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<MembersViewModel> GetAll(IRolesBLL _rolesBLL)
        {
            var membersData = _unitOfWork.Repository<Members>().GetAll();
            var rolesData = _rolesBLL.GetAll();

            //linq lambda left join 語法
            //GroupJoin + SelectMany
            var qJoin = membersData.GroupJoin(
                rolesData,
                m => m.RoleId,
                r => r.RoleId,
                (x, y) => new { member = x, role = y })
                .SelectMany(
                 x => x.role.DefaultIfEmpty(),
                 (x, y) => new { member = x.member, role = y });

            var qResult = qJoin.Select(x => new MembersViewModel
            {
                UserId = x.member.UserId,
                UserName = x.member.UserName,
                UserEmail = x.member.UserEmail,
                IsEmailValid = x.member.IsEmailValid,
                IsThirdLogin = x.member.IsThirdLogin,
                RoleID = x.member.RoleId,
                RolesDes = WebUtility.UrlEncode($"{x.role.Platform}_{x.role.RoleId}_{x.role.RoleName}")
            });

            return qResult;
        }

        public Members GetById(string userID)
        {
            var dbMember = this._unitOfWork.Repository<Members>().GetById(new object[] { userID });
            return dbMember;
        }

        /// <summary>
        /// 驗證會員帳號密碼是否存在
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public bool IsOkForUsers(ref Members member, ref string errmsg)
        {
            bool result = true;
            try
            {
                if (member == null) { result = false; }
                else
                {
                    var dbMember = this.GetById(member.UserId);
                    if (dbMember == null)
                    {
                        throw new Exception("查無此User帳號");
                    }

                    var roleData = _rolesBLL.GetByIdAsync(dbMember.RoleId).GetAwaiter().GetResult();
                    if (roleData == null)
                    {
                        throw new Exception("沒有權限");
                    }
                    else
                    {
                        if (roleData.Platform != "後台")
                        {
                            throw new Exception("沒有權限");
                        }
                    }

                    var loginKey = this._Configuration["Blog:LoginKey"];
                    var decryptPwd = StringEncrypt.aesDecryptBase64(dbMember.UserPwd, loginKey);

                    if (member.UserPwd == decryptPwd)
                    {
                        member.UserName = dbMember.UserName;
                        member.UserEmail = dbMember.UserEmail;
                        member.RoleId = dbMember.RoleId;
                        result = true;
                    }
                    else
                    {
                        throw new Exception("密碼錯誤");
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                errmsg = ex.Message;
                _logger.LogError($"Error occurs at {this.GetType().Name}.IsOkForUsers() .", ex);
            }
            return result;
        }

        public List<Claim> GetClaims(Members member)
        {
            List<Claim> userClaims = new List<Claim>();
            try
            {
                String UserCookieTime = this._Configuration["Blog:UserCookieTime"];
                DateTime authTime = DateTime.UtcNow;
                DateTime expiresAt = authTime.AddMinutes(Convert.ToDouble(UserCookieTime));

                userClaims.Add(new Claim(ClaimTypes.Name, member.UserName));
                userClaims.Add(new Claim("UserID", member.UserId));
                userClaims.Add(new Claim(ClaimTypes.Email, member.UserEmail));
                userClaims.Add(new Claim(ClaimTypes.Expiration, expiresAt.ToString()));
                userClaims.Add(new Claim(ClaimTypes.Role, member.RoleId));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.GetClaims() .", ex);
            }
            return userClaims;
        }

        public async Task<Members> GetByIdAsync(string userID)
        {
            return await this._unitOfWork.Repository<Members>().GetByIdAsync(new object[] { userID });
        }
        public async Task<MembersViewModel> GetViewByIdAsync(Members member, IRolesBLL _rolesBLL, string state)
        {
            MembersViewModel model = new MembersViewModel();
            RolesViewModel roleViewModel = new RolesViewModel();

            try
            {
                Roles role = await _rolesBLL.GetByIdAsync(member.RoleId);
                var roleDes = $"{role.Platform}_{role.RoleId}_{role.RoleName}";

                List<RoleSelect> roleList = this.GetRoleSelect(_rolesBLL);
                roleViewModel.role = role;
                roleViewModel.RolesList = roleList;

                if (!string.IsNullOrEmpty(member.UserPwd))
                {
                    var loginKey = this._Configuration["Blog:LoginKey"];
                    var decryptPwd = StringEncrypt.aesDecryptBase64(member.UserPwd, loginKey);
                    model.UserPwd = decryptPwd;
                }

                model.State = state;
                model.UserId = member.UserId;
                model.UserName = member.UserName;
                model.UserEmail = member.UserEmail;
                model.IsEmailValid = member.IsEmailValid;
                model.IsThirdLogin = member.IsThirdLogin;
                model.RoleID = member.RoleId;
                model.RolesDes = roleDes;
                model.roleViewModel = roleViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.GetViewByIdAsync() .", ex);
            }

            return model;
        }


        public List<RoleSelect> GetRoleSelect(IRolesBLL _rolesBLL)
        {
            List<RoleSelect> roleList = new List<RoleSelect>();

            var roles = _rolesBLL.GetAll();
            if (roles != null)
            {
                roleList = roles.Select(r => new RoleSelect
                {
                    RoleID = r.RoleId,
                    Platform = r.Platform,
                    RoleDes = $"{r.Platform}_{r.RoleId}_{r.RoleName}"
                }).ToList();
            }
            return roleList;
        }
        public bool UserExists(string userID)
        {
            return this._unitOfWork.Repository<Members>().GetAll().Any(x => x.UserId == userID);
        }
        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public bool EmailExists(string userID, string userEmail)
        {
            return this._unitOfWork.Repository<Members>().GetAll()
                        .Where(x => x.UserId != userID)
                        .Any(x => x.UserEmail == userEmail);
        }
    }
}


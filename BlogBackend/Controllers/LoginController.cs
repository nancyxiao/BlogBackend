using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using DBClassLibrary;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace BlogBackend.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private IMembersBLL _membersBLL;
        private ILoginBLL _loginBLL;

        public LoginController(ILogger<LoginController> logger,
            IMembersBLL membersBLL,
            ILoginBLL loginBLL)
        {
            this._logger = logger;
            this._membersBLL = membersBLL;
            this._loginBLL = loginBLL;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult UserLogin([Bind] LoginViewModel user)
        {
            if (!ModelState.IsValid) return View(user);

            Members member = new Members()
            {
                UserId = user.UserId,
                UserPwd = user.UserPwd
            };

            string errmsg = "";
            if (_membersBLL.IsOkForUsers(ref member, ref errmsg))
            {
                var userClaims = _membersBLL.GetClaims(member);
                var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                HttpContext.SignInAsync(claimsPrincipal,
                    new AuthenticationProperties()
                    {
                        IsPersistent = false //瀏覽器關閉即刻登出
                    });

                _loginBLL.CreateByParam(user.UserId, LoginState.LoginSuccess, DateTime.UtcNow);

                if(!string.IsNullOrEmpty(user.ReturnUrl) && Url.IsLocalUrl(user.ReturnUrl))
                {
                    return Redirect(user.ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
               
            }
            ModelState.AddModelError("LoginFaild", errmsg);
            _loginBLL.CreateByParam(user.UserId, LoginState.LoginFaild, DateTime.UtcNow);

            return View(user);
        }
        [AllowAnonymous]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();

            return RedirectToAction("UserLogin", "Login"); //導至登入頁
        }
    }
}

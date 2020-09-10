using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using DBClassLibrary;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace BlogBackend.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private IMembersBLL _membersBLL;
        private ILoginBLL _loginBLL;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public LoginController(ILogger<LoginController> logger,
            IMembersBLL membersBLL,
            ILoginBLL loginBLL,
            IConfiguration configuration,
            IHttpClientFactory clientFactory)
        {
            this._logger = logger;
            this._membersBLL = membersBLL;
            this._loginBLL = loginBLL;
            this._configuration = configuration;
            this._clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            ViewBag.WebKey = this._configuration["Captcha:WebKey"];
            return View();
        }
        [HttpPost]
        public IActionResult UserLogin([Bind] LoginViewModel user)
        {
            ViewBag.WebKey = this._configuration["Captcha:WebKey"];

            if (!ModelState.IsValid) return View(user);

            string caMsg = string.Empty;
            if (!ReCaptchaPassed(user.Token, ref caMsg))
            {
                ModelState.AddModelError("LoginFaild", $"You failed the CAPTCHA.\n\r {caMsg}");
                _loginBLL.CreateByParam(user.UserId, LoginState.LoginFaild, DateTime.UtcNow);

                return View(user);
            }

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

                if (!string.IsNullOrEmpty(user.ReturnUrl) && Url.IsLocalUrl(user.ReturnUrl))
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
        /// <summary>
        /// 呼叫Google reCaptcha v3的API
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool ReCaptchaPassed(string token, ref string msg)
        {
            bool result = false;
            try
            {
                string secretKey = this._configuration["Captcha:SecretKey"];
                string apiUrl = this._configuration["Captcha:ApiUrl"];
                string responseStr = string.Empty;

                //var captchaData = new { secret = secretKey, response = token, remoteip = "" };
                //string postBody = JsonConvert.SerializeObject(captchaData);
                //var captchaJson = new StringContent(postBody, Encoding.UTF8, "application/json");
                var captchaParam = new FormUrlEncodedContent(new[]
                                            {
                                            new KeyValuePair<string, string>("secret", secretKey),
                                            new KeyValuePair<string, string> ("response", token)
                                            });


                var httpClient = _clientFactory.CreateClient("captcha");
                using (var httpResponse = httpClient.PostAsync(apiUrl, captchaParam))
                {
                    if (httpResponse.Result.StatusCode == HttpStatusCode.OK)
                    {
                        var content = httpResponse.Result.Content;
                        using (StreamReader reader = new StreamReader(content.ReadAsStreamAsync().Result, Encoding.UTF8))
                        {
                            responseStr = reader.ReadToEnd();
                            var responseData = JsonConvert.DeserializeObject<Captcha>(responseStr);
                            result = responseData.success;
                            msg = responseStr;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = (ex.InnerException ?? ex).ToString();
                throw ex;
            }


            return result;
        }
    }

    class Captcha
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }

    }
}



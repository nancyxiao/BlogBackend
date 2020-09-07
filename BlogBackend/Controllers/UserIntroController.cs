using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class UserIntroController : Controller
    {
        private readonly ILogger<UserIntroController> _logger;
        private readonly IUserIntroBLL _userIntroBLL;
        private readonly IMembersBLL _memberBLL;


        private readonly int pageSize = 5;

        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"}
        };

        private readonly string _localfolder;
        private readonly string _subblobfolder;


        public UserIntroController(ILogger<UserIntroController> logger,
            IUserIntroBLL userIntroBLL,
            IMembersBLL memberBLL,
            IWebHostEnvironment env)
        {
            this._logger = logger;
            this._userIntroBLL = userIntroBLL;
            this._memberBLL = memberBLL;
            _localfolder = $@"{env.WebRootPath}\UploadFolder\UserIntro";

            _subblobfolder = "Mugshot";
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var qResult = _userIntroBLL.GetAll();

            var dataPage = qResult.GetPaged(page, pageSize);

            return View(dataPage);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return View(new UserIntroViewModel());
            else
            {
                var userIntro = await _userIntroBLL.GetByIdAsync(id.ToString());
                if (userIntro == null)
                {
                    return NotFound();
                }
                return View(new UserIntroViewModel
                {
                    UserId = userIntro.UserId,
                    Introduction = Helper.JSStringEscape(userIntro.Introduction, false),
                    Photo = userIntro.Photo,
                    LocalPath = userIntro.LocalPath
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, UserIntroViewModel model)
        {
            bool isValid = false;
            try
            {
                string uniqueFileName = string.Empty;
                string extension = string.Empty;

                if (model.ImageFile != null)
                {
                    uniqueFileName = $"{Guid.NewGuid().ToString()}_{model.ImageFile.FileName}";
                    extension = Path.GetExtension(model.ImageFile.FileName);
                    model.BlobName = $"{_subblobfolder}/{model.UserId}/{uniqueFileName}";
                    model.LocalPath = $@"{_localfolder}\{uniqueFileName}";
                }

                if (!string.IsNullOrEmpty(extension) && !_contentTypes.Any(x => x.Key == extension))
                {
                    ModelState.AddModelError("ImageFile", "檔案格式僅允許圖片檔，例如：png、jpg、jpeg、gif");
                }

                if (!_memberBLL.UserExists(model.UserId))
                {
                    ModelState.AddModelError("UserId", "此會員帳號不存在");
                }

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        if (_userIntroBLL.UserIntroExists(model.UserId))
                        {
                            ModelState.AddModelError("UserId", "此會員的個人簡介已經存在");
                        }

                        if (ModelState.IsValid)
                        {
                            _userIntroBLL.Create(model);

                            await _userIntroBLL.SaveAsync();
                        }
                    }
                    else
                    {
                        try
                        {
                            _userIntroBLL.Update(model);

                            await _userIntroBLL.SaveAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!_userIntroBLL.UserIntroExists(id)) { return NotFound(); }
                            else { throw; }
                        }
                    }
                }

                isValid = ModelState.IsValid;
            }
            catch (Exception ex)
            {
                isValid = false;
                this._logger.LogError($"Error occurs at {this.GetType().Name}.AddOrEdit() POST .", ex);
            }

            if (isValid)
            {
                return Json(new
                {
                    isValid = isValid,
                    html = Helper.RenderRazorViewToString(
                                                                                           this,
                                                                                           "_ViewAll",
                                                                                           _userIntroBLL.GetAll().GetPaged(1, pageSize, "/UserIntro"))
                });
            }


            return Json(new { isValid = isValid, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteConfirm(string id, string text)
        {
            ViewBag.Id = id;
            ViewBag.Text = text;
            ViewBag.Controller = this.RouteData.Values["controller"].ToString();
            return await Task.Run(() => View("../Shared/DeleteConfirm"));
        }
        //[ValidateAntiForgeryToken]=>若使用json參數則不能用這個屬性
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] string[] ids)
        {
            if (ids.Length > 0)
            {
                try
                {
                    foreach (string id in ids)
                    {
                        var userIntro = await _userIntroBLL.GetByIdAsync(id);
                        var model = new UserIntroViewModel
                        {
                            UserId = userIntro.UserId,
                            Introduction = userIntro.Introduction,
                            LocalPath = userIntro.LocalPath,
                            Photo = userIntro.Photo,
                            BlobName = userIntro.Photo.Substring(userIntro.Photo.IndexOf(_subblobfolder))
                        };
                        _userIntroBLL.Delete(model);

                     
                    }

                    await _userIntroBLL.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return Json(new
            {
                html = Helper.RenderRazorViewToString(
                                                                        this,
                                                                        "_ViewAll",
                                                                        _userIntroBLL.GetAll().GetPaged(1, pageSize, "/UserIntro"))
            });
        }
    }
}

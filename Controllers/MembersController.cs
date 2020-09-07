using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ILogger<MembersController> _logger;
        private readonly IMembersBLL _membersBLL;
        private readonly IRolesBLL _rolesBLL;

        private readonly int pageSize = 5;

        public MembersController(ILogger<MembersController> logger
            , IMembersBLL membersBLL
            , IRolesBLL rolesBLL)
        {
            this._logger = logger;
            this._membersBLL = membersBLL;
            this._rolesBLL = rolesBLL;

        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var qResult = _membersBLL.GetAll(this._rolesBLL);
           
            var dataPage = qResult.GetPaged(page, pageSize);

            return View(dataPage);
        }
        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<MembersViewModel> model)
        {
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string state)
        {
            if (string.IsNullOrEmpty(id))
                return View(new MembersViewModel
                {
                    State = state,
                     roleViewModel = new RolesViewModel
                     {
                          RolesList = _membersBLL.GetRoleSelect(this._rolesBLL)
                     }
                });
            else
            {
                var member = await _membersBLL.GetByIdAsync(id.ToString());
                if (member == null)
                {
                    return NotFound();
                }

                var memberViewModel = await _membersBLL.GetViewByIdAsync(member, this._rolesBLL,  state);

                return View(memberViewModel);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, MembersViewModel model)
        {
            bool isValid = false;
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        if (_membersBLL.UserExists(model.UserId))
                        {
                            ModelState.AddModelError("UserId", "此會員帳號已經存在");
                        }

                        if (_membersBLL.EmailExists(model.UserId, model.UserEmail))
                        {
                            ModelState.AddModelError("UserEmail", "此 Email 已經註冊過");
                        }

                        if (ModelState.IsValid)
                        {
                            _membersBLL.Create(model);

                            await _membersBLL.SaveAsync();
                        }
                     

                    }
                    else
                    {
                        if (_membersBLL.EmailExists(model.UserId, model.UserEmail))
                        {
                            ModelState.AddModelError("UserEmail", "此 Email 已經註冊過");
                        }

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                _membersBLL.Update(model);

                                await _membersBLL.SaveAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!_membersBLL.UserExists(id)) { return NotFound(); }
                                else { throw; }
                            }

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
                                                                                           _membersBLL.GetAll(this._rolesBLL).GetPaged(1, pageSize, "/Members"))
                });
            }

            model.roleViewModel = new RolesViewModel
            {
                RolesList = _membersBLL.GetRoleSelect(this._rolesBLL)
            };
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
                        var member = await _membersBLL.GetByIdAsync(id);
                        _membersBLL.Delete(member);
                    }

                    await _membersBLL.SaveAsync();
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
                                                                        _membersBLL.GetAll(this._rolesBLL).GetPaged(1, pageSize, "/Members"))
            });
        }
    }
}

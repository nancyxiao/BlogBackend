using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogBackend.Lib;
using BusinessLibrary;
using DBClassLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class ClassAllController : Controller
    {
        private readonly ILogger<ClassAllController> _logger;
        private readonly IClassAllBLL _classAllBLL;
        private readonly IMembersBLL _memberBLL;

        private readonly int pageSize = 5;

        public ClassAllController(ILogger<ClassAllController> logger, IClassAllBLL classAllBLL, IMembersBLL memberBLL )
        {
            this._logger = logger;
            this._classAllBLL = classAllBLL;
            this._memberBLL = memberBLL;
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var dataPage = _classAllBLL.GetAll().GetPaged(page, pageSize);
            return View(dataPage);
        }

        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<ClassAll> model)
        {
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string classId, string state)
        {
            ViewBag.State = state;

            if (string.IsNullOrEmpty(id))
                return View(new ClassAll());
            else
            {
                var classAll = await _classAllBLL.GetByIdAsync(id, classId);
                if (classAll == null)
                {
                    return NotFound();
                }
                return View(classAll);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, ClassAll model)
        {
            if (!_memberBLL.UserExists(model.UserId))
            {
                ModelState.AddModelError("UserId", "此會員帳號不存在");
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(id))
                {
                    model.ClassId = await _classAllBLL.GetMaxClassID(model.UserId);
                    _classAllBLL.Create(model);
                    await _classAllBLL.SaveAsync();
                }
                else
                {
                    try
                    {
                        _classAllBLL.Update(model);
                    

                        await _classAllBLL.SaveAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_classAllBLL.ClassIdExists(id, model.ClassId)) { return NotFound(); }
                        else { throw; }
                    }
                }
                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(
                                                                                        this,
                                                                                        "_ViewAll",
                                                                                        _classAllBLL.GetAll().GetPaged(1, pageSize, "/ClassAll"))
                });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
        }
        [NoDirectAccess]
        public async Task<IActionResult> DeleteConfirm(string id,string classId, string text)
        {
            ViewBag.Id = string.IsNullOrEmpty(id) ? "" : $"{id},{classId}";
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
                    foreach (string value in ids)
                    {
                        var userId = value.Split(',')[0];
                        var classId = value.Split(',')[1];
                        var model = await _classAllBLL.GetByIdAsync(userId,classId);
                        _classAllBLL.Delete(model);
                    }

                    await _classAllBLL.SaveAsync();
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
                                                                        _classAllBLL.GetAll().GetPaged(1, pageSize, "/ClassAll"))
            });
        }
    }
}

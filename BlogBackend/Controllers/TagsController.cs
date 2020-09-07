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

    public class TagsController : Controller
    {
        private readonly ILogger<TagsController> _logger;
        private readonly ITagsBLL _tagsBLL;
        private readonly IMembersBLL _memberBLL;

        private readonly int pageSize = 5;

        public TagsController(ILogger<TagsController> logger, ITagsBLL tagsBLL, IMembersBLL memberBLL)
        {
            this._logger = logger;
            this._tagsBLL = tagsBLL;
            this._memberBLL = memberBLL;
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var dataPage = _tagsBLL.GetAll().GetPaged(page, pageSize);
            return View(dataPage);
        }

        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<Tags> model)
        {
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string tagId, string state)
        {
            ViewBag.State = state;

            if (string.IsNullOrEmpty(id))
                return View(new Tags());
            else
            {
                var classAll = await _tagsBLL.GetByIdAsync(id, tagId);
                if (classAll == null)
                {
                    return NotFound();
                }
                return View(classAll);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id,Tags model)
        {
            string state = string.IsNullOrEmpty(id) ? "add" : "edit";

            if (!_memberBLL.UserExists(model.UserId))
            {
                ModelState.AddModelError("UserId", "此會員帳號不存在");
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(id))
                {
                    model.TagId = await _tagsBLL.GetMaxTagID(model.UserId);
                    _tagsBLL.Create(model);
                    await _tagsBLL.SaveAsync();
                }
                else
                {
                    try
                    {
                        _tagsBLL.Update(model);


                        await _tagsBLL.SaveAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_tagsBLL.TagIdExists(id, model.TagId)) { return NotFound(); }
                        else { throw; }
                    }
                }
                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(
                                                                                        this,
                                                                                        "_ViewAll",
                                                                                        _tagsBLL.GetAll().GetPaged(1, pageSize, "/Tags"))
                });
            }

            ViewBag.State = state;
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
        }
        [NoDirectAccess]
        public async Task<IActionResult> DeleteConfirm(string id, string tagId, string text)
        {
            ViewBag.Id = string.IsNullOrEmpty(id) ? "" : $"{id},{tagId}";
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
                        var tagId = value.Split(',')[1];
                        var model = await _tagsBLL.GetByIdAsync(userId, tagId);
                        _tagsBLL.Delete(model);
                    }

                    await _tagsBLL.SaveAsync();
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
                                                                        _tagsBLL.GetAll().GetPaged(1, pageSize, "/Tags"))
            });
        }
    }
}

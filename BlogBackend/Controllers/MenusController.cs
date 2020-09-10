using BlogBackend.Lib;
using BusinessLibrary;
using DBClassLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class MenusController : Controller
    {
        private readonly ILogger<MenusController> _logger;
        private readonly IMenusBLL _menusBLL;
        private readonly int pageSize = 5;

        public MenusController(ILogger<MenusController> logger, IMenusBLL menusBLL)
        {
            _logger = logger;
            _menusBLL = menusBLL;
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var dataPage = _menusBLL.GetAll().GetPaged(page, pageSize);
            return View(dataPage);
        }
        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<DBClassLibrary.Menus> model)
        {
            return View(model);
        }
        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string state)
        {
            ViewBag.State = state;
            if (string.IsNullOrEmpty(id))
                return View(new DBClassLibrary.Menus());
            else
            {
                var menu = await _menusBLL.GetByIdAsync(id.ToString());
                if (menu == null)
                {
                    return NotFound();
                }
                return View(menu);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, Menus menuModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(id))
                {
                    menuModel.MenuId = await _menusBLL.GetMaxMenuID();
                    _menusBLL.Create(menuModel);
                    await _menusBLL.SaveAsync();
                }
                else
                {
                    try
                    {
                        _menusBLL.Update(menuModel);
                        await _menusBLL.SaveAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_menusBLL.MenuExists(id)) { return NotFound(); }
                        else { throw; }
                    }
                }
                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(
                                                                                        this,
                                                                                        "_ViewAll",
                                                                                        _menusBLL.GetAll().GetPaged(1, pageSize, "/Menus"))
                });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", menuModel) });
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
        public async Task<IActionResult> Delete([FromBody]string[] ids)
        {
            if (ids.Length > 0)
            {
                try
                {
                    foreach (string id in ids)
                    {
                        var menu = await _menusBLL.GetByIdAsync(id);
                        _menusBLL.Delete(menu);
                    }
                    await _menusBLL.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return Json( new { html = Helper.RenderRazorViewToString(
                                                                        this,
                                                                        "_ViewAll",
                                                                        _menusBLL.GetAll().GetPaged(1, pageSize, "/Menus"))
            });
        }
    }

}

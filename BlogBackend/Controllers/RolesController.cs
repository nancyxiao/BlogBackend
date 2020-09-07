using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DBClassLibrary;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly ILogger<RolesController> _logger;
        private readonly IRolesBLL _rolesBLL;
        private readonly IMenusBLL _menusBLL;

        private readonly int pageSize = 5;

        public RolesController(ILogger<RolesController> logger, IRolesBLL rolesBLL
            , IMenusBLL menusBLL)
        {
            this._logger = logger;
            this._rolesBLL = rolesBLL;
            this._menusBLL = menusBLL;
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var dataPage = _rolesBLL.GetAll().GetPaged(page, pageSize);
            return View(dataPage);
        }
        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<Roles> model)
        {
            return View(model);
        }
        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string state)
        {
            ViewBag.State = state;
            if (string.IsNullOrEmpty(id))
                return View(new RolesViewModel());
            else
            {
                var role = await _rolesBLL.GetByIdAsync(id.ToString());
                if (role == null)
                {
                    return NotFound();
                }
                return View(new RolesViewModel
                {
                    role = role
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, RolesViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(id))
                {
                    roleModel.role.RoleId = await _rolesBLL.GetMaxRoleID();
                    _rolesBLL.Create(roleModel.role);
                    if (roleModel.TreeIds.Length > 0)
                    {
                        var menus = CreateRoleMenusMapping(roleModel.role.RoleId, roleModel.TreeIds);
                        _rolesBLL.CreateMenus(menus);
                    }


                    await _rolesBLL.SaveAsync();
                }
                else
                {
                    try
                    {
                        _rolesBLL.Update(roleModel.role);
                        //若有對應到角色的選單則先刪除
                        var menusQuery = _rolesBLL.GetMenuMapping(roleModel.role.RoleId);
                        if (menusQuery != null)
                        {
                            _rolesBLL.DeleteMenus(menusQuery.ToList());
                        }
                        //新增角色對應的選單
                        if (roleModel.TreeIds.Length > 0)
                        {
                            var menus = CreateRoleMenusMapping(id, roleModel.TreeIds);
                            _rolesBLL.CreateMenus(menus);
                        }


                        await _rolesBLL.SaveAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_rolesBLL.RoleExists(id)) { return NotFound(); }
                        else { throw; }
                    }
                }
                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(
                                                                                        this,
                                                                                        "_ViewAll",
                                                                                        _rolesBLL.GetAll().GetPaged(1, pageSize, "/Roles"))
                });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", roleModel) });
        }

        private List<RoleMenuMapping> CreateRoleMenusMapping(string id, string treeIds)
        {

            List<RoleMenuMapping> menus = new List<RoleMenuMapping>();
            string[] treeArray = treeIds.Split(',');
            foreach (string menuid in treeArray)
            {
                menus.Add(new RoleMenuMapping
                {
                    RoleId = id,
                    MenuId = menuid
                });
            }

            return menus;
        }

        [HttpPost]
        [NoDirectAccess]
        public async Task<IActionResult> GetTreeData([FromBody] Roles roleModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleModel.Platform))
                {
                    var tree = this._menusBLL.GetAll().Where(m => m.Platform == roleModel.Platform)
                        .Select(m => new
                        {
                            id = m.MenuId,
                            pid = m.ParentId ?? "",
                            name = m.MenuName//,
                            //open = true
                        });

                    var mappingMenus = this._rolesBLL.GetMenuMapping(roleModel.RoleId)
                                                            .Select(m => new { id = m.MenuId });

                    var roleData = await this._rolesBLL.GetByIdAsync(roleModel.RoleId);


                    return await Task.Run(() => Json(new
                    {
                        data = tree,
                        menus = mappingMenus,
                        role = roleData != null ? new   {
                                        CanQuery = roleData.CanQuery,
                                        CanUpdate = roleData.CanUpdate
                                    } : null
                    }));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.Run(() => Json(null));
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
                        var role = await _rolesBLL.GetByIdAsync(id);
                        _rolesBLL.Delete(role);

                        var menus = _rolesBLL.GetMenuMapping(role.RoleId);
                        if (menus != null)
                        {
                            _rolesBLL.DeleteMenus(menus.ToList());
                        }
                    }

                    await _rolesBLL.SaveAsync();
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
                                                                        _rolesBLL.GetAll().GetPaged(1, pageSize, "/Roles"))
            });
        }
    }
}

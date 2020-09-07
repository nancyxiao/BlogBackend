using BusinessLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogBackend.Lib
{
    public class MenuAttribute: ActionFilterAttribute
    {
        private readonly IMenusBLL _menusBLL;
        private readonly IRolesBLL _rolesBLL;
        public MenuAttribute(IMenusBLL menusBLL, IRolesBLL rolesBLL)
        {
            this._menusBLL = menusBLL;
            this._rolesBLL = rolesBLL;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Controller is Controller controller)
            {
                //找出角色對應的選單
                var claim = filterContext.HttpContext.User.FindFirst(ClaimTypes.Role);
                var roleId = claim != null ? claim.Value : string.Empty;
                var roleMenuMapping = this._rolesBLL.GetMenuMapping(roleId);
                var menuIds = roleMenuMapping.Select(x => x.MenuId);

                controller.ViewBag.Menus = _menusBLL.GetAll().Where(m => menuIds.Contains(m.MenuId));
            }
           
        }
    }
}

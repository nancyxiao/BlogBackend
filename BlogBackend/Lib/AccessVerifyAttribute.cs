using BusinessLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogBackend.Lib
{
    public class AccessVerifyAttribute : IAuthorizationFilter
    {
        private readonly IMenusBLL _menusBLL;
        private readonly IRolesBLL _rolesBLL;
        public AccessVerifyAttribute(IMenusBLL menusBLL, IRolesBLL rolesBLL)
        {
            this._menusBLL = menusBLL;
            this._rolesBLL = rolesBLL;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //if (context.HttpContext.User.Identity.IsAuthenticated == false)
            //{
            //    RedirectResult result = new RedirectResult("~/Login/Logout");
            //    context.Result = result;
            //    //context.HttpContext.Abort();
            //}

            if (context.Filters.Any(it => it is Microsoft.AspNetCore.Mvc.Authorization.IAllowAnonymousFilter))
            {

            }
            else
            {

                string currentCtrlName = "";
                string currentActionName = "";

                var descript = context.ActionDescriptor as ControllerActionDescriptor;
                if (descript != null)
                {
                    currentCtrlName = descript.ControllerName;
                    currentActionName = descript.ActionName;
                }

                //找出角色對應的選單
                var claim = context.HttpContext.User.FindFirst(ClaimTypes.Role);
                var roleId = claim != null ? claim.Value : string.Empty;
                var roleMenuMapping = this._rolesBLL.GetMenuMapping(roleId);
                var menuData = this._menusBLL.GetAll();
                var menuIQueryable = roleMenuMapping.Join(menuData, r => r.MenuId, m => m.MenuId,
                    (x, y) => y);


                bool hasMenu = menuIQueryable.Any(x => x.Controller == currentCtrlName && x.Action == currentActionName);

               

                if (hasMenu == false)
                {
                    RedirectResult result = new RedirectResult("~/Login/Logout");
                    context.Result = result;
                }
            }
        }
    }
}

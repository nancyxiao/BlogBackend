using BlogViewModels;
using BusinessLibrary;
//using DBClassLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogBackend.Lib
{
    public class AccessVerifyHandler : AuthorizationHandler<AccessVerifyRequirement>
    {
        private readonly IMenusBLL _menusBLL;
        private readonly IRolesBLL _rolesBLL;

        public AccessVerifyHandler(IMenusBLL menusBLL, IRolesBLL rolesBLL)
        {
            this._menusBLL = menusBLL;
            this._rolesBLL = rolesBLL;
        }
        protected IQueryable<MenuDataViewModel> GetMenusByRoleId(string roleId)
        {
            var menuData = _menusBLL.GetAll();
            var roleMenuMapping = this._rolesBLL.GetMenuMapping(roleId);

            var resultData = menuData.Join(roleMenuMapping, m => m.MenuId, r => r.MenuId,
                (x, y) => new MenuDataViewModel
                {
                    ControllerName = x.Controller,
                    ActionName = x.Action
                });

            return resultData;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessVerifyRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated == false)
            {
                context.Fail();
                return Task.FromResult(0);
            }


            string currentCtrlName = "";
            string currentActionName = "";


            var mvcContext = context.Resource as AuthorizationFilterContext;
            var descriptor = mvcContext?.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null)
            {
                currentActionName = descriptor.ActionName;
                currentCtrlName = descriptor.ControllerName;

                var claim = context.User.FindFirst(ClaimTypes.Role);
                var roleId = claim != null ? claim.Value : string.Empty;
                var menusIQueryable = this.GetMenusByRoleId(roleId);

                bool hasMenu = menusIQueryable.Any(x => x.ControllerName == currentCtrlName && x.ActionName == currentActionName);
                if (hasMenu == false)
                {
                    context.Fail();
                }
            }

            context.Succeed(requirement);

            return Task.FromResult(0);
        }
    }
}

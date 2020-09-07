using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogBackend.Lib
{
    public  class Helper
    {
        public static string RenderRazorViewToString(Controller controller, string viewName, object model = null)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        sw,
                        new HtmlHelperOptions()
                    );
                //viewResult.View.RenderAsync(viewContext);

                var task = viewResult.View.RenderAsync(viewContext);

                task.GetAwaiter().GetResult();
                //viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.GetStringBuilder().ToString();
            }
        }

        //public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        //{
        //    var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

        //    return (claim != null) ? claim.Value : string.Empty;
        //}

        /// <returns>置換特殊字元之後的字串</returns>
        /// <remarks></remarks>
        public static string JSStringEscape(string raw, bool inHtmlAttribute)
        {
            raw = raw.Replace("\r\n", "\\n").Replace("\r", "").Replace("\n", "\\n");
            if (inHtmlAttribute)
                raw = raw.Replace("\"", "&quot;").Replace("'", "\\'");
            else
                raw = raw.Replace("'", "\\'").Replace("\"", "\\\"");
            return raw;
        }
    }
}

using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Kululu.Web.Controllers
{
    /// <summary>
    /// class stolen from
    /// http://www.codeproject.com/KB/aspnet/aspnet_mvc_restapi.aspx
    /// 
    /// Article by Omar Al Zabir
    /// "Build truly RESTful API and website using same ASP.NET MVC code"
    /// 18 Aug 2011
    /// </summary>
    public class EnableJsonAttribute : ActionFilterAttribute
    {
        private readonly static string[] JsonTypes = new[] { "application/json", "text/json" };

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (typeof(RedirectToRouteResult).IsInstanceOfType(filterContext.Result))
                return;

            var acceptTypes = filterContext.HttpContext.Request.AcceptTypes ?? new[] { "text/html" };

            var model = filterContext.Controller.ViewData.Model;

            var contentEncoding = filterContext.HttpContext.Request.ContentEncoding ?? Encoding.UTF8;

            if (JsonTypes.Any(acceptTypes.Contains))
                filterContext.Result = new JsonResult2
                    {
                        Data = model,
                        ContentEncoding = contentEncoding,
                        ContentType = filterContext.HttpContext.Request.ContentType
                    };
        }
    }
}
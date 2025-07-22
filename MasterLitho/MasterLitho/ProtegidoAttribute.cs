using System;
using System.Web;
using System.Web.Mvc;

public class ProtegidoAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var session = filterContext.HttpContext.Session;
        if (session["ID_USUARIO"] == null)
        {
            filterContext.Result = new RedirectToRouteResult(
                           new System.Web.Routing.RouteValueDictionary(
                               new { controller = "Login", action = "IniciarSesion" }
                           )
                       ); return;
        }

        var response = filterContext.HttpContext.Response;
        response.Cache.SetCacheability(HttpCacheability.NoCache);
        response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
        response.Cache.SetNoStore();

        base.OnActionExecuting(filterContext);
    }
}

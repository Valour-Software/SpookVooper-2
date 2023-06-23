using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SV2.Controllers;
using System.Security.Claims;
using Valour.Api.Models;

namespace SV2.Helpers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class UserRequiredAttribute : ActionFilterAttribute, IActionFilter, IEndpointFilter
{
    public UserRequiredAttribute()
    {
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        SVUser? user = UserManager.GetUser(context.HttpContext);
        context.HttpContext.Items["user"] = user;
        var result = await next(context);
        return result;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // yes i know this is bad
        // but this is meant to be temp
        var path = context.HttpContext.Request.Path.Value;
        if (!(path == "/Account/Login"
            || path == "/dev/lackaccess"
            || path == "/callback")) 
        {
            SVUser? user = UserManager.GetUser(context.HttpContext);
            SVController controller = (SVController)context.Controller;
            if (user is null || user.OAuthToken is null)
                context.Result = controller.Redirect("/Account/Login");
            context.HttpContext.Items["user"] = user;
        }
    }
}
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SV2.Controllers;
using System.Security.Claims;
using Valour.Api.Models;

namespace SV2.Helpers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class IsMinisterAttribute : ActionFilterAttribute
{
    public string Type { get; }
    public IsMinisterAttribute(string type)
    {
        Type = type;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        SVUser? user = UserManager.GetUser(context.HttpContext);
        SVController controller = (SVController)context.Controller;
        if (user is null)
            context.Result = controller.Redirect("/Account/Login");
        if (!user.IsMinister(Type))
            context.Result = controller.RedirectBack($"You must be {Type}");
    }
}
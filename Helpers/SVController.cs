using Microsoft.AspNetCore.Mvc;

namespace SV2.Helpers;

public abstract class SVController : Controller
{
    [TempData]
    public string StatusMessage { get; set; }

    public IActionResult RedirectBack(string reason)
    {
        StatusMessage = reason;
        var url = Request.Headers["Referer"].ToString();
        if (url == "") url = "/";
        return Redirect(url);
    }
}

namespace SV2.API;

public class BaseAPI
{
    public static string VERSION;

    public static async Task TokenInvalid(HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsync($"Token is invalid.");
    }

    public static async Task Unauthorized(string reason, HttpContext ctx)
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsync(reason);
    }

    public static async Task NotFound(string reason, HttpContext ctx)
    {
        ctx.Response.StatusCode = 404;
        await ctx.Response.WriteAsync(reason);
    }

    public static async Task BadRequest(string reason, HttpContext ctx)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync(reason);
    }
}
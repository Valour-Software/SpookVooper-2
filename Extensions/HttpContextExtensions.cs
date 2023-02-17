namespace SV2.Extensions;

public static class HttpContextExtensions
{
    public static SVUser GetUser(this HttpContext context)
    {
        return (SVUser)context.Items["user"]!;
    }
}

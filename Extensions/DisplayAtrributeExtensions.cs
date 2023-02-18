using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SV2.Extensions;

public class Extensions
{
    public static string GetDescription<T>(string propertyName) where T : class
    {
        MemberInfo memberInfo = typeof(T).GetProperty(propertyName);
        if (memberInfo == null)
        {
            return null;
        }

        return memberInfo.GetCustomAttribute<DisplayAttribute>()?.GetDescription();
    }
}

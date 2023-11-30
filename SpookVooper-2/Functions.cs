using System.Text.Json;

namespace SV2;

public static class Functions
{
    public static string ToJson(object? value) => JsonSerializer.Serialize(value);
}

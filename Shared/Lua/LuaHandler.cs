using System.Globalization;
using System.ComponentModel;
using Decimal = Shared.Lua.Scripting.Decimal;
using System.Xml.Linq;
using System.Data;
using System.Text.Json.Serialization;
using Shared.Lua.LuaObjects;
using Shared.Lua.Scripting;

namespace Shared.Lua;

public static class StringExtensions
{
    public static string ToTitleCase(this string title)
    {
        title = title.Replace("_", " ");
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
    }
}

public class LuaTable : LuaObject
{
    public List<LuaObject> Items { get; set; }
    public LuaTable()
    {
        Items = new();
        type = ObjType.LuaTable;
    }

    [JsonIgnore]
    public IEnumerable<string> Keys
    {
        get
        {
            return Items.Select(x => x.Name);
        }
    }

    [JsonIgnore]
    public IEnumerable<LuaObject> Values
    {
        get
        {
            return Items;
        }
    }
    public LuaObject this[string key]
    {
        get
        {
            return Items.FirstOrDefault(x => x.Name == key);
        }
    }

    public string GetValue(string key)
    {
        var obj = this[key];
        if (obj is null) return null;
        return obj.Value;
    }

    public bool ContainsKey(string key)
    {
        return Items.FirstOrDefault(x => x.Name == key) != null;
    }
}

public static class LuaHandler
{
    public static void HandleError(string filename, int linenumber, string error, string message)
    {
        var text = $"{filename.Split("/").Last()}:{linenumber} {error}: {message}";
        Console.WriteLine(text);
    }
}
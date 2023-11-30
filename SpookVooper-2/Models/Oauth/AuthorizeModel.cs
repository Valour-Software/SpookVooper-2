using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV2.Models.Oauth;

public class AuthorizeModel
{
    public string ReponseType { get; set; }
    public long ClientID { get; set; }
    public long UserID { get; set; }
    public long EntityId { get; set; }
    public string Redirect { get; set; }
    public string Scope { get; set; }
    public string State { get; set; }
    public string Code { get; set; }

    public List<SelectListItem> CanSelect { get; set; }

    public string GetDesc(string scope)
    {
        if (scope == "view")
        {
            return "View your account";
        }
        else if (scope == "eco")
        {
            return "Control credit payments";
        }
        else if (scope == "resources")
        {
            return "Control resource trades";
        }
        else
        {
            return $"Use {scope} management";
        }
    }
    public List<string> GetScopeDesc()
    {
        List<string> scopes = new List<string>();

        foreach (string s in Scope.Split('|'))
        {
            if (s != "")
            {
                scopes.Add($"- {GetDesc(s)}");
            }
        }

        return scopes;
    }
}

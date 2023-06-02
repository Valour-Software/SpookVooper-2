using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SV2.Database.Models.Entities;
using SV2.Extensions;
using SV2.Helpers;
using SV2.Models.Oauth;
using System.Security.Cryptography;
using System.Text;
using static System.Formats.Asn1.AsnWriter;

namespace SV2.Controllers;

public class OauthController : SVController
{
    private readonly VooperDB _context;

    public static List<AuthorizeModel> authModels = new List<AuthorizeModel>();

    public OauthController(
        VooperDB context)
    {
        _context = context;
    }

    [UserRequired]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(HttpContext.GetUser());
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> Authorize(string response_type, long client_id, string redirect_uri, string scope, string state)
    {
        SVUser user = HttpContext.GetUser();

        if (response_type == null || string.IsNullOrWhiteSpace(response_type))
        {
            return NotFound("Please define the response type.");
        }


        if (response_type.ToLower() == "code")
        {
            OauthApp app = await _context.OauthApps.FindAsync(client_id);

            if (app == null)
            {
                return NotFound("Could not find that client ID!");
            }

            string fscope = "";
            var scopes = new List<string>();

            foreach (string s in scope.Split(','))
            {
                fscope += $"|{s}";
                scopes.Add(s);
            }

            var canselect = new List<SelectListItem>();
            canselect.Add(new(user.Name, user.Id.ToString(), true));
            foreach (var group in user.GetGroupsIn(user))
            {
                if (canselect.Any(x => x.Value == group.Id.ToString()))
                    continue;
                bool hasperm = true;
                foreach (var _scope in scopes)
                {
                    if (_scope == "eco" && !group.HasPermission(user, GroupPermissions.Eco))
                        hasperm = false;
                    if (_scope == "resources" && !group.HasPermission(user, GroupPermissions.Resources))
                        hasperm = false;
                }
                if (hasperm)
                    canselect.Add(new(group.Name, group.Id.ToString(), false));
            }

            AuthorizeModel model = new AuthorizeModel()
            {
                ClientID = client_id,
                Redirect = redirect_uri,
                UserID = user.Id,
                ReponseType = response_type,
                Scope = fscope,
                State = state,
                CanSelect = canselect
            };

            return View(model);
        }
        else
        {
            return NotFound($"Response type {response_type} is not yet supported!");
        }

        return NotFound("Ahhhh");
    }

    [HttpPost]
    [UserRequired]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Authorize(AuthorizeModel model)
    {
        ModelState.Remove("Code");
        ModelState.Remove("CanSelect");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = HttpContext.GetUser();

        var entity = BaseEntity.Find(model.EntityId);

        if (!entity.HasPermission(user, GroupPermissions.Edit))
        {
            StatusMessage = "You lack permission!";
            return View();
        }

        // Create key
        model.Code = Guid.NewGuid().ToString();

        authModels.Add(model);

        return Redirect(model.Redirect + $"?code={model.Code}&state={model.State}&entityid={model.EntityId}");
    }

    [HttpGet]
    [EnableCors("ApiPolicy")]
    public async Task<IActionResult> RequestToken(string grant_type, string code, string redirect_uri,
                                                    long client_id, string client_secret)
    {
        if (grant_type.ToLower() == "authorization_code")
        {
            AuthorizeModel auth = authModels.FirstOrDefault(x => x.Code == code);

            if (auth == null)
                return NotFound("Could not find specified code.");

            if (auth.ClientID != client_id)
                return NotFound("Client ID does not match.");

            if (auth.Redirect != redirect_uri)
                return NotFound("Redirect does not match.");

            OauthApp app = await _context.OauthApps.FirstOrDefaultAsync(x => x.Id == client_id);

            if (app.Secret != client_secret)
                return Unauthorized("Failed authorization. This failure has been logged.");

            var user = DBCache.Get<SVUser>(auth.UserID);
            var entity = DBCache.FindEntity(auth.EntityId);
            if (!(entity.EntityType == EntityType.User))
            {
                foreach (var scope in auth.Scope.Split('|'))
                {
                    if (scope == "eco" && !(entity.HasPermission(user, GroupPermissions.Eco)))
                        return Unauthorized("You lack permission!");
                    if (scope == "resources" && !(entity.HasPermission(user, GroupPermissions.Resources)))
                        return Unauthorized("You lack permission!");
                }
            }

            app.Uses += 1;

            _context.OauthApps.Update(app);
            await _context.SaveChangesAsync();

            string hash = ToHex(SHA256.Create().ComputeHash(Guid.NewGuid().ToByteArray()), false);

            AuthToken token = new AuthToken()
            {
                Id = hash,
                AppId = client_id,
                UserId = auth.UserID,
                Scope = auth.Scope,
                Time = DateTime.UtcNow,
                EntityId = auth.EntityId,
                EntityType = entity.EntityType
            };

            await _context.AuthTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            TokenResponse response = new TokenResponse()
            {
                access_token = token.Id,
                expires_in = 3600,
                entityid = token.EntityId,
                entityType = entity.EntityType,
                userid = token.UserId,
            };

            authModels.Remove(auth);

            return Json(response);
        }
        else
        {
            return NotFound("Grant type not implemented!");
        }
    }

    private static string ToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public long entityid { get; set; }
        public long userid { get; set; }
        public EntityType entityType { get; set; }
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> RegisterApp()
    {
        RegisterAppModel model = new RegisterAppModel();
        return View(model);
    }

    [HttpPost]
    [UserRequired]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterApp(RegisterAppModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            StatusMessage = "Please enter a valid name!";
            return View();
        }

        var user = HttpContext.GetUser();

        OauthApp app = new OauthApp()
        {
            Id = IdManagers.GeneralIdGenerator.Generate(),
            Secret = Guid.NewGuid().ToString(),
            Name = model.Name,
            Image_Url = model.Image_Url,
            OwnerId = user.Id,
            Uses = 0
        };

        await _context.OauthApps.AddAsync(app);
        await _context.SaveChangesAsync();

        StatusMessage = "Successfully created Application!";

        return RedirectToAction("ViewApp", new { appid = app.Id });
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> ViewApp(long appid)
    {
        OauthApp app = await _context.OauthApps.FindAsync(appid);

        if (app == null)
            return NotFound("Could not find that app!");

        var user = HttpContext.GetUser();

        if (app.OwnerId != user.Id)
            return Unauthorized("You do not own this app!");

        return View(app);
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> ViewSecret(long appid)
    {
        OauthApp app = await _context.OauthApps.FindAsync(appid);

        if (app == null)
        {
            return NotFound("Could not find that app!");
        }

        var user = HttpContext.GetUser();

        if (app.OwnerId != user.Id)
        {
            return Unauthorized("You do not own this app!");
        }

        return View((object)app.Secret);
    }

    [HttpGet]
    [UserRequired]
    public async Task<IActionResult> ResetSecret(string secret)
    {
        OauthApp app = await _context.OauthApps.FirstOrDefaultAsync(x => x.Secret == secret);

        if (app == null)
        {
            return NotFound("There was an error resetting the secret!");
        }

        var user = HttpContext.GetUser();

        if (app.OwnerId != user.Id)
        {
            return Unauthorized("There was an error resetting the secret!");
        }

        app.Secret = Guid.NewGuid().ToString();

        _context.OauthApps.Update(app);
        await _context.SaveChangesAsync();

        return RedirectToAction("ViewApp", new { appid = app.Id });
    }

}

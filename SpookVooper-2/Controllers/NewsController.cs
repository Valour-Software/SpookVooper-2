using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Database.Models.Districts;
using SV2.Helpers;
using SV2.Database.Models.News;

namespace SV2.Controllers;

public class NewsController : SVController
{
    private readonly ILogger<NewsController> _logger;
    private readonly VooperDB _dbctx;

    public NewsController(ILogger<NewsController> logger,
        VooperDB dbctx)
    {
        _logger = logger;
        _dbctx = dbctx;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [HttpGet("/News/ViewPost/{id}")]
    public async Task<IActionResult> ViewPost(long id)
    {
        var post = await _dbctx.NewsPosts.FirstOrDefaultAsync(x => x.Id == id)!;
        post.ViewCount += 1;
        await _dbctx.SaveChangesAsync();
        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Create(long groupid)
    {
        SVUser? user = UserManager.GetUser(HttpContext);

        if (user is null)
        {
            return Redirect("/account/login");
        }

        Group? group = DBCache.Get<Group>(groupid);

        if (group == null) return RedirectBack($"Error: Could not find group {groupid}");

        if (!group.Flags.Contains(GroupFlag.News)) return RedirectBack($"Error: Group does not have a press pass!");

        if (!group.HasPermission(user, GroupPermissions.News)) return RedirectBack($"Error: You do not have permission!");

        NewsPost post = new NewsPost()
        {
            Id = IdManagers.GeneralIdGenerator.Generate(),
            GroupId = groupid,
            AuthorId = user.Id
        };

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsPost model)
    {
        SVUser? user = UserManager.GetUser(HttpContext);
        if (user is null)
            return Redirect("/account/login");

        if (!ModelState.IsValid) return View(model);

        Group? group = DBCache.Get<Group>(model.GroupId);

        if (group == null) return RedirectBack($"Error: Could not find group {model.GroupId}");

        if (!group.Flags.Contains(GroupFlag.News)) return RedirectBack($"Error: Group does not have a press pass!");

        if (!group.HasPermission(user, GroupPermissions.News)) return RedirectBack($"Error: You do not have permission!");

        model.Timestamp = DateTime.UtcNow;

        await _dbctx.NewsPosts.AddAsync(model);
        await _dbctx.SaveChangesAsync();

        return Redirect($"/News/ViewPost/{model.Id}");
    }

    [IsMinister("Minister of Journalism")]
    public async Task<IActionResult> AddPressPass(long groupid)
    {
        Group? group = DBCache.Get<Group>(groupid);
        if (group == null) return RedirectBack($"Failed to find group {groupid}");

        group.Flags.Add(GroupFlag.News);

        StatusMessage = $"Gave press pass to {group.Name}";
        return Redirect($"/Group/View/{groupid}");
    }

    [IsMinister("Minister of Journalism")]
    public async Task<IActionResult> RemovePressPass(long groupid)
    {
        Group? group = DBCache.Get<Group>(groupid);
        if (group == null) return RedirectBack($"Failed to find group {groupid}");

        if (!group.Flags.Contains(GroupFlag.News)) return RedirectBack($"Failed: Group does not have a press pass!");

        group.Flags.Remove(GroupFlag.News);

        StatusMessage = $"Removed press pass from {group.Name}";
        return Redirect($"/Group/View/{groupid}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
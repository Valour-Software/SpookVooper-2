using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Valour.Api.Models;
using SV2.Helpers;
using SV2.Extensions;
using SV2.Database.Models.Districts;
using System.Xml.Linq;
using SV2.Database.Managers;

namespace SV2.Controllers
{
    public class StateController : SVController
    {
        private readonly ILogger<StateController> _logger;

        public StateController(ILogger<StateController> logger)
        {
            _logger = logger;
        }

        public IActionResult View(long id) {
            State? state = DBCache.Get<State>(id);
            if (state is null)
                return RedirectBack();

            return View(state);
        }

        [UserRequired]
        public IActionResult Edit(long id) {
            var user = HttpContext.GetUser();
            State? state = DBCache.Get<State>(id);
            if (state is null)
                return RedirectBack();

            if (!state.CanEdit(user))
                return RedirectBack("You lack permission to manage this state!");

            return View(state);
        }

        [HttpPost]
        [UserRequired]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(State newstate) {
            State? oldstate = DBCache.Get<State>(newstate.Id);
            if (oldstate is null)
                return Redirect("/");

            var user = HttpContext.GetUser();
            if (!oldstate.CanEdit(user))
                return RedirectBack("You lack permission to edit this state!");

            oldstate.Name = newstate.Name;
            oldstate.Description = newstate.Description;
            oldstate.MapColor = newstate.MapColor;

            StatusMessage = "Successfully saved your changes.";
            return Redirect($"/Province/View/{oldstate.Id}");
        }

        [HttpPost("/State/ChangeGovernor/{id}")]
        [ValidateAntiForgeryToken]
        [UserRequired]
        public IActionResult ChangeGovernor(long id, long GovernorId) {
            State? state = DBCache.Get<State>(id);
            if (state is null)
                return Redirect("/");

            var user = HttpContext.GetUser();
            if (state.District.GovernorId != user.Id)
                return RedirectBack("You must be governor of the district to change the governor of a province!");

            state.GovernorId = GovernorId;

            return RedirectBack($"Successfully changed the governorship of this province to {BaseEntity.Find(GovernorId).Name}");
        }
    }
}
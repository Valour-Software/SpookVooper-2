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

namespace SV2.Controllers
{
    public class DistrictController : SVController
    {
        private readonly ILogger<DistrictController> _logger;
        private readonly VooperDB _dbctx;

        public DistrictController(ILogger<DistrictController> logger,
            VooperDB dbctx)
        {
            _logger = logger;
            _dbctx = dbctx;
        }

        [HttpGet("/District/View/{name}")]
        public IActionResult View(string name)
        {
            District district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == name);

            return View(district);
        }

        [UserRequired]
        public IActionResult ManageStates(long Id) {
            District district = DBCache.Get<District>(Id);
            SVUser user = HttpContext.GetUser();

            if (district is null)
                return Redirect("/");

            if (user.Id != district.GovernorId)
                return Redirect("/");

            return View(new ManageStatesModel() {
                States = district.States,
                District = district,
                CreateStateModel = new() {
                    DistrictId = district.Id
                }
            });
        }

        [UserRequired]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateState(CreateStateModel model) {
            SVUser user = HttpContext.GetUser();

            District district = DBCache.Get<District>(model.DistrictId);
            if (district is null)
                return Redirect("/");
            if (user.Id != district.GovernorId)
                return Redirect("/");

            var state = new State() {
                Name = model.Name,
                Description = model.Description,
                MapColor = model.MapColor,
                DistrictId = district.Id
            };
            Group stategroup = new(model.Name, district.GroupId) {
                Id = IdManagers.GroupIdGenerator.Generate(),
                Credits = 0.0m
            };

            DBCache.Put(stategroup.Id, stategroup);
            DBCache.dbctx.Add(stategroup);
            state.GroupId = stategroup.Id;
            state.Id = stategroup.Id;

            var role = new GroupRole() {
                Name = "Governor",
                Color = "ffffff",
                GroupId = stategroup.Id,
                PermissionValue = GroupPermissions.FullControl.Value,
                Id = IdManagers.GeneralIdGenerator.Generate(),
                Authority = 99999999,
                Salary = 0.0m,
                MembersIds = new()
            };
            DBCache.Put(role.Id, role);
            DBCache.dbctx.GroupRoles.Add(role);

            DBCache.Put(state.Id, state);
            DBCache.dbctx.Add(state);

            return RedirectBack("Successfully create state.");
        }

        [UserRequired]
        public IActionResult EditPolicies(long Id)
        {
            District district = DBCache.Get<District>(Id);
            SVUser user = HttpContext.GetUser();

            if (district is null) {
                return Redirect("/");
            }

            if (user.Id != district.GovernorId)
            {
                return Redirect("/");
            }

            DistrictPolicyModel model = new(district);
            return View(model);
        }

        [HttpPost]
        [UserRequired]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPolicies(DistrictPolicyModel model)
        {
            SVUser user = HttpContext.GetUser();

            District district = DBCache.Get<District>(model.DistrictId);
            if (district is null) {
                return Redirect("/");
            }

            if (user.Id != district.GovernorId)
            {
                return Redirect("/");
            }

            // update or create ubi policies
            foreach(UBIPolicy pol in model.UBIPolicies)
            {
                UBIPolicy? oldpol = DBCache.GetAll<UBIPolicy>().FirstOrDefault(x => x.DistrictId == district.Id && x.ApplicableRank == pol.ApplicableRank);
                if (oldpol is not null) 
                {
                    oldpol.Rate = pol.Rate;
                }
                else {
                    pol.Id = IdManagers.GeneralIdGenerator.Generate();
                    pol.DistrictId = model.DistrictId;
                    DBCache.Put(pol.Id, pol);
                    DBCache.dbctx.UBIPolicies.Add(pol);
                }
            }
            
            // update or create tax policies
            foreach(TaxPolicy pol in model.TaxPolicies)
            {
                TaxPolicy? oldpol = DBCache.Get<TaxPolicy>(pol.Id);
                if (oldpol is not null) 
                {
                    if (oldpol.DistrictId != district.Id) {
                        continue;
                    }
                    oldpol.Rate = pol.Rate;
                    oldpol.Minimum = pol.Minimum;
                    oldpol.Maximum = pol.Maximum;
                }
                else {
                    pol.Id = IdManagers.GeneralIdGenerator.Generate();
                    pol.DistrictId = model.DistrictId;
                    DBCache.Put(pol.Id, pol);
                    DBCache.dbctx.TaxPolicies.Add(pol);
                }
            }

            //await _dbctx.SaveChangesAsync();

            StatusMessage = $"Successfully edited policies.";
            return Redirect($"/District/EditPolicies?Id={district.Id}");
        }

        [UserRequired]
        [HttpGet]
        public IActionResult MoveDistrict(long id)
        {
            District district = DBCache.Get<District>(id);

            if (district is null)
                return RedirectBack($"Error: Could not find {district.Name}!");

            SVUser user = HttpContext.GetUser();

            var daysWaited = Math.Round(DateTime.Now.Subtract(user.LastMoved).TotalDays, 0);

            if (daysWaited < 60)
                return RedirectBack($"Error: You must wait another {60 - daysWaited} days to move again!");

            user.DistrictId = district.Id;

            if (user.DistrictId is not null)
                user.LastMoved = DateTime.UtcNow;

            return RedirectBack($"You have moved to {district.Name}!");
        }
    }
}
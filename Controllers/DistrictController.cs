using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using SV2.Managers;
using System.Diagnostics;

namespace SV2.Controllers
{
    [Route("/District")]
    public class DistrictController : Controller
    {
        private readonly ILogger<DistrictController> _logger;
        private readonly VooperDB _dbctx;
        
        [TempData]
        public string StatusMessage { get; set; }

        public DistrictController(ILogger<DistrictController> logger,
            VooperDB dbctx)
        {
            _logger = logger;
            _dbctx = dbctx;
        }

        [HttpGet("View/{name}")]
        public IActionResult View(string name)
        {
            District district = DBCache.GetAll<District>().FirstOrDefault(x => x.Name == name);
            SVUser? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }

            return View(district);
        }

        public IActionResult EditPolicies(long Id)
        {
            District district = DBCache.Get<District>(Id);
            SVUser? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPolicies(DistrictPolicyModel model)
        {
            SVUser? user = UserManager.GetUser(HttpContext);

            if (user is null) 
            {
                return Redirect("/account/login");
            }
            
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
                    _dbctx.UBIPolicies.Add(pol);
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
                    _dbctx.TaxPolicies.Add(pol);
                }
            }

            await _dbctx.SaveChangesAsync();

            StatusMessage = $"Successfully edited policies.";
            return Redirect($"/District/EditPolicies?Id={district.Id}");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
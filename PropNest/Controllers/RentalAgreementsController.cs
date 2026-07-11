using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using PropNest.Services;

namespace PropNest.Controllers
{
    public class RentalAgreementsController : Controller
    {
        private readonly HttpClient _http;
        private readonly RentPaymentService _paymentService;

        public RentalAgreementsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
            _paymentService = new RentPaymentService(factory);
        }

        public async Task<IActionResult> Index(string? filter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Check and update expired agreements
            await _paymentService.CheckAndUpdateExpiredAgreementsAsync();

            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements") ?? new();
            var tenants    = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants") ?? new();
            var units      = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits") ?? new();

            if (filter == "ExpiringSoon")
            {
                var now = DateTime.Now;
                var thirtyDaysFromNow = now.AddDays(30);
                agreements = agreements.Where(a => a.AgreementStatus == "Active" &&
                                                   a.EndDate <= thirtyDaysFromNow &&
                                                   a.EndDate >= now).ToList();
                ViewBag.Filter = "ExpiringSoon";
            }

            // Attach navigation properties (single fetch, no N+1)
            var tenantMap = tenants.ToDictionary(t => t.TenantID);
            var unitMap   = units.ToDictionary(u => u.UnitID);
            foreach (var a in agreements)
            {
                a.Tenant       = tenantMap.GetValueOrDefault(a.TenantID);
                a.PropertyUnit = unitMap.GetValueOrDefault(a.UnitID);
            }

            return View(agreements);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
            if (agreement == null) return NotFound();

            // Attach navigation properties so the view can display tenant/unit names
            agreement.Tenant = await _http.GetFromJsonAsync<Tenant>($"api/Tenants/{agreement.TenantID}");
            agreement.PropertyUnit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");

            // Log activity
            PropNest.Helpers.RecentActivityHelper.LogActivity(
                HttpContext, 
                $"Viewed Rental Agreement: Agr-{agreement.AgreementID}", 
                $"Monthly Rent: Rs. {agreement.MonthlyRent:N0} | Status: {agreement.AgreementStatus}",
                Url.Action("Details", "RentalAgreements", new { id = agreement.AgreementID }) ?? ""
            );

            return View(agreement);
        }

        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");

            // Only show vacant units in dropdown — can't lease an occupied one
            var vacantUnits = units!.Where(u => u.Status == "Vacant").ToList();

            ViewData["TenantID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tenants, "TenantID", "FullName");
            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(vacantUnits, "UnitID", "UnitNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenantID,UnitID,StartDate,EndDate,MonthlyRent,SecurityDeposit,AgreementStatus,Version")] RentalAgreement agreement)
        {
            // Safety check — confirm unit is still vacant before creating agreement
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");
            if (unit == null || unit.Status != "Vacant")
            {
                ModelState.AddModelError(string.Empty, "Selected unit is no longer vacant.");
            }

            if (ModelState.IsValid)
            {
                var response = await _http.PostAsJsonAsync("api/RentalAgreements", agreement);

                if (response.IsSuccessStatusCode)
                {
                    // Flip unit status to Occupied
                    unit!.Status = "Occupied";
                    unit.VacantSince = null;
                    await _http.PutAsJsonAsync($"api/PropertyUnits/{unit.UnitID}", unit);

                    // Fetch the newly created agreement to get its ID
                    var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
                    var createdAgreement = agreements?.OrderByDescending(a => a.AgreementID).FirstOrDefault();

                    if (createdAgreement != null)
                    {
                        // Generate monthly rent payments
                        var payments = _paymentService.GenerateMonthlyPayments(createdAgreement);
                        if (payments.Count > 0)
                        {
                            await _paymentService.CreatePaymentsAsync(payments);
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error creating rental agreement.");
                }
            }

            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            var vacantUnits = units!.Where(u => u.Status == "Vacant").ToList();

            ViewData["TenantID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tenants, "TenantID", "FullName", agreement.TenantID);
            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(vacantUnits, "UnitID", "UnitNumber", agreement.UnitID);
            return View(agreement);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
            if (agreement == null) return NotFound();

            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");

            ViewData["TenantID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tenants, "TenantID", "FullName", agreement.TenantID);
            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", agreement.UnitID);
            return View(agreement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AgreementID,TenantID,UnitID,StartDate,EndDate,MonthlyRent,SecurityDeposit,AgreementStatus,Version")] RentalAgreement agreement)
        {
            if (id != agreement.AgreementID) return NotFound();
            if (ModelState.IsValid)
            {
                var original = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
                
                await _http.PutAsJsonAsync($"api/RentalAgreements/{id}", agreement);

                if (original != null)
                {
                    // Case 1: Unit changed
                    if (original.UnitID != agreement.UnitID)
                    {
                        // Make old unit vacant
                        var oldUnit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{original.UnitID}");
                        if (oldUnit != null)
                        {
                            oldUnit.Status = "Vacant";
                            oldUnit.VacantSince = DateTime.Today;
                            await _http.PutAsJsonAsync($"api/PropertyUnits/{oldUnit.UnitID}", oldUnit);
                        }

                        // Make new unit occupied (if agreement is active)
                        if (agreement.AgreementStatus == "Active")
                        {
                            var newUnit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");
                            if (newUnit != null)
                            {
                                newUnit.Status = "Occupied";
                                newUnit.VacantSince = null;
                                await _http.PutAsJsonAsync($"api/PropertyUnits/{newUnit.UnitID}", newUnit);
                            }
                        }
                    }
                    else
                    {
                        // Case 2: Unit did not change, but agreement status changed
                        if (original.AgreementStatus != agreement.AgreementStatus)
                        {
                            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");
                            if (unit != null)
                            {
                                if (agreement.AgreementStatus == "Expired" || agreement.AgreementStatus == "Terminated")
                                {
                                    unit.Status = "Vacant";
                                    unit.VacantSince = DateTime.Today;
                                }
                                else if (agreement.AgreementStatus == "Active")
                                {
                                    unit.Status = "Occupied";
                                    unit.VacantSince = null;
                                }
                                await _http.PutAsJsonAsync($"api/PropertyUnits/{unit.UnitID}", unit);
                            }
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");

            ViewData["TenantID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tenants, "TenantID", "FullName", agreement.TenantID);
            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", agreement.UnitID);
            return View(agreement);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
            if (agreement == null) return NotFound();
            return View(agreement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete rental agreements.";
                return RedirectToAction(nameof(Index));
            }
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");

            await _http.DeleteAsync($"api/RentalAgreements/{id}");

            // Free up the unit since agreement is gone
            if (agreement != null)
            {
                var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");
                if (unit != null)
                {
                    unit.Status = "Vacant";
                    unit.VacantSince = DateTime.Today;
                    await _http.PutAsJsonAsync($"api/PropertyUnits/{unit.UnitID}", unit);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
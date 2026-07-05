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

        public async Task<IActionResult> Index()
        {
            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            return View(agreements);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
            if (agreement == null) return NotFound();
            return View(agreement);
        }

        public async Task<IActionResult> Create()
        {
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
                await _http.PutAsJsonAsync($"api/RentalAgreements/{id}", agreement);

                // If agreement ended, free up the unit again
                if (agreement.AgreementStatus == "Expired" || agreement.AgreementStatus == "Terminated")
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

            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");

            ViewData["TenantID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(tenants, "TenantID", "FullName", agreement.TenantID);
            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", agreement.UnitID);
            return View(agreement);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{id}");
            if (agreement == null) return NotFound();
            return View(agreement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
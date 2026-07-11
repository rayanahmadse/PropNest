using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using System.Linq;

namespace PropNest.Controllers
{
    public class TenantsController : Controller
    {
        private readonly HttpClient _http;

        public TenantsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        public async Task<IActionResult> Index(string? search)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants") ?? new List<Tenant>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                tenants = tenants.Where(t =>
                    t.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.CNIC.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (t.Email != null && t.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (t.ContactNumber != null && t.ContactNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search;
            return View(tenants);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tenant = await _http.GetFromJsonAsync<Tenant>($"api/Tenants/{id}");
            if (tenant == null) return NotFound();

            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements") ?? new List<RentalAgreement>();
            var payments = await _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments") ?? new List<RentPayment>();
            var requests = await _http.GetFromJsonAsync<List<MaintenanceRequest>>("api/MaintenanceRequests") ?? new List<MaintenanceRequest>();
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits") ?? new List<PropertyUnit>();

            var tenantAgreements = agreements.Where(a => a.TenantID == tenant.TenantID).OrderByDescending(a => a.StartDate).ToList();
            var activeAgreement = tenantAgreements.FirstOrDefault(a => a.AgreementStatus == "Active")
                                  ?? tenantAgreements.FirstOrDefault();

            PropertyUnit? activeUnit = null;
            if (activeAgreement != null)
            {
                activeUnit = units.FirstOrDefault(u => u.UnitID == activeAgreement.UnitID);
            }

            var agreementIds = tenantAgreements.Select(a => a.AgreementID).ToHashSet();
            var tenantPayments = payments.Where(p => agreementIds.Contains(p.AgreementID))
                                         .OrderByDescending(p => p.DueDate)
                                         .ToList();

            var unitIds = tenantAgreements.Select(a => a.UnitID).ToHashSet();
            var tenantRequests = requests.Where(r => unitIds.Contains(r.UnitID))
                                         .OrderByDescending(r => r.DateLogged)
                                         .ToList();

            var vm = new TenantDetailsViewModel
            {
                Tenant = tenant,
                Agreements = tenantAgreements,
                ActiveAgreement = activeAgreement,
                ActiveUnit = activeUnit,
                RentPayments = tenantPayments,
                MaintenanceRequests = tenantRequests
            };

            // Log activity
            PropNest.Helpers.RecentActivityHelper.LogActivity(
                HttpContext, 
                $"Viewed Tenant: {tenant.FullName}", 
                $"CNIC: {tenant.CNIC} | Status: {tenant.Status}",
                Url.Action("Details", "Tenants", new { id = tenant.TenantID }) ?? ""
            );

            return View(vm);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenantID,FullName,CNIC,Email,ContactNumber,EmergencyContact,Status")] Tenant tenant)
        {
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/Tenants", tenant);
                return RedirectToAction(nameof(Index));
            }
            return View(tenant);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var tenant = await _http.GetFromJsonAsync<Tenant>($"api/Tenants/{id}");
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TenantID,FullName,CNIC,Email,ContactNumber,EmergencyContact,Status")] Tenant tenant)
        {
            if (id != tenant.TenantID) return NotFound();
            if (ModelState.IsValid)
            {
                await _http.PutAsJsonAsync($"api/Tenants/{id}", tenant);
                return RedirectToAction(nameof(Index));
            }
            return View(tenant);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var tenant = await _http.GetFromJsonAsync<Tenant>($"api/Tenants/{id}");
            if (tenant == null) return NotFound();
            return View(tenant);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete tenants.";
                return RedirectToAction(nameof(Index));
            }
            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements") ?? new List<RentalAgreement>();
            var hasActiveAgreement = agreements.Any(a => a.TenantID == id && a.AgreementStatus == "Active");

            if (hasActiveAgreement)
            {
                TempData["Error"] = "This tenant cannot be deleted because they still have an active agreement.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _http.DeleteAsync($"api/Tenants/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

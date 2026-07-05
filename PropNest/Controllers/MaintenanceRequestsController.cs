using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using PropNest.Services;

namespace PropNest.Controllers
{
    public class MaintenanceRequestsController : Controller
    {
        private readonly HttpClient _http;
        private readonly MaintenanceRequestService _service;

        public MaintenanceRequestsController(IHttpClientFactory factory, MaintenanceRequestService service)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            // Auto-close old requests before showing the list via injected service
            try
            {
                await _service.AutoCloseOldRequestsAsync();
            }
            catch
            {
                // ignore failures here
            }

            var requests = await _http.GetFromJsonAsync<List<MaintenanceRequest>>("api/MaintenanceRequests");
            return View(requests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var request = await _http.GetFromJsonAsync<MaintenanceRequest>($"api/MaintenanceRequests/{id}");
            if (request == null) return NotFound();
            return View(request);
        }

        public async Task<IActionResult> Create()
        {
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            var staff = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");

            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber");
            ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staff, "StaffID", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitID,StaffID,Category,Description,DateLogged,Status")] MaintenanceRequest maintenanceRequest)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate open requests for same unit/category
                if (!string.IsNullOrEmpty(maintenanceRequest.Category))
                {
                    var duplicate = await _service.DuplicateOpenRequestExistsAsync(maintenanceRequest.UnitID, maintenanceRequest.Category);
                    if (duplicate)
                    {
                        ModelState.AddModelError(string.Empty, "A similar open maintenance request already exists for this unit.");
                        var unitsDup = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
                        var staffDup = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");
                        ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(unitsDup, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
                        ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staffDup, "StaffID", "FullName", maintenanceRequest.StaffID);
                        return View(maintenanceRequest);
                    }
                }

                await _http.PostAsJsonAsync("api/MaintenanceRequests", maintenanceRequest);
                return RedirectToAction(nameof(Index));
            }

            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            var staff = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");

            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
            ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staff, "StaffID", "FullName", maintenanceRequest.StaffID);
            return View(maintenanceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var request = await _http.GetFromJsonAsync<MaintenanceRequest>($"api/MaintenanceRequests/{id}");
            if (request == null) return NotFound();

            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            var staff = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");

            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", request.UnitID);
            ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staff, "StaffID", "FullName", request.StaffID);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RequestID,UnitID,StaffID,Category,Description,DateLogged,Status")] MaintenanceRequest maintenanceRequest)
        {
            if (id != maintenanceRequest.RequestID) return NotFound();
            if (ModelState.IsValid)
            {
                // Enforce allowed status transitions: Open -> In Progress -> Resolved
                var existing = await _http.GetFromJsonAsync<MaintenanceRequest>($"api/MaintenanceRequests/{id}");
                if (existing != null && existing.Status != maintenanceRequest.Status)
                {
                    var allowed = new Dictionary<string, string[]>()
                    {
                        { "Open", new[] { "In Progress", "Resolved" } },
                        { "In Progress", new[] { "Resolved" } },
                        { "Resolved", new string[] { } }
                    };

                    if (allowed.ContainsKey(existing.Status) && !allowed[existing.Status].Contains(maintenanceRequest.Status))
                    {
                        ModelState.AddModelError(string.Empty, $"Invalid status transition from {existing.Status} to {maintenanceRequest.Status}.");
                        var unitsInvalid = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
                        var staffInvalid = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");

                        ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(unitsInvalid, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
                        ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staffInvalid, "StaffID", "FullName", maintenanceRequest.StaffID);
                        return View(maintenanceRequest);
                    }

                    // If marking Resolved, set DateResolved
                    if (maintenanceRequest.Status == "Resolved")
                    {
                        maintenanceRequest.DateResolved = DateTime.Now;
                    }
                }

                await _http.PutAsJsonAsync($"api/MaintenanceRequests/{id}", maintenanceRequest);
                return RedirectToAction(nameof(Index));
            }

            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            var staff = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");

            ViewData["UnitID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(units, "UnitID", "UnitNumber", maintenanceRequest.UnitID);
            ViewData["StaffID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(staff, "StaffID", "FullName", maintenanceRequest.StaffID);
            return View(maintenanceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var request = await _http.GetFromJsonAsync<MaintenanceRequest>($"api/MaintenanceRequests/{id}");
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _http.DeleteAsync($"api/MaintenanceRequests/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

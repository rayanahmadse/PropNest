using Microsoft.AspNetCore.Mvc;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class PropertyUnitsController : Controller
    {
        private readonly HttpClient _http;

        public PropertyUnitsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        public async Task<IActionResult> Index(string? filter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits") ?? new List<PropertyUnit>();

            if (filter == "Vacant")
            {
                units = units.Where(u => u.Status == "Vacant").ToList();
                ViewBag.Filter = "Vacant";
            }

            return View(units);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{id}");
            if (unit == null) return NotFound();

            // Log activity
            PropNest.Helpers.RecentActivityHelper.LogActivity(
                HttpContext, 
                $"Viewed Property Unit: {unit.UnitNumber}", 
                $"Type: {unit.PropertyType} | Status: {unit.Status}",
                Url.Action("Details", "PropertyUnits", new { id = unit.UnitID }) ?? ""
            );

            return View(unit);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            unit.Status = "Vacant";
            ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/PropertyUnits", unit);
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{id}");
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitID,UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            if (id != unit.UnitID) return NotFound();
            if (ModelState.IsValid)
            {
                await _http.PutAsJsonAsync($"api/PropertyUnits/{id}", unit);
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{id}");
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete property units.";
                return RedirectToAction(nameof(Index));
            }
            await _http.DeleteAsync($"api/PropertyUnits/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

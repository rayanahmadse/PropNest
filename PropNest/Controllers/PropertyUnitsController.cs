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

        public async Task<IActionResult> Index()
        {
            var units = await _http.GetFromJsonAsync<List<PropertyUnit>>("api/PropertyUnits");
            return View(units);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{id}");
            if (unit == null) return NotFound();
            return View(unit);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UnitNumber,PropertyType,FloorLevel,AreaSqFt,Amenities,Status,AskingRent,VacantSince")] PropertyUnit unit)
        {
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/PropertyUnits", unit);
                return RedirectToAction(nameof(Index));
            }
            return View(unit);
        }

        public async Task<IActionResult> Edit(int? id)
        {
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
            if (id == null) return NotFound();
            var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{id}");
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _http.DeleteAsync($"api/PropertyUnits/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

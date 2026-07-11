using Microsoft.AspNetCore.Mvc;
using PropNest.Models;

namespace PropNest.Controllers
{
    public class StaffController : Controller
    {
        private readonly HttpClient _http;

        public StaffController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            var staff = await _http.GetFromJsonAsync<List<Staff>>("api/Staff");
            return View(staff);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var staff = await _http.GetFromJsonAsync<Staff>($"api/Staff/{id}");
            if (staff == null) return NotFound();
            return View(staff);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,ContactNumber,Specialty,Status")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/Staff", staff);
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var staff = await _http.GetFromJsonAsync<Staff>($"api/Staff/{id}");
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StaffID,FullName,ContactNumber,Specialty,Status")] Staff staff)
        {
            if (id != staff.StaffID) return NotFound();
            if (ModelState.IsValid)
            {
                await _http.PutAsJsonAsync($"api/Staff/{id}", staff);
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var staff = await _http.GetFromJsonAsync<Staff>($"api/Staff/{id}");
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete staff members.";
                return RedirectToAction(nameof(Index));
            }
            await _http.DeleteAsync($"api/Staff/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

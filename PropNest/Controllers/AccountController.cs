using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace PropNest.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _http;

        public AccountController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        // ── LOGIN ────────────────────────────────────────────────────────────

        public IActionResult Login()
        {
            // If already logged in, go straight to dashboard
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string role)
        {
            var payload = new { Username = username, Password = password };
            var response = await _http.PostAsJsonAsync("api/Users/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            var body = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<JsonElement>(body);
            var actualRole = user.GetProperty("role").GetString();

            if (!string.Equals(actualRole, role, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = $"This account is registered as {actualRole}, not {role}.";
                return View();
            }

            // Store in Session
            HttpContext.Session.SetString("Username", user.GetProperty("username").GetString()!);
            HttpContext.Session.SetString("Role",     actualRole!);
            HttpContext.Session.SetInt32("UserID",    user.GetProperty("userID").GetInt32());

            return RedirectToAction("Index", "Home");
        }

        // ── SIGNUP ───────────────────────────────────────────────────────────

        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(string username, string password, string role)
        {
            var payload = new { Username = username, Password = password, Role = role };
            var response = await _http.PostAsJsonAsync("api/Users/register", payload);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Username already taken. Please choose another.";
                return View();
            }

            TempData["Success"] = "Account created! Please log in.";
            return RedirectToAction("Login");
        }

        // ── LOGOUT ───────────────────────────────────────────────────────────

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}

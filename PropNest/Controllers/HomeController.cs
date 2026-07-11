using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace PropNest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            ViewBag.Role = role;
            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Read list of recent activities using helper
            var activities = PropNest.Helpers.RecentActivityHelper.GetActivities(HttpContext);
            ViewBag.RecentActivities = activities;

            var viewModel = new DashboardViewModel();

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Get rent payments for this month and calculate collected amount
                var paymentResponse = await client.GetAsync("https://localhost:7120/api/RentPayments");
                if (paymentResponse.IsSuccessStatusCode)
                {
                    var paymentContent = await paymentResponse.Content.ReadAsStringAsync();
                    var payments = JArray.Parse(paymentContent);
                    var now = DateTime.Now;
                    var monthStart = new DateTime(now.Year, now.Month, 1);

                    viewModel.TotalRentCollectedThisMonth = payments
                        .Where(p =>
                        {
                            if (p["status"]?.ToString() != "Paid") return false;
                            var dateStr = p["paymentDate"]?.ToString();
                            if (string.IsNullOrWhiteSpace(dateStr)) return false;
                            return DateTime.TryParse(dateStr, out var d) && d >= monthStart;
                        })
                        .Sum(p => decimal.TryParse((p["amountPaid"] ?? p["AmountPaid"] ?? p["amount"])?.ToString(), out var amt) ? amt : 0);

                    viewModel.OverduePaymentsCount = payments
                        .Where(p => p["status"]?.ToString() == "Overdue")
                        .Count();
                }

                // Get property units and count vacant ones
                var unitResponse = await client.GetAsync("https://localhost:7120/api/PropertyUnits");
                if (unitResponse.IsSuccessStatusCode)
                {
                    var unitContent = await unitResponse.Content.ReadAsStringAsync();
                    var units = JArray.Parse(unitContent);
                    viewModel.VacantUnitsCount = units
                        .Where(u => u["status"]?.ToString() == "Vacant")
                        .Count();
                }

                // Get rental agreements and count those expiring in 30 days
                var agreementResponse = await client.GetAsync("https://localhost:7120/api/RentalAgreements");
                if (agreementResponse.IsSuccessStatusCode)
                {
                    var agreementContent = await agreementResponse.Content.ReadAsStringAsync();
                    var agreements = JArray.Parse(agreementContent);
                    var now = DateTime.Now;
                    var thirtyDaysFromNow = now.AddDays(30);

                    viewModel.AgreementsExpiringIn30Days = agreements
                        .Where(a => a["agreementStatus"]?.ToString() == "Active" &&
                                    DateTime.Parse(a["endDate"]?.ToString() ?? "9999-12-31") <= thirtyDaysFromNow &&
                                    DateTime.Parse(a["endDate"]?.ToString() ?? "9999-12-31") >= now)
                        .Count();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dashboard metrics: {ex.Message}");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

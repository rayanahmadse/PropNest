using Microsoft.AspNetCore.Mvc;
using PropNest.Models;
using PropNest.Services;

namespace PropNest.Controllers
{
    public class RentPaymentsController : Controller
    {
        private readonly HttpClient _http;
        private readonly RentPaymentService _paymentService;

        public RentPaymentsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
            _paymentService = new RentPaymentService(factory);
        }

        public async Task<IActionResult> Index(string? search, string? filter)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Check and update overdue payments
            await _paymentService.CheckAndUpdateOverduePaymentsAsync();

            // Check and update expired agreements
            await _paymentService.CheckAndUpdateExpiredAgreementsAsync();

            // Fetch all data in parallel for efficiency
            var paymentsTask   = _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments");
            var agreementsTask = _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            var tenantsTask    = _http.GetFromJsonAsync<List<Tenant>>("api/Tenants");
            await Task.WhenAll(paymentsTask, agreementsTask, tenantsTask);

            var allPayments = paymentsTask.Result  ?? new List<RentPayment>();
            var agreements  = agreementsTask.Result ?? new List<RentalAgreement>();
            var tenants     = tenantsTask.Result    ?? new List<Tenant>();

            // Build "Rayan Ahmad (Arg-12)" label map
            var tenantMap = tenants.ToDictionary(t => t.TenantID, t => t.FullName);
            var agreementLabels = agreements.ToDictionary(
                a => a.AgreementID,
                a => tenantMap.TryGetValue(a.TenantID, out var name)
                        ? $"{name} (Arg-{a.AgreementID})"
                        : $"Arg-{a.AgreementID}"
            );
            ViewBag.AgreementLabels = agreementLabels;

            // Build sequential payment number map (001, 002…) ordered by DueDate per agreement
            var paymentNumbers = new Dictionary<int, string>();
            foreach (var grp in allPayments.GroupBy(p => p.AgreementID))
            {
                int seq = 1;
                foreach (var p in grp.OrderBy(p => p.DueDate))
                    paymentNumbers[p.PaymentID] = seq++.ToString("D3");
            }
            ViewBag.PaymentNumbers = paymentNumbers;

            var payments = allPayments;

            if (filter == "Overdue")
            {
                payments = payments.Where(p => p.Status == "Overdue").ToList();
                ViewBag.Filter = "Overdue";
            }
            else if (filter == "CollectedThisMonth")
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                payments = payments.Where(p => p.Status == "Paid" && p.PaymentDate >= monthStart).ToList();
                ViewBag.Filter = "CollectedThisMonth";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                payments = payments.Where(p =>
                    p.PaymentID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.AgreementID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.PaymentMethod.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (agreementLabels.TryGetValue(p.AgreementID, out var label) &&
                     label.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search;
            return View(payments);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();

            payment.RentalAgreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{payment.AgreementID}");
            if (payment.RentalAgreement != null)
            {
                payment.RentalAgreement.Tenant = await _http.GetFromJsonAsync<Tenant>($"api/Tenants/{payment.RentalAgreement.TenantID}");
            }

            // Log activity
            PropNest.Helpers.RecentActivityHelper.LogActivity(
                HttpContext, 
                $"Viewed Rent Payment #{payment.PaymentID}", 
                $"Amount: Rs. {payment.AmountPaid:N0} | Status: {payment.Status}",
                Url.Action("Details", "RentPayments", new { id = payment.PaymentID }) ?? ""
            );

            return View(payment);
        }

        public async Task<IActionResult> MarkReminderSent(int id)
        {
            var resp = await _http.PutAsync($"api/RentPayments/{id}/send-reminder", null);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to mark reminder as sent.";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReceipt(int id)
        {
            // Call API to generate receipt
            var resp = await _http.PostAsync($"api/RentPayments/{id}/generate-receipt", null);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to generate receipt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Fetch the generated PDF from API and return directly to browser
            var getResp = await _http.GetAsync($"api/RentPayments/{id}/receipt");
            if (!getResp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Receipt generated but failed to download.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var bytes = await getResp.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", $"receipt_{id}.pdf");
        }

        [HttpGet("RentPayments/DownloadReceipt/{id}")]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var resp = await _http.GetAsync($"api/RentPayments/{id}/receipt");
            if (!resp.IsSuccessStatusCode) return NotFound();
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", $"receipt_{id}.pdf");
        }

        // Helper: build SelectList with "Tenant Name (Arg-ID)" display text
        private async Task<Microsoft.AspNetCore.Mvc.Rendering.SelectList> BuildAgreementSelectListAsync(int? selectedId = null)
        {
            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements") ?? new List<RentalAgreement>();
            var tenants = await _http.GetFromJsonAsync<List<Tenant>>("api/Tenants") ?? new List<Tenant>();
            var tenantMap = tenants.ToDictionary(t => t.TenantID, t => t.FullName);

            var items = agreements.Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = a.AgreementID.ToString(),
                Text  = tenantMap.TryGetValue(a.TenantID, out var name)
                            ? $"{name} (Arg-{a.AgreementID})"
                            : $"Arg-{a.AgreementID}"
            }).ToList();

            return new Microsoft.AspNetCore.Mvc.Rendering.SelectList(items, "Value", "Text", selectedId?.ToString());
        }

        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            ViewData["AgreementID"] = await BuildAgreementSelectListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AgreementID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status")] RentPayment payment)
        {
            if (ModelState.IsValid)
            {
                var response = await _http.PostAsJsonAsync("api/RentPayments", payment);
                if (response.IsSuccessStatusCode && payment.Status == "Paid")
                {
                    var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{payment.AgreementID}");
                    if (agreement != null)
                    {
                        var nextDueDate = payment.DueDate.AddMonths(1);
                        if (nextDueDate <= agreement.EndDate)
                        {
                            var allPayments = await _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments") ?? new();
                            var exists = allPayments.Any(p => p.AgreementID == payment.AgreementID && 
                                                              p.DueDate.Year == nextDueDate.Year && 
                                                              p.DueDate.Month == nextDueDate.Month);
                            if (!exists)
                            {
                                var nextPayment = new RentPayment
                                {
                                    AgreementID = payment.AgreementID,
                                    DueDate = nextDueDate,
                                    PaymentDate = null,
                                    AmountPaid = agreement.MonthlyRent,
                                    PaymentMethod = "Cash",
                                    Status = "Pending"
                                };
                                await _http.PostAsJsonAsync("api/RentPayments", nextPayment);
                            }
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AgreementID"] = await BuildAgreementSelectListAsync(payment.AgreementID);
            return View(payment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();

            ViewData["AgreementID"] = await BuildAgreementSelectListAsync(payment.AgreementID);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentID,AgreementID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status,ReceiptPath,ReceiptGenerated,LateFeeAmount,LateFeeApplied,ReminderSentAt,ReminderSent")] RentPayment payment)
        {
            if (id != payment.PaymentID) return NotFound();
            if (ModelState.IsValid)
            {
                var original = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
                await _http.PutAsJsonAsync($"api/RentPayments/{id}", payment);

                // If status updated to Paid from unpaid
                if (payment.Status == "Paid" && original != null && original.Status != "Paid")
                {
                    var agreement = await _http.GetFromJsonAsync<RentalAgreement>($"api/RentalAgreements/{payment.AgreementID}");
                    if (agreement != null)
                    {
                        var nextDueDate = payment.DueDate.AddMonths(1);
                        if (nextDueDate <= agreement.EndDate)
                        {
                            var allPayments = await _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments") ?? new();
                            var exists = allPayments.Any(p => p.AgreementID == payment.AgreementID && 
                                                              p.DueDate.Year == nextDueDate.Year && 
                                                              p.DueDate.Month == nextDueDate.Month);
                            if (!exists)
                            {
                                var nextPayment = new RentPayment
                                {
                                    AgreementID = payment.AgreementID,
                                    DueDate = nextDueDate,
                                    PaymentDate = null,
                                    AmountPaid = agreement.MonthlyRent,
                                    PaymentMethod = "Cash",
                                    Status = "Pending"
                                };
                                await _http.PostAsJsonAsync("api/RentPayments", nextPayment);
                            }
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AgreementID"] = await BuildAgreementSelectListAsync(payment.AgreementID);
            return View(payment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Only Admins can delete rent payments.";
                return RedirectToAction(nameof(Index));
            }
            await _http.DeleteAsync($"api/RentPayments/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

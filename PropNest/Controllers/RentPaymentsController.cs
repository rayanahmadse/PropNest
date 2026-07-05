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

        public async Task<IActionResult> Index(string? search)
        {
            // Stage 7 automation: overdue, late fees, reminders
            await _paymentService.CheckAndUpdateOverduePaymentsAsync();
            await _http.PostAsync("api/RentPayments/run-stage7-automation", null);

            var payments = await _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments") ?? new List<RentPayment>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                payments = payments.Where(p =>
                    p.PaymentID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.AgreementID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.PaymentMethod.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            ViewBag.Search = search;
            return View(payments);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();
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

        public async Task<IActionResult> Create()
        {
            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            ViewData["AgreementID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(agreements, "AgreementID", "AgreementID");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AgreementID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status")] RentPayment payment)
        {
            if (ModelState.IsValid)
            {
                await _http.PostAsJsonAsync("api/RentPayments", payment);
                return RedirectToAction(nameof(Index));
            }

            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            ViewData["AgreementID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(agreements, "AgreementID", "AgreementID", payment.AgreementID);
            return View(payment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();

            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            ViewData["AgreementID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(agreements, "AgreementID", "AgreementID", payment.AgreementID);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentID,AgreementID,PaymentDate,DueDate,AmountPaid,PaymentMethod,Status,ReceiptPath,ReceiptGenerated,LateFeeAmount,LateFeeApplied,ReminderSentAt,ReminderSent")] RentPayment payment)
        {
            if (id != payment.PaymentID) return NotFound();
            if (ModelState.IsValid)
            {
                await _http.PutAsJsonAsync($"api/RentPayments/{id}", payment);
                return RedirectToAction(nameof(Index));
            }

            var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
            ViewData["AgreementID"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(agreements, "AgreementID", "AgreementID", payment.AgreementID);
            return View(payment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _http.GetFromJsonAsync<RentPayment>($"api/RentPayments/{id}");
            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _http.DeleteAsync($"api/RentPayments/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}

using PropNest.Models;

namespace PropNest.Services
{
    public class RentPaymentService
    {
        private readonly HttpClient _http;

        public RentPaymentService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        /// <summary>
        /// Generate monthly rent payments automatically from StartDate to EndDate
        /// </summary>
        public List<RentPayment> GenerateMonthlyPayments(RentalAgreement agreement)
        {
            var payments = new List<RentPayment>();

            // Start from the agreement's StartDate
            var currentDueDate = agreement.StartDate;

            // Generate one payment per month until EndDate
            while (currentDueDate <= agreement.EndDate)
            {
                var payment = new RentPayment
                {
                    AgreementID = agreement.AgreementID,
                    DueDate = currentDueDate,
                    PaymentDate = null, // Not paid yet
                    AmountPaid = agreement.MonthlyRent,
                    PaymentMethod = "Pending",
                    Status = "Pending"
                };

                payments.Add(payment);

                // Move to next month
                currentDueDate = currentDueDate.AddMonths(1);
            }

            return payments;
        }

        /// <summary>
        /// Create multiple rent payments via API
        /// </summary>
        public async Task<bool> CreatePaymentsAsync(List<RentPayment> payments)
        {
            try
            {
                foreach (var payment in payments)
                {
                    var response = await _http.PostAsJsonAsync("api/RentPayments", payment);
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check and update overdue payments
        /// </summary>
        public async Task CheckAndUpdateOverduePaymentsAsync()
        {
            try
            {
                var allPayments = await _http.GetFromJsonAsync<List<RentPayment>>("api/RentPayments");

                if (allPayments == null) return;

                var today = DateTime.Today;

                foreach (var payment in allPayments.Where(p => p.Status == "Pending" && p.DueDate < today))
                {
                    // Update to Overdue
                    payment.Status = "Overdue";
                    await _http.PutAsJsonAsync($"api/RentPayments/{payment.PaymentID}", payment);
                }
            }
            catch
            {
                // Silently fail - don't break the app if overdue check fails
            }
        }
    }
}

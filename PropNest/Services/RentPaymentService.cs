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
        /// Generate the first rent payment when an agreement is created.
        /// DueDate = StartDate + 1 month (e.g. start July 11 → due August 11).
        /// Only creates a single "Pending" payment — subsequent months are added
        /// when the previous month's payment is marked Paid.
        /// </summary>
        public List<RentPayment> GenerateMonthlyPayments(RentalAgreement agreement)
        {
            var firstDueDate = agreement.StartDate.AddMonths(1);

            // Don't create payment if it falls past the agreement end date
            if (firstDueDate > agreement.EndDate)
                return new List<RentPayment>();

            return new List<RentPayment>
            {
                new RentPayment
                {
                    AgreementID   = agreement.AgreementID,
                    DueDate       = firstDueDate,
                    PaymentDate   = null,
                    AmountPaid    = agreement.MonthlyRent,
                    PaymentMethod = "Cash",   // "Pending" violates DB constraint; Cash = placeholder
                    Status        = "Pending"
                }
            };
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

        /// <summary>
        /// Check and update expired agreements, freeing their property units
        /// </summary>
        public async Task CheckAndUpdateExpiredAgreementsAsync()
        {
            try
            {
                var agreements = await _http.GetFromJsonAsync<List<RentalAgreement>>("api/RentalAgreements");
                if (agreements == null) return;

                var today = DateTime.Today;
                foreach (var agreement in agreements.Where(a => a.AgreementStatus == "Active" && a.EndDate < today))
                {
                    // Update agreement status to Expired
                    agreement.AgreementStatus = "Expired";
                    await _http.PutAsJsonAsync($"api/RentalAgreements/{agreement.AgreementID}", agreement);

                    // Free up the unit
                    var unit = await _http.GetFromJsonAsync<PropertyUnit>($"api/PropertyUnits/{agreement.UnitID}");
                    if (unit != null)
                    {
                        unit.Status = "Vacant";
                        unit.VacantSince = today;
                        await _http.PutAsJsonAsync($"api/PropertyUnits/{unit.UnitID}", unit);
                    }
                }
            }
            catch
            {
                // Silently fail
            }
        }
    }
}

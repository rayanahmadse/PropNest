using PropNest.Models;

namespace PropNest.Services
{
    public class MaintenanceRequestService
    {
        private readonly HttpClient _http;

        public MaintenanceRequestService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7120/");
        }

        public async Task<int> AutoCloseOldRequestsAsync(int days = 30)
        {
            var response = await _http.PostAsync($"api/MaintenanceRequests/auto-close-old?days={days}", null);
            if (!response.IsSuccessStatusCode) return 0;
            var result = await response.Content.ReadFromJsonAsync<AutoCloseResult>();
            return result?.closed ?? 0;
        }

        public async Task<bool> DuplicateOpenRequestExistsAsync(int unitId, string category)
        {
            if (string.IsNullOrEmpty(category)) return false;
            var response = await _http.PostAsync($"api/MaintenanceRequests/check-duplicate?unitId={unitId}&category={Uri.EscapeDataString(category)}", null);
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<DuplicateCheckResult>();
            return result?.exists ?? false;
        }

        public async Task<List<MaintenanceRequest>?> GetByUnitAsync(int unitId)
        {
            return await _http.GetFromJsonAsync<List<MaintenanceRequest>>($"api/MaintenanceRequests/by-unit/{unitId}");
        }

        public async Task<List<MaintenanceRequest>?> GetOverdueOpenAsync(int days = 30)
        {
            return await _http.GetFromJsonAsync<List<MaintenanceRequest>>($"api/MaintenanceRequests/overdue-open?days={days}");
        }

        private class AutoCloseResult { public int closed { get; set; } }
        private class DuplicateCheckResult { public bool exists { get; set; } }
    }
}

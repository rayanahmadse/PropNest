namespace PropNest.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRentCollectedThisMonth { get; set; }
        public int VacantUnitsCount { get; set; }
        public int OverduePaymentsCount { get; set; }
        public int AgreementsExpiringIn30Days { get; set; }
    }
}

namespace PropNest.Models
{
    public class LeaseContract
    {
        public int LeaseID { get; set; }
        public int TenantID { get; set; }
        public int UnitID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal? SecurityDeposit { get; set; }
        public string LeaseStatus { get; set; } = "Active";
        public int Version { get; set; } = 1;

        // Navigation properties
        public Tenant? Tenant { get; set; }
        public PropertyUnit? PropertyUnit { get; set; }
    }
}
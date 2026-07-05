namespace PropNest.Models
{
    public class TenantDetailsViewModel
    {
        public Tenant? Tenant { get; set; }
        public List<RentalAgreement> Agreements { get; set; } = new();
        public RentalAgreement? ActiveAgreement { get; set; }
        public PropertyUnit? ActiveUnit { get; set; }
        public List<RentPayment> RentPayments { get; set; } = new();
        public List<MaintenanceRequest> MaintenanceRequests { get; set; } = new();
    }
}

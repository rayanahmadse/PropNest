namespace PropNest.Models
{
    public class RentPayment
    {
        public int PaymentID { get; set; }
        public int LeaseID { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string? PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Pending";

        // Navigation property
        public LeaseContract? LeaseContract { get; set; }
    }
}
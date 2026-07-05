using Microsoft.EntityFrameworkCore;

namespace PropNest.Models
{
    public class PropNestContext : DbContext
    {
        public PropNestContext(DbContextOptions<PropNestContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<PropertyUnit> PropertyUnits { get; set; }
        public DbSet<RentalAgreement> RentalAgreements { get; set; }
        public DbSet<RentPayment> RentPayments { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tenant
            modelBuilder.Entity<Tenant>().ToTable("Tenant");
            modelBuilder.Entity<Tenant>().HasKey(t => t.TenantID);

            // PropertyUnit
            modelBuilder.Entity<PropertyUnit>().ToTable("PropertyUnit");
            modelBuilder.Entity<PropertyUnit>().HasKey(p => p.UnitID);
            modelBuilder.Entity<PropertyUnit>().Property(p => p.AreaSqFt).HasPrecision(18, 2);
            modelBuilder.Entity<PropertyUnit>().Property(p => p.AskingRent).HasPrecision(18, 2);

            // Staff
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Staff>().HasKey(s => s.StaffID);

            // RentalAgreement
            modelBuilder.Entity<RentalAgreement>().ToTable("RentalAgreement");
            modelBuilder.Entity<RentalAgreement>().HasKey(l => l.AgreementID);
            modelBuilder.Entity<RentalAgreement>().Property(l => l.MonthlyRent).HasPrecision(18, 2);
            modelBuilder.Entity<RentalAgreement>().Property(l => l.SecurityDeposit).HasPrecision(18, 2);
            modelBuilder.Entity<RentalAgreement>()
                .HasOne(l => l.Tenant)
                .WithMany()
                .HasForeignKey(l => l.TenantID);
            modelBuilder.Entity<RentalAgreement>()
                .HasOne(l => l.PropertyUnit)
                .WithMany()
                .HasForeignKey(l => l.UnitID);

            // RentPayment
            modelBuilder.Entity<RentPayment>().ToTable("RentPayment");
            modelBuilder.Entity<RentPayment>().HasKey(r => r.PaymentID);
            modelBuilder.Entity<RentPayment>().Property(r => r.AmountPaid).HasPrecision(18, 2);
            modelBuilder.Entity<RentPayment>()
                .HasOne(r => r.RentalAgreement)
                .WithMany()
                .HasForeignKey(r => r.AgreementID);

            // MaintenanceRequest
            modelBuilder.Entity<MaintenanceRequest>().ToTable("MaintenanceRequest");
            modelBuilder.Entity<MaintenanceRequest>().HasKey(m => m.RequestID);
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.PropertyUnit)
                .WithMany()
                .HasForeignKey(m => m.UnitID);
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Staff)
                .WithMany()
                .HasForeignKey(m => m.StaffID);
        }
    }
}
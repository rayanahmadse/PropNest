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
        public DbSet<LeaseContract> LeaseContracts { get; set; }
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

            // Staff
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Staff>().HasKey(s => s.StaffID);

            // LeaseContract
            modelBuilder.Entity<LeaseContract>().ToTable("LeaseContract");
            modelBuilder.Entity<LeaseContract>().HasKey(l => l.LeaseID);
            modelBuilder.Entity<LeaseContract>()
                .HasOne(l => l.Tenant)
                .WithMany()
                .HasForeignKey(l => l.TenantID);
            modelBuilder.Entity<LeaseContract>()
                .HasOne(l => l.PropertyUnit)
                .WithMany()
                .HasForeignKey(l => l.UnitID);

            // RentPayment
            modelBuilder.Entity<RentPayment>().ToTable("RentPayment");
            modelBuilder.Entity<RentPayment>().HasKey(r => r.PaymentID);
            modelBuilder.Entity<RentPayment>()
                .HasOne(r => r.LeaseContract)
                .WithMany()
                .HasForeignKey(r => r.LeaseID);

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
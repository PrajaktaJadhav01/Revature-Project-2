using Microsoft.EntityFrameworkCore;

namespace CustomerManagement
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerInteraction> CustomerInteractions { get; set; }
        public DbSet<CustomerAudit> CustomerAudits { get; set; }
        public DbSet<Segment> Segments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer("Server=.\\SQLEXPRESS;Database=CustomerManagementDB;Trusted_Connection=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customer");
            modelBuilder.Entity<Customer>().Property(c => c.AccountValue).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<ContactPerson>().ToTable("ContactPerson");
            modelBuilder.Entity<CustomerAddress>().ToTable("CustomerAddress");
            modelBuilder.Entity<CustomerInteraction>().ToTable("CustomerInteraction");
            modelBuilder.Entity<CustomerAudit>().ToTable("CustomerAudit");
            modelBuilder.Entity<Segment>().ToTable("Segment");
        }
    }
}
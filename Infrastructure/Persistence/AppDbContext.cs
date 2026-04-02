using CustomerManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagementSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.CustomerId);
            entity.HasIndex(c => c.Email);
            entity.HasIndex(c => c.Classification);
            entity.HasIndex(c => c.Segment);
            entity.HasIndex(c => new { c.Email, c.CustomerName }).IsUnique();

            entity.HasMany(c => c.Contacts)
                .WithOne(c => c.Customer!)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Addresses)
                .WithOne(a => a.Customer!)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Interactions)
                .WithOne(i => i.Customer!)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(c => c.AccountValue).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("CustomerAddress");
            entity.HasKey(a => a.AddressId);
            entity.Property(a => a.AddressId).HasColumnName("AddressId");
            entity.Property(a => a.CustomerId).HasColumnName("CustomerId");
            entity.Property(a => a.AddressType).HasConversion<string>().HasColumnName("AddressType");
            entity.Property(a => a.Street).HasColumnName("Street");
            entity.Property(a => a.City).HasColumnName("City");
            entity.Property(a => a.State).HasColumnName("State");
            entity.Property(a => a.PostalCode).HasColumnName("PostalCode");
            entity.Property(a => a.Country).HasColumnName("Country");
            entity.Property(a => a.IsPrimary).HasColumnName("IsPrimary");
            entity.HasIndex(a => new { a.CustomerId, a.IsPrimary }).HasFilter("[IsPrimary] = 1");
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.ToTable("CustomerInteraction");
            entity.HasKey(i => i.InteractionId);
            entity.Property(i => i.InteractionId).HasColumnName("InteractionId");
            entity.Property(i => i.CustomerId).HasColumnName("CustomerId");
            entity.Property(i => i.InteractionDate).HasColumnName("InteractionDate");
            entity.Property(i => i.Type).HasConversion<string>().HasColumnName("InteractionType");
            entity.Property(i => i.Subject).HasColumnName("Subject");
            entity.Property(i => i.Details).HasColumnName("Details");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("ContactPerson");
            entity.HasKey(c => c.ContactId);
            entity.Property(c => c.ContactId).HasColumnName("ContactPersonId");
            entity.Property(c => c.CustomerId).HasColumnName("CustomerId");
            entity.Property(c => c.FirstName).HasColumnName("FirstName");
            entity.Property(c => c.LastName).HasColumnName("LastName");
            entity.Property(c => c.Email).HasColumnName("Email");
            entity.Property(c => c.Phone).HasColumnName("Phone");
            entity.Property(c => c.Title).HasColumnName("Title");
            entity.Property(c => c.IsPrimary).HasColumnName("IsPrimary");
            entity.HasIndex(c => new { c.CustomerId, c.IsPrimary }).HasFilter("[IsPrimary] = 1");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}

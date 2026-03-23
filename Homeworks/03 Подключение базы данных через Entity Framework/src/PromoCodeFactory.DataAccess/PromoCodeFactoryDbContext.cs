using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess;

public class PromoCodeFactoryDbContext : DbContext
{
    public PromoCodeFactoryDbContext(DbContextOptions<PromoCodeFactoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerPromoCode> CustomerPromoCodes { get; set; }
    public DbSet<Preference> Preferences { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Preference>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.ServiceInfo).HasMaxLength(200);
            entity.Property(e => e.PartnerName).HasMaxLength(100);
        });

        base.OnModelCreating(modelBuilder);
    }
}

using GLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Data;

public class GlmsDbContext(DbContextOptions<GlmsDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().HasMany(c => c.Contracts).WithOne(c => c.Client)
            .HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Contract>().HasMany(c => c.ServiceRequests).WithOne(r => r.Contract)
            .HasForeignKey(r => r.ContractId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Contract>().Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<ServiceRequest>().Property(r => r.AmountUsd).HasPrecision(18, 2);
        modelBuilder.Entity<ServiceRequest>().Property(r => r.CostZar).HasPrecision(18, 2);
        modelBuilder.Entity<ServiceRequest>().Property(r => r.ExchangeRate).HasPrecision(18, 6);
    }
}

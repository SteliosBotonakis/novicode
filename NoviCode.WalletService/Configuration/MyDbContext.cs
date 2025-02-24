using NoviCode.WalletService.Data;
using Microsoft.EntityFrameworkCore;

namespace NoviCode.WalletService.Configuration;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    { }

    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the composite key for ExchangeRate (Date + Currency)
        modelBuilder.Entity<ExchangeRate>()
            .HasKey(e => new { e.Date, e.Currency });
        
        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasColumnType("decimal(18,2)");
    }
}
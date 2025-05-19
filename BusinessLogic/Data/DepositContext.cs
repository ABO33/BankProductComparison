using Microsoft.EntityFrameworkCore;
using BusinessLogic.Models;

namespace BusinessLogic.Data
{
    public class DepositContext : DbContext
    {
        public DepositContext(DbContextOptions<DepositContext> options)
            : base(options)
        {
        }

        // Your existing deposits table
        public DbSet<Deposit> Deposits { get; set; } = null!;

        // NEW: the catalog table
        public DbSet<DepositCatalog> DepositCatalogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // (Optional) further Fluent API configuration:
            // e.g. modelBuilder.Entity<DepositCatalog>().Property(d => d.BankName).HasMaxLength(200);
        }
    }
}

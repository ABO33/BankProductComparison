using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using BusinessLogic.Models;


namespace BusinessLogic.Data
{
    public class DepositContext : DbContext
    {
        public DepositContext(DbContextOptions<DepositContext> options)
            : base(options)
        {
        }

        public DbSet<Deposit> Deposits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed example deposits
            modelBuilder.Entity<Deposit>().HasData(
                new Deposit { Id = 1, Name = "Standard 6-Month", BankName = "Bank A", Currency = "BGN", MinAmount = 5000, MaxAmount = 100000, MinTermMonths = 6, MaxTermMonths = 6, InterestRate = 1.5m, TaxRate = 10m },
                new Deposit { Id = 2, Name = "Variable 12-Month", BankName = "Bank B", Currency = "BGN", MinAmount = 1000, MaxAmount = 50000, MinTermMonths = 12, MaxTermMonths = 12, InterestRate = 2.0m, TaxRate = 10m },
                new Deposit { Id = 3, Name = "Long-Term 24-Month", BankName = "Bank C", Currency = "EUR", MinAmount = 2000, MaxAmount = 200000, MinTermMonths = 24, MaxTermMonths = 24, InterestRate = 1.8m, TaxRate = 10m }
            );
        }
    }
}

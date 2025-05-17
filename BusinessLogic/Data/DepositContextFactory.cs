using BusinessLogic.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BusinessLogic.Data
{
    public class DepositContextFactory : IDesignTimeDbContextFactory<DepositContext>
    {
        public DepositContext CreateDbContext(string[] args)
        {
            // Hard-coded dev connection string using your bankuser account:
            var connString =
                "Server=localhost;Port=3306;Database=BankProductsDb;User=bankuser;Password=BankUser!2025;";

            var optionsBuilder = new DbContextOptionsBuilder<DepositContext>();
            optionsBuilder.UseMySql(
                connString,
                ServerVersion.AutoDetect(connString)
            );

            return new DepositContext(optionsBuilder.Options);
        }
    }
}

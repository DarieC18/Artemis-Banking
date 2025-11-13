using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Persistence
{
    public class ArtemisBankingDbContext : DbContext
    {
        public ArtemisBankingDbContext(DbContextOptions<ArtemisBankingDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanPaymentSchedule> LoanPaymentSchedules { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<CreditCardConsumption> CreditCardConsumptions { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArtemisBankingDbContext).Assembly);
        }
    }
}

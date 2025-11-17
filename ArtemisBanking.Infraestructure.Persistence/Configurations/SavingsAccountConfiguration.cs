using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
    {
        public void Configure(EntityTypeBuilder<SavingsAccount> builder)
        {
            builder.ToTable("SavingsAccounts");

            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.NumeroCuenta)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(sa => sa.Balance)
                   .HasColumnType("decimal(18,2)");

            builder.Property(sa => sa.UserId)
                   .IsRequired();

            builder.Property(sa => sa.FechaCreacion)
                   .IsRequired();

            builder.Property(sa => sa.EsPrincipal)
                   .IsRequired();

            builder.Property(sa => sa.IsActive)
                   .IsRequired();

            builder.HasMany(sa => sa.Transactions)
                   .WithOne(t => t.SavingsAccount)
                   .HasForeignKey(t => t.SavingsAccountId);

            builder.HasIndex(sa => sa.NumeroCuenta)
                   .IsUnique();

        }
    }
}

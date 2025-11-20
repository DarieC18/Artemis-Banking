using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Monto)
                   .HasColumnType("decimal(18,2)");

            builder.Property(t => t.FechaTransaccion)
                   .IsRequired();

            builder.Property(t => t.Tipo)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(t => t.Beneficiario)
                   .HasMaxLength(50);

            builder.Property(t => t.Origen)
                   .HasMaxLength(50);

            builder.Property(t => t.Estado)
                   .HasMaxLength(20);

            builder.HasOne(t => t.SavingsAccount)
                   .WithMany(sa => sa.Transactions)
                   .HasForeignKey(t => t.SavingsAccountId);

            builder.Property(t => t.OperationType)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(t => t.OperatedByUserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.HasIndex(t => new { t.OperatedByUserId, t.FechaTransaccion });

        }
    }
}

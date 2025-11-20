using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("Loans");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.NumeroPrestamo)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(l => l.MontoCapital)
                   .HasColumnType("decimal(18,2)");

            builder.Property(l => l.MontoPendiente)
                   .HasColumnType("decimal(18,2)");

            builder.Property(l => l.TasaInteres)
                   .HasColumnType("decimal(5,2)");

            builder.Property(l => l.PlazoMeses)
                   .IsRequired();

            builder.Property(l => l.IsActive)
                   .IsRequired();

            builder.Property(l => l.FechaCreacion)
                   .IsRequired();

            builder.Property(l => l.UserId)
                   .IsRequired();

            builder.Property(l => l.AdminUserId)
                   .IsRequired();

            builder.HasMany(l => l.TablaAmortizacion)
                   .WithOne(ps => ps.Loan)
                   .HasForeignKey(ps => ps.LoanId);
        }
    }
}

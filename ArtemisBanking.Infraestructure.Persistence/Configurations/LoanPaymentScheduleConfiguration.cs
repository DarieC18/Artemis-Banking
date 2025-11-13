using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class LoanPaymentScheduleConfiguration : IEntityTypeConfiguration<LoanPaymentSchedule>
    {
        public void Configure(EntityTypeBuilder<LoanPaymentSchedule> builder)
        {
            builder.ToTable("LoanPaymentSchedules");

            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.NumeroCuota)
                   .IsRequired();

            builder.Property(ps => ps.ValorCuota)
                   .HasColumnType("decimal(18,2)");

            builder.Property(ps => ps.SaldoPendiente)
                   .HasColumnType("decimal(18,2)");

            builder.Property(ps => ps.FechaPago)
                   .IsRequired();

            builder.Property(ps => ps.Pagada)
                   .IsRequired();

            builder.Property(ps => ps.Atrasada)
                   .IsRequired();
        }
    }
}

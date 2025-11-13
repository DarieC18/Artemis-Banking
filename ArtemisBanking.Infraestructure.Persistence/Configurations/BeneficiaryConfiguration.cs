using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            builder.ToTable("Beneficiaries");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId)
                   .IsRequired();

            builder.Property(b => b.NumeroCuentaBeneficiario)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(b => b.NombreBeneficiario)
                   .HasMaxLength(100);

            builder.Property(b => b.ApellidoBeneficiario)
                   .HasMaxLength(100);

            builder.Property(b => b.FechaCreacion)
                   .IsRequired();
        }
    }
}

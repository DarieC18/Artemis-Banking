using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class CreditCardConsumptionConfiguration : IEntityTypeConfiguration<CreditCardConsumption>
    {
        public void Configure(EntityTypeBuilder<CreditCardConsumption> builder)
        {
            builder.ToTable("CreditCardConsumptions");

            builder.HasKey(cc => cc.Id);

            builder.Property(cc => cc.Monto)
                   .HasColumnType("decimal(18,2)");

            builder.Property(cc => cc.FechaConsumo)
                   .IsRequired();

            builder.Property(cc => cc.Comercio)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(cc => cc.Estado)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(cc => cc.EsAvanceEfectivo)
                   .IsRequired();

        }
    }
}

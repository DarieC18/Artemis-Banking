using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Persistence.Configurations
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            builder.ToTable("CreditCards");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.NumeroTarjeta)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.LimiteCredito)
                   .HasColumnType("decimal(18,2)");

            builder.Property(c => c.DeudaActual)
                   .HasColumnType("decimal(18,2)");

            builder.Property(c => c.FechaExpiracion)
                   .IsRequired()
                   .HasMaxLength(10); // ej: MM/AA o MM/AAAA

            builder.Property(c => c.CVCHash)
                   .IsRequired();

            builder.Property(c => c.IsActive)
                   .IsRequired();

            builder.Property(c => c.FechaCreacion)
                   .IsRequired();

            builder.Property(c => c.UserId)
                   .IsRequired();

            builder.Property(c => c.AdminUserId)
                   .IsRequired();

            builder.HasMany(c => c.Consumos)
                   .WithOne(cc => cc.CreditCard)
                   .HasForeignKey(cc => cc.CreditCardId);
        }
    }
}

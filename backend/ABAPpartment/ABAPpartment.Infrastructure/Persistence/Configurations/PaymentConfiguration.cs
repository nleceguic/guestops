using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
               .IsRequired()
               .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Type)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(p => p.Method)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(p => p.Status)
               .IsRequired()
               .HasMaxLength(30)
               .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(p => p.TransactionRef)
               .HasMaxLength(200);

        builder.HasOne(p => p.Reservation)
               .WithMany(r => r.Payments)
               .HasForeignKey(p => p.ReservationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
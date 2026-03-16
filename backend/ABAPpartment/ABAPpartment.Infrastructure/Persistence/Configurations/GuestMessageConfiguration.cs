using ABAPpartment.Application.Services;
using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class GuestMessageConfiguration : IEntityTypeConfiguration<GuestMessage>
{
    public void Configure(EntityTypeBuilder<GuestMessage> builder)
    {
        builder.ToTable("GuestMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Channel)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(m => m.Direction)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(m => m.Body)
               .IsRequired()
               .HasMaxLength(4000);

        builder.Property(m => m.AIConfidence)
               .HasColumnType("decimal(5,2)");

        builder.Property(m => m.DetectedTopic)
               .HasMaxLength(50);

        builder.HasIndex(m => new { m.GuestId, m.Direction, m.SentAt })
               .HasDatabaseName("IX_GuestMessages_Guest_Direction_SentAt");

        builder.HasOne(m => m.Guest)
               .WithMany(u => u.GuestMessages)
               .HasForeignKey(m => m.GuestId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Reservation)
               .WithMany(r => r.GuestMessages)
               .HasForeignKey(m => m.ReservationId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
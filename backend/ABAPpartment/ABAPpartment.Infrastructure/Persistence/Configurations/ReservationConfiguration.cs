using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.ToTable("Apartments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.InternalCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.InternalCode).IsUnique().HasDatabaseName("UQ_Apartments_InternalCode");

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.AddressLine).IsRequired().HasMaxLength(300);
        builder.Property(a => a.District).IsRequired().HasMaxLength(100);
        builder.Property(a => a.BaseNightlyRate).HasColumnType("decimal(10,2)");
        builder.Property(a => a.FloorArea).HasColumnType("decimal(6,2)");
        builder.Property(a => a.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(a => a.Longitude).HasColumnType("decimal(9,6)");
        builder.Property(a => a.SmartLockCode).HasMaxLength(50);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(30).HasDefaultValue(ApartmentStatus.Active);

        builder.HasIndex(a => a.OwnerId).HasDatabaseName("IX_Apartments_OwnerId");
        builder.HasIndex(a => a.Status).HasDatabaseName("IX_Apartments_Status");

        builder.HasOne(a => a.Owner)
               .WithMany(u => u.OwnedApartments)
               .HasForeignKey(a => a.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Channel).IsRequired().HasMaxLength(50).HasDefaultValue(ReservationChannel.Direct);
        builder.Property(r => r.ExternalRef).HasMaxLength(100);
        builder.Property(r => r.NumGuests).IsRequired();
        builder.Property(r => r.TotalAmount).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(r => r.Currency).IsRequired().HasMaxLength(5).HasDefaultValue("EUR");
        builder.Property(r => r.Status).IsRequired().HasMaxLength(30).HasDefaultValue(ReservationStatus.Confirmed);
        builder.Property(r => r.CheckInMethod).HasMaxLength(30);

        builder.Property(r => r.CheckInDate).HasColumnType("date");
        builder.Property(r => r.CheckOutDate).HasColumnType("date");

        builder.Ignore(r => r.Nights);
        builder.Ignore(r => r.IsActive);

        builder.HasIndex(r => new { r.ApartmentId, r.CheckInDate, r.CheckOutDate })
               .HasDatabaseName("IX_Reservations_ApartmentId_Dates");
        builder.HasIndex(r => r.GuestId).HasDatabaseName("IX_Reservations_GuestId");
        builder.HasIndex(r => r.Status).HasDatabaseName("IX_Reservations_Status");

        builder.HasOne(r => r.Apartment)
               .WithMany(a => a.Reservations)
               .HasForeignKey(r => r.ApartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Guest)
               .WithMany(u => u.Reservations)
               .HasForeignKey(r => r.GuestId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
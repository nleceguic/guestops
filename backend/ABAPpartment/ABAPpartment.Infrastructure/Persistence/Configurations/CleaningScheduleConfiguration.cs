using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class CleaningScheduleConfiguration : IEntityTypeConfiguration<CleaningSchedule>
{
    public void Configure(EntityTypeBuilder<CleaningSchedule> builder)
    {
        builder.ToTable("CleaningSchedules");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ScheduledDate).HasColumnType("date").IsRequired();
        builder.Property(c => c.ScheduledTime).HasColumnType("time").IsRequired();
        builder.Property(c => c.Type).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Scheduled");
        builder.Property(c => c.Notes).HasMaxLength(2000);

        builder.HasIndex(c => new { c.ScheduledDate, c.Status })
               .HasDatabaseName("IX_CleaningSchedules_Date_Status");

        builder.HasOne(c => c.Apartment)
               .WithMany(a => a.CleaningSchedules)
               .HasForeignKey(c => c.ApartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Reservation)
               .WithMany()
               .HasForeignKey(c => c.ReservationId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedTo)
               .WithMany(u => u.CleaningSchedules)
               .HasForeignKey(c => c.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Category).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(4000);
        builder.Property(i => i.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Open");
        builder.Property(i => i.ZendeskTicketId).HasMaxLength(50);

        builder.HasIndex(i => i.ApartmentId)
               .HasDatabaseName("IX_Incidents_ApartmentId");

        builder.HasIndex(i => new { i.AssignedToId, i.Status })
               .HasDatabaseName("IX_Incidents_AssignedToId_Status");

        builder.HasOne(i => i.Apartment)
               .WithMany(a => a.Incidents)
               .HasForeignKey(i => i.ApartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Reservation)
               .WithMany(r => r.Incidents)
               .HasForeignKey(i => i.ReservationId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ReportedBy)
               .WithMany(u => u.ReportedIncidents)
               .HasForeignKey(i => i.ReportedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AssignedTo)
               .WithMany(u => u.AssignedIncidents)
               .HasForeignKey(i => i.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
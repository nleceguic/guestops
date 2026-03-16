using ABAPpartment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABAPpartment.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasMaxLength(30);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue(UserRole.Guest);

        builder.Property(u => u.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("es");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Ignore(u => u.FullName);

        builder.HasMany(u => u.OwnedApartments)
               .WithOne(a => a.Owner)
               .HasForeignKey(a => a.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReportedIncidents)
               .WithOne(i => i.ReportedBy)
               .HasForeignKey(i => i.ReportedById)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedIncidents)
               .WithOne(i => i.AssignedTo)
               .HasForeignKey(i => i.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.CleaningSchedules)
               .WithOne(c => c.AssignedTo)
               .HasForeignKey(c => c.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
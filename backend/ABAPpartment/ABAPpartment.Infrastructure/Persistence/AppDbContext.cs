using Microsoft.EntityFrameworkCore;
using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<GuestMessage> GuestMessages => Set<GuestMessage>();
    public DbSet<CleaningSchedule> CleaningSchedules => Set<CleaningSchedule>();
    public DbSet<OccupancyForecast> OccupancyForecasts => Set<OccupancyForecast>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
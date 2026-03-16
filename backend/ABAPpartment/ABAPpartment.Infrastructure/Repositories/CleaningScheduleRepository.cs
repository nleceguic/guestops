using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class CleaningScheduleRepository : ICleaningScheduleRepository
{
    private readonly AppDbContext _db;

    public CleaningScheduleRepository(AppDbContext db) => _db = db;

    public Task<CleaningSchedule?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.CleaningSchedules.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<CleaningSchedule?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => _db.CleaningSchedules
              .Include(c => c.Apartment)
              .Include(c => c.AssignedTo)
              .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<CleaningSchedule>> GetAllAsync(CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .AsNoTracking()
                    .OrderBy(c => c.ScheduledDate)
                    .ThenBy(c => c.ScheduledTime)
                    .ToListAsync(ct);

    public async Task<IEnumerable<CleaningSchedule>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.ApartmentId == apartmentId)
                    .AsNoTracking()
                    .OrderByDescending(c => c.ScheduledDate)
                    .ToListAsync(ct);

    public async Task<IEnumerable<CleaningSchedule>> GetByAssignedOperatorAsync(int operatorId, CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.AssignedToId == operatorId)
                    .AsNoTracking()
                    .OrderBy(c => c.ScheduledDate)
                    .ThenBy(c => c.ScheduledTime)
                    .ToListAsync(ct);

    public async Task<IEnumerable<CleaningSchedule>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.ScheduledDate == date)
                    .AsNoTracking()
                    .OrderBy(c => c.ScheduledTime)
                    .ToListAsync(ct);

    public async Task<IEnumerable<CleaningSchedule>> GetByStatusAsync(string status, CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.Status == status)
                    .AsNoTracking()
                    .OrderBy(c => c.ScheduledDate)
                    .ThenBy(c => c.ScheduledTime)
                    .ToListAsync(ct);

    public async Task<IEnumerable<CleaningSchedule>> GetByDateRangeAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
        => await _db.CleaningSchedules
                    .Include(c => c.Apartment)
                    .Include(c => c.AssignedTo)
                    .Where(c => c.ScheduledDate >= from && c.ScheduledDate <= to)
                    .AsNoTracking()
                    .OrderBy(c => c.ScheduledDate)
                    .ThenBy(c => c.ScheduledTime)
                    .ToListAsync(ct);

    public async Task AddAsync(CleaningSchedule schedule, CancellationToken ct = default)
        => await _db.CleaningSchedules.AddAsync(schedule, ct);

    public void Update(CleaningSchedule schedule)
        => _db.CleaningSchedules.Update(schedule);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
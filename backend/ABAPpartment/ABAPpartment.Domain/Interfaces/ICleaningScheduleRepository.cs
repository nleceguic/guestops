using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface ICleaningScheduleRepository
{
    Task<CleaningSchedule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CleaningSchedule?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<CleaningSchedule>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<CleaningSchedule>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<CleaningSchedule>> GetByAssignedOperatorAsync(int operatorId, CancellationToken ct = default);
    Task<IEnumerable<CleaningSchedule>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IEnumerable<CleaningSchedule>> GetByStatusAsync(string status, CancellationToken ct = default);

    Task<IEnumerable<CleaningSchedule>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

    Task AddAsync(CleaningSchedule schedule, CancellationToken ct = default);
    void Update(CleaningSchedule schedule);
    Task SaveChangesAsync(CancellationToken ct = default);
}
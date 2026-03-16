using ABAPpartment.Application.DTOs.Cleaning;

namespace ABAPpartment.Application.Interfaces;

public interface ICleaningScheduleService
{
    Task<CleaningScheduleDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<CleaningScheduleSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<CleaningScheduleSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<CleaningScheduleSummaryDto>> GetByOperatorAsync(int operatorId, CancellationToken ct = default);
    Task<IEnumerable<CleaningScheduleSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<DailyPlanningDto> GetDailyPlanningAsync(DateOnly date, CancellationToken ct = default);
    Task<CleaningScheduleDto> GenerateCheckoutCleaningAsync(int reservationId, CancellationToken ct = default);
    Task<CleaningScheduleDto> CreateAsync(CreateCleaningScheduleRequest request, CancellationToken ct = default);
    Task<CleaningScheduleDto> UpdateAsync(int id, UpdateCleaningScheduleRequest request, CancellationToken ct = default);
    Task<CleaningScheduleDto> UpdateStatusAsync(int id, UpdateCleaningStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
using ABAPpartment.Application.DTOs.Apartments;

namespace ABAPpartment.Application.Interfaces;

public interface IApartmentService
{
    Task<ApartmentDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ApartmentSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ApartmentSummaryDto>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<IEnumerable<ApartmentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<ApartmentMetricsDto> GetMetricsAsync(int apartmentId, CancellationToken ct = default);
    Task<ApartmentDto> CreateAsync(CreateApartmentRequest request, CancellationToken ct = default);
    Task<ApartmentDto> UpdateAsync(int id, UpdateApartmentRequest request, CancellationToken ct = default);
    Task<ApartmentDto> UpdateStatusAsync(int id, UpdateApartmentStatusRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
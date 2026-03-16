using ABAPpartment.Application.DTOs.Reservations;

namespace ABAPpartment.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ReservationSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ReservationSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<ReservationSummaryDto>> GetByGuestAsync(int guestId, CancellationToken ct = default);
    Task<ReservationDto> CreateAsync(int guestId, CreateReservationRequest request, CancellationToken ct = default);
    Task<ReservationDto> UpdateAsync(int id, UpdateReservationRequest request, CancellationToken ct = default);
    Task<ReservationDto> UpdateStatusAsync(int id, UpdateStatusRequest request, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
}
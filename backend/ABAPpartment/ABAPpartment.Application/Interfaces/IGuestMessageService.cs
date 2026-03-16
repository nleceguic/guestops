using ABAPpartment.Application.DTOs.GuestMessages;

namespace ABAPpartment.Application.Interfaces;

public interface IGuestMessageService
{
    Task<GuestMessageDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<GuestMessageDto>> GetByReservationAsync(int reservationId, CancellationToken ct = default);
    Task<IEnumerable<GuestMessageDto>> GetByGuestAsync(int guestId, CancellationToken ct = default);
    Task<IEnumerable<GuestMessageDto>> GetRecentAsync(int count = 20, CancellationToken ct = default);
    Task<IEnumerable<GuestMessageDto>> GetPendingHumanReplyAsync(CancellationToken ct = default);
    Task<ProcessMessageResult> ProcessInboundAsync(
        ProcessMessageRequest request,
        CancellationToken ct = default);
    Task<GuestMessageDto> SendManualReplyAsync(
        int reservationId,
        SendManualReplyRequest request,
        CancellationToken ct = default);
}
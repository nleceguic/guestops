using ABAPpartment.Application.DTOs.GuestMessages;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;

namespace ABAPpartment.Application.Services;

public class GuestMessageService : IGuestMessageService
{
    private readonly IGuestMessageRepository _messages;
    private readonly IReservationRepository _reservations;
    private readonly IUserRepository _users;
    private readonly IIncidentRepository _incidents;
    private readonly IAIAssistantService _ai;
    private const decimal AutoReplyThreshold = 70m;

    public GuestMessageService(
        IGuestMessageRepository messages,
        IReservationRepository reservations,
        IUserRepository users,
        IIncidentRepository incidents,
        IAIAssistantService ai)
    {
        _messages = messages;
        _reservations = reservations;
        _users = users;
        _incidents = incidents;
        _ai = ai;
    }

    public async Task<GuestMessageDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var msg = await _messages.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Mensaje {id} no encontrado.");
        return ToDto(msg);
    }

    public async Task<IEnumerable<GuestMessageDto>> GetByReservationAsync(int reservationId, CancellationToken ct = default)
        => (await _messages.GetByReservationAsync(reservationId, ct)).Select(ToDto);

    public async Task<IEnumerable<GuestMessageDto>> GetByGuestAsync(int guestId, CancellationToken ct = default)
        => (await _messages.GetByGuestAsync(guestId, ct)).Select(ToDto);

    public async Task<IEnumerable<GuestMessageDto>> GetRecentAsync(int count = 20, CancellationToken ct = default)
        => (await _messages.GetRecentAsync(count, ct)).Select(ToDto);

    public async Task<IEnumerable<GuestMessageDto>> GetPendingHumanReplyAsync(CancellationToken ct = default)
        => (await _messages.GetPendingHumanReplyAsync(ct)).Select(ToDto);

    public async Task<ProcessMessageResult> ProcessInboundAsync(
        ProcessMessageRequest req,
        CancellationToken ct = default)
    {
        if (!GuestMessageChannel.All.Contains(req.Channel))
            throw new ArgumentException($"Canal inválido: {req.Channel}.");

        var guest = await _users.GetByIdAsync(req.GuestId, ct)
            ?? throw new KeyNotFoundException($"Usuario {req.GuestId} no encontrado.");

        Reservation? reservation = null;
        if (req.ReservationId.HasValue)
        {
            reservation = await _reservations.GetByIdWithDetailsAsync(req.ReservationId.Value, ct)
                ?? throw new KeyNotFoundException($"Reserva {req.ReservationId} no encontrada.");
        }

        var inbound = new GuestMessage
        {
            ReservationId = req.ReservationId,
            GuestId = req.GuestId,
            Guest = guest,
            Channel = req.Channel,
            Direction = GuestMessageDirection.Inbound,
            Body = req.Body.Trim(),
            IsAutoReply = false,
            SentAt = DateTime.UtcNow,
        };

        await _messages.AddAsync(inbound, ct);
        await _messages.SaveChangesAsync(ct);

        GuestMessage? autoReplyMsg = null;
        bool incidentCreated = false;
        int? incidentId = null;
        AIAssistantResponse? aiResponse = null;

        if (reservation != null)
        {
            var context = BuildContext(reservation);
            aiResponse = await _ai.GetResponseAsync(req.Body, guest.Language, context, ct);

            if (aiResponse != null && aiResponse.Confidence >= AutoReplyThreshold)
            {
                autoReplyMsg = new GuestMessage
                {
                    ReservationId = req.ReservationId,
                    GuestId = req.GuestId,
                    Guest = guest,
                    Channel = req.Channel,
                    Direction = GuestMessageDirection.Outbound,
                    Body = aiResponse.Reply,
                    IsAutoReply = true,
                    AIConfidence = aiResponse.Confidence,
                    DetectedTopic = aiResponse.DetectedTopic,
                    SentAt = DateTime.UtcNow,
                };

                await _messages.AddAsync(autoReplyMsg, ct);

                if (aiResponse.ShouldCreateIncident)
                {
                    var incident = new Incident
                    {
                        ApartmentId = reservation.ApartmentId,
                        ReservationId = req.ReservationId,
                        ReportedById = req.GuestId,
                        Category = IncidentCategory.Maintenance,
                        Priority = IncidentPriority.High,
                        Title = $"Incidencia reportada por huésped vía {req.Channel}",
                        Description = req.Body.Trim(),
                        Status = IncidentStatus.Open,
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _incidents.AddAsync(incident, ct);
                    await _incidents.SaveChangesAsync(ct);

                    autoReplyMsg.IncidentId = incident.Id;
                    incidentCreated = true;
                    incidentId = incident.Id;
                }

                await _messages.SaveChangesAsync(ct);
            }
        }

        return new ProcessMessageResult(
            InboundMessage: ToDto(inbound),
            AutoReply: autoReplyMsg != null ? ToDto(autoReplyMsg) : null,
            WasAutoReplied: autoReplyMsg != null,
            IncidentCreated: incidentCreated,
            IncidentId: incidentId,
            DetectedTopic: aiResponse?.DetectedTopic ?? "unknown",
            AIConfidence: aiResponse?.Confidence
        );
    }

    public async Task<GuestMessageDto> SendManualReplyAsync(
        int reservationId,
        SendManualReplyRequest req,
        CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdWithDetailsAsync(reservationId, ct)
            ?? throw new KeyNotFoundException($"Reserva {reservationId} no encontrada.");

        var operator_ = await _users.GetByIdAsync(req.OperatorId, ct)
            ?? throw new KeyNotFoundException($"Usuario {req.OperatorId} no encontrado.");

        if (operator_.Role is not (UserRole.Operator or UserRole.Admin))
            throw new InvalidOperationException("Solo un Operator o Admin puede enviar respuestas manuales.");

        var reply = new GuestMessage
        {
            ReservationId = reservationId,
            GuestId = reservation.GuestId,
            Guest = reservation.Guest,
            Channel = GuestMessageChannel.Chat,
            Direction = GuestMessageDirection.Outbound,
            Body = req.Body.Trim(),
            IsAutoReply = false,
            SentAt = DateTime.UtcNow,
        };

        await _messages.AddAsync(reply, ct);
        await _messages.SaveChangesAsync(ct);

        return ToDto(reply);
    }

    private static AIAssistantContext BuildContext(Reservation r) => new(
        ApartmentName: r.Apartment.Name,
        ApartmentAddress: r.Apartment.AddressLine,
        ApartmentDistrict: r.Apartment.District,
        SmartLockCode: r.Apartment.SmartLockCode,
        CheckInDate: r.CheckInDate,
        CheckOutDate: r.CheckOutDate,
        GuestFirstName: r.Guest.FirstName
    );

    private static GuestMessageDto ToDto(GuestMessage m) => new(
        m.Id,
        m.ReservationId,
        m.GuestId,
        m.Guest.FullName,
        m.Channel,
        m.Direction,
        m.Body,
        m.IsAutoReply,
        m.AIConfidence,
        m.DetectedTopic,
        m.IncidentId,
        m.SentAt
    );
}

public static class GuestMessageChannel
{
    public const string WhatsApp = "WhatsApp";
    public const string Email = "Email";
    public const string Chat = "Chat";
    public const string Phone = "Phone";

    public static readonly IReadOnlyList<string> All =
        new[] { WhatsApp, Email, Chat, Phone };
}

public static class GuestMessageDirection
{
    public const string Inbound = "Inbound";
    public const string Outbound = "Outbound";
}
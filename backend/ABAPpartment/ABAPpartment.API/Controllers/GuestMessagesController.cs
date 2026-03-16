using ABAPpartment.Application.DTOs.GuestMessages;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GuestMessagesController : ControllerBase
{
    private readonly IGuestMessageService _service;

    public GuestMessagesController(IGuestMessageService service) => _service = service;

    /// <summary>Últimos 20 mensajes entrantes — bandeja de entrada del Operations Dashboard.</summary>
    [HttpGet("recent")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<GuestMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int count = 20,
        CancellationToken ct = default)
        => Ok(await _service.GetRecentAsync(count, ct));

    /// <summary>Mensajes inbound que todavía no tienen respuesta — requieren atención humana.</summary>
    [HttpGet("pending-reply")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<GuestMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReply(CancellationToken ct)
        => Ok(await _service.GetPendingHumanReplyAsync(ct));

    /// <summary>Historial completo de mensajes de una reserva (conversación).</summary>
    [HttpGet("reservation/{reservationId:int}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<GuestMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByReservation(int reservationId, CancellationToken ct)
        => Ok(await _service.GetByReservationAsync(reservationId, ct));

    /// <summary>Todos los mensajes de un huésped concreto.</summary>
    [HttpGet("guest/{guestId:int}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<GuestMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGuest(int guestId, CancellationToken ct)
        => Ok(await _service.GetByGuestAsync(guestId, ct));

    /// <summary>Procesa un mensaje entrante de un huésped.</summary>
    [HttpPost("inbound")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProcessMessageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessInbound(
        [FromBody] ProcessMessageRequest request,
        CancellationToken ct)
        => Ok(await _service.ProcessInboundAsync(request, ct));

    /// <summary>Envía una respuesta manual de un operador al huésped.</summary>
    [HttpPost("reservation/{reservationId:int}/reply")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(GuestMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendManualReply(
        int reservationId,
        [FromBody] SendManualReplyRequest request,
        CancellationToken ct)
    {
        var result = await _service.SendManualReplyAsync(reservationId, request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
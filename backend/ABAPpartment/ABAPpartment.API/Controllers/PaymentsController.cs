using ABAPpartment.Application.DTOs.Payments;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Operator")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service) => _service = service;

    /// <summary>Detalle de un pago.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    /// <summary>Lista todos los pagos pendientes de confirmar.</summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<PaymentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
        => Ok(await _service.GetPendingAsync(ct));

    /// <summary>Filtra pagos por estado.</summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
        => Ok(await _service.GetByStatusAsync(status, ct));

    /// <summary>Resumen financiero completo de una reserva.</summary>
    [HttpGet("reservation/{reservationId:int}")]
    [ProducesResponseType(typeof(ReservationPaymentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReservation(int reservationId, CancellationToken ct)
        => Ok(await _service.GetByReservationAsync(reservationId, ct));

    /// <summary>Registra un nuevo pago. Los pagos en Cash se confirman automáticamente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Confirma un pago pendiente — lo marca como Completed.</summary>
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        int id,
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken ct)
        => Ok(await _service.ConfirmAsync(id, request, ct));

    /// <summary>Marca un pago pendiente como fallido.</summary>
    [HttpPost("{id:int}/fail")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(
        int id,
        [FromBody] FailPaymentRequest request,
        CancellationToken ct)
        => Ok(await _service.FailAsync(id, request, ct));

    /// <summary>Procesa un reembolso parcial o total sobre un pago completado.</summary>
    [HttpPost("{id:int}/refund")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refund(
        int id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken ct)
        => Ok(await _service.RefundAsync(id, request, ct));
}
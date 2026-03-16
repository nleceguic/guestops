using System.Security.Claims;
using ABAPpartment.Application.DTOs.Reservations;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

    /// <summary>Lista todas las reservas.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<ReservationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>Devuelve el detalle de una reserva por ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Reservas del huésped autenticado.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<ReservationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var guestId = GetCurrentUserId();
        var result = await _service.GetByGuestAsync(guestId, ct);
        return Ok(result);
    }

    /// <summary>Reservas de un apartamento concreto.</summary>
    [HttpGet("apartment/{apartmentId:int}")]
    [Authorize(Roles = "Admin,Operator,Owner")]
    [ProducesResponseType(typeof(IEnumerable<ReservationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByApartment(int apartmentId, CancellationToken ct)
    {
        var result = await _service.GetByApartmentAsync(apartmentId, ct);
        return Ok(result);
    }

    /// <summary>Crea una nueva reserva. El precio se calcula automáticamente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken ct)
    {
        var guestId = GetCurrentUserId();
        var result = await _service.CreateAsync(guestId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza fechas, huéspedes o método de check-in.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReservationRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Cambia el estado de la reserva.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Cancela una reserva (soft cancel, no borra el registro).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await _service.CancelAsync(id, ct);
        return NoContent();
    }
 
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                 ?? throw new UnauthorizedAccessException("Token inválido.");
        return int.Parse(claim.Value);
    }
}
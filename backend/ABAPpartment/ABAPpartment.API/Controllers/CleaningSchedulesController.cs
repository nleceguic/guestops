using ABAPpartment.Application.DTOs.Cleaning;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Operator")]
[Produces("application/json")]
public class CleaningSchedulesController : ControllerBase
{
    private readonly ICleaningScheduleService _service;

    public CleaningSchedulesController(ICleaningScheduleService service) => _service = service;

    /// <summary>Lista todas las limpiezas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CleaningScheduleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Detalle completo de una limpieza.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CleaningScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    /// <summary>Operations Dashboard: planning del día con resumen de estados.</summary>
    [HttpGet("daily/{date}")]
    public async Task<IActionResult> GetDailyPlanning(string date, CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out var parsedDate))
            return BadRequest("Formato de fecha inválido. Usa yyyy-MM-dd. Ejemplo: 2026-04-10");

        return Ok(await _service.GetDailyPlanningAsync(parsedDate, ct));
    }

    /// <summary>Historial de limpiezas de un apartamento.</summary>
    [HttpGet("apartment/{apartmentId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningScheduleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByApartment(int apartmentId, CancellationToken ct)
        => Ok(await _service.GetByApartmentAsync(apartmentId, ct));

    /// <summary>Limpiezas asignadas a un operario concreto.</summary>
    [HttpGet("operator/{operatorId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningScheduleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOperator(int operatorId, CancellationToken ct)
        => Ok(await _service.GetByOperatorAsync(operatorId, ct));

    /// <summary>Filtra limpiezas por estado.</summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<CleaningScheduleSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
        => Ok(await _service.GetByStatusAsync(status, ct));

    /// <summary>Crea una nueva limpieza manualmente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CleaningScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCleaningScheduleRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Genera automáticamente la limpieza de checkout para una reserva.</summary>
    [HttpPost("generate-checkout/{reservationId:int}")]
    [ProducesResponseType(typeof(CleaningScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateCheckout(int reservationId, CancellationToken ct)
    {
        var result = await _service.GenerateCheckoutCleaningAsync(reservationId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza fecha, hora o asignación de una limpieza.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CleaningScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCleaningScheduleRequest request,
        CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    /// <summary>Cambia el estado de una limpieza.</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(CleaningScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateCleaningStatusRequest request,
        CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(id, request, ct));

    /// <summary>Descarta una limpieza (soft delete → Skipped).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
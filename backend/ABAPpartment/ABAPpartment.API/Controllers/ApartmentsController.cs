using System.Security.Claims;
using ABAPpartment.Application.DTOs.Apartments;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ApartmentsController : ControllerBase
{
    private readonly IApartmentService _service;

    public ApartmentsController(IApartmentService service) => _service = service;

    /// <summary>Lista todos los apartamentos.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<ApartmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>Detalle completo de un apartamento.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Apartamentos del propietario autenticado.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Owner,Admin")]
    [ProducesResponseType(typeof(IEnumerable<ApartmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var ownerId = GetCurrentUserId();
        var result = await _service.GetByOwnerAsync(ownerId, ct);
        return Ok(result);
    }

    /// <summary>Filtra apartamentos por estado (Active, Inactive, UnderMaintenance).</summary>
    [HttpGet("by-status/{status}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<ApartmentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
    {
        var result = await _service.GetByStatusAsync(status, ct);
        return Ok(result);
    }

    /// <summary>Owner Dashboard: métricas de rendimiento del apartamento.</summary>
    [HttpGet("{id:int}/metrics")]
    [Authorize(Roles = "Owner,Admin,Operator")]
    [ProducesResponseType(typeof(ApartmentMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetrics(int id, CancellationToken ct)
    {
        var result = await _service.GetMetricsAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Crea un nuevo apartamento.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateApartmentRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza los datos de un apartamento.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(ApartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateApartmentRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Cambia el estado del apartamento.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(ApartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateApartmentStatusRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Elimina (soft delete) un apartamento marcándolo como Inactive.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
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
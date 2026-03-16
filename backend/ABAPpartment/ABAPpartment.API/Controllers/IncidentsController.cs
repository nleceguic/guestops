using System.Security.Claims;
using ABAPpartment.Application.DTOs.Incidents;
using ABAPpartment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABAPpartment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _service;

    public IncidentsController(IIncidentService service) => _service = service;

    /// <summary>Lista todas las incidencias.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    /// <summary>Operations Dashboard: resumen global con críticas, sin asignar y recientes.</summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IncidentDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(ct));

    /// <summary>Detalle completo de una incidencia.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    /// <summary>Incidencias de un apartamento concreto.</summary>
    [HttpGet("apartment/{apartmentId:int}")]
    [Authorize(Roles = "Admin,Operator,Owner")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByApartment(int apartmentId, CancellationToken ct)
        => Ok(await _service.GetByApartmentAsync(apartmentId, ct));

    /// <summary>Incidencias asignadas a un operario.</summary>
    [HttpGet("operator/{operatorId:int}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOperator(int operatorId, CancellationToken ct)
        => Ok(await _service.GetByOperatorAsync(operatorId, ct));

    /// <summary>Filtra por estado.</summary>
    [HttpGet("by-status/{status}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
        => Ok(await _service.GetByStatusAsync(status, ct));

    /// <summary>Filtra por categoría.</summary>
    [HttpGet("by-category/{category}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken ct)
        => Ok(await _service.GetByCategoryAsync(category, ct));

    /// <summary>Filtra por prioridad.</summary>
    [HttpGet("by-priority/{priority}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IEnumerable<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPriority(string priority, CancellationToken ct)
        => Ok(await _service.GetByPriorityAsync(priority, ct));

    /// <summary>Abre una nueva incidencia. Se auto-asigna al primer operador disponible.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateIncidentRequest request,
        CancellationToken ct)
    {
        var reportedById = GetCurrentUserId();
        var result = await _service.CreateAsync(reportedById, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza título, descripción, prioridad o categoría.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateIncidentRequest request,
        CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    /// <summary>Asigna manualmente un operario.</summary>
    [HttpPatch("{id:int}/assign")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        int id,
        [FromBody] AssignIncidentRequest request,
        CancellationToken ct)
        => Ok(await _service.AssignAsync(id, request, ct));

    /// <summary>Cambia el estado.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateIncidentStatusRequest request,
        CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(id, request, ct));

    /// <summary>Asocia un ticket de Zendesk a la incidencia.</summary>
    [HttpPatch("{id:int}/zendesk")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncZendesk(
        int id,
        [FromBody] SyncZendeskRequest request,
        CancellationToken ct)
        => Ok(await _service.SyncZendeskAsync(id, request, ct));

    /// <summary>Cierra una incidencia (soft delete → Closed).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
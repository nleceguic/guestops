using ABAPpartment.Application.DTOs.Incidents;

namespace ABAPpartment.Application.Interfaces;

public interface IIncidentService
{
    Task<IncidentDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetByOperatorAsync(int operatorId, CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<IEnumerable<IncidentSummaryDto>> GetByPriorityAsync(string priority, CancellationToken ct = default);
    Task<IncidentDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<IncidentDto> CreateAsync(int reportedById, CreateIncidentRequest request, CancellationToken ct = default);
    Task<IncidentDto> UpdateAsync(int id, UpdateIncidentRequest request, CancellationToken ct = default);
    Task<IncidentDto> AssignAsync(int id, AssignIncidentRequest request, CancellationToken ct = default);
    Task<IncidentDto> UpdateStatusAsync(int id, UpdateIncidentStatusRequest request, CancellationToken ct = default);
    Task<IncidentDto> SyncZendeskAsync(int id, SyncZendeskRequest request, CancellationToken ct = default);
    Task CancelAsync(int id, CancellationToken ct = default);
}
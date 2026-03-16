using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Incident?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetByAssignedOperatorAsync(int operatorId, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetByPriorityAsync(string priority, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<IEnumerable<Incident>> GetOpenByApartmentAsync(int apartmentId, CancellationToken ct = default);

    Task<IEnumerable<Incident>> GetActiveAsync(CancellationToken ct = default);

    Task<int> CountOpenByApartmentAsync(int apartmentId, CancellationToken ct = default);

    Task AddAsync(Incident incident, CancellationToken ct = default);
    void Update(Incident incident);
    Task SaveChangesAsync(CancellationToken ct = default);
}
using ABAPpartment.Domain.Entities;

namespace ABAPpartment.Domain.Interfaces;

public interface IApartmentRepository
{
    Task<Apartment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Apartment?> GetByIdWithOwnerAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Apartment>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Apartment>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<IEnumerable<Apartment>> GetByStatusAsync(string status, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string internalCode, int? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Apartment apartment, CancellationToken ct = default);
    void Update(Apartment apartment);
    void Delete(Apartment apartment);
    Task SaveChangesAsync(CancellationToken ct = default);
}
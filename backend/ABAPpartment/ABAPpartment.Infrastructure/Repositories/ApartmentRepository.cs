using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class ApartmentRepository : IApartmentRepository
{
    private readonly AppDbContext _db;

    public ApartmentRepository(AppDbContext db) => _db = db;

    public Task<Apartment?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Apartments.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Apartment?> GetByIdWithOwnerAsync(int id, CancellationToken ct = default)
        => _db.Apartments
              .Include(a => a.Owner)
              .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Apartment>> GetAllAsync(CancellationToken ct = default)
        => await _db.Apartments
                    .Include(a => a.Owner)
                    .AsNoTracking()
                    .OrderBy(a => a.InternalCode)
                    .ToListAsync(ct);

    public async Task<IEnumerable<Apartment>> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
        => await _db.Apartments
                    .Include(a => a.Owner)
                    .Where(a => a.OwnerId == ownerId)
                    .AsNoTracking()
                    .OrderBy(a => a.InternalCode)
                    .ToListAsync(ct);

    public async Task<IEnumerable<Apartment>> GetByStatusAsync(string status, CancellationToken ct = default)
        => await _db.Apartments
                    .Include(a => a.Owner)
                    .Where(a => a.Status == status)
                    .AsNoTracking()
                    .OrderBy(a => a.InternalCode)
                    .ToListAsync(ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => _db.Apartments.AnyAsync(a => a.Id == id, ct);

    public Task<bool> ExistsByCodeAsync(string internalCode, int? excludeId = null, CancellationToken ct = default)
        => _db.Apartments.AnyAsync(a =>
            a.InternalCode == internalCode.ToUpperInvariant()
            && (excludeId == null || a.Id != excludeId), ct);

    public async Task AddAsync(Apartment apartment, CancellationToken ct = default)
        => await _db.Apartments.AddAsync(apartment, ct);

    public void Update(Apartment apartment)
        => _db.Apartments.Update(apartment);

    public void Delete(Apartment apartment)
        => _db.Apartments.Remove(apartment);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
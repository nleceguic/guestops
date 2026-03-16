using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class IncidentRepository : IIncidentRepository
{
    private readonly AppDbContext _db;

    public IncidentRepository(AppDbContext db) => _db = db;

    private IQueryable<Incident> WithDetails()
        => _db.Incidents
              .Include(i => i.Apartment)
              .Include(i => i.ReportedBy)
              .Include(i => i.AssignedTo);

    public Task<Incident?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Incidents.FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<Incident?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IEnumerable<Incident>> GetAllAsync(CancellationToken ct = default)
        => await WithDetails()
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.ApartmentId == apartmentId)
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetByAssignedOperatorAsync(int operatorId, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.AssignedToId == operatorId)
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetByStatusAsync(string status, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.Status == status)
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetByPriorityAsync(string priority, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.Priority == priority)
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetByCategoryAsync(string category, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.Category == category)
                 .AsNoTracking()
                 .OrderByDescending(i => i.CreatedAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetOpenByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.ApartmentId == apartmentId
                          && IncidentStatus.IsActive(i.Status))
                 .AsNoTracking()
                 .ToListAsync(ct);

    public async Task<IEnumerable<Incident>> GetActiveAsync(CancellationToken ct = default)
        => await WithDetails()
                 .Where(i => i.Status == IncidentStatus.Open
                          || i.Status == IncidentStatus.InProgress)
                 .AsNoTracking()
                 .OrderBy(i => i.Priority == IncidentPriority.Critical ? 0 :
                               i.Priority == IncidentPriority.High ? 1 :
                               i.Priority == IncidentPriority.Medium ? 2 : 3)
                 .ThenBy(i => i.CreatedAt)
                 .ToListAsync(ct);

    public Task<int> CountOpenByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => _db.Incidents.CountAsync(i =>
            i.ApartmentId == apartmentId &&
            (i.Status == IncidentStatus.Open || i.Status == IncidentStatus.InProgress), ct);

    public async Task AddAsync(Incident incident, CancellationToken ct = default)
        => await _db.Incidents.AddAsync(incident, ct);

    public void Update(Incident incident)
        => _db.Incidents.Update(incident);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
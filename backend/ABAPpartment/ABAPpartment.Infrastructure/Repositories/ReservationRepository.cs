using ABAPpartment.Domain.Entities;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABAPpartment.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _db;

    public ReservationRepository(AppDbContext db) => _db = db;

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Reservation?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => _db.Reservations
              .Include(r => r.Apartment)
              .Include(r => r.Guest)
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Reservation>> GetAllAsync(CancellationToken ct = default)
        => await _db.Reservations
                    .Include(r => r.Apartment)
                    .Include(r => r.Guest)
                    .AsNoTracking()
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync(ct);

    public async Task<IEnumerable<Reservation>> GetByApartmentAsync(int apartmentId, CancellationToken ct = default)
        => await _db.Reservations
                    .Include(r => r.Apartment)
                    .Include(r => r.Guest)
                    .Where(r => r.ApartmentId == apartmentId)
                    .AsNoTracking()
                    .OrderByDescending(r => r.CheckInDate)
                    .ToListAsync(ct);

    public async Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId, CancellationToken ct = default)
        => await _db.Reservations
                    .Include(r => r.Apartment)
                    .Include(r => r.Guest)
                    .Where(r => r.GuestId == guestId)
                    .AsNoTracking()
                    .OrderByDescending(r => r.CheckInDate)
                    .ToListAsync(ct);

    public Task<bool> HasOverlapAsync(
        int apartmentId,
        DateOnly checkIn,
        DateOnly checkOut,
        int? excludeReservationId = null,
        CancellationToken ct = default)
        => _db.Reservations.AnyAsync(r =>
            r.ApartmentId == apartmentId
            && (excludeReservationId == null || r.Id != excludeReservationId)
            && r.Status != ReservationStatus.Cancelled
            && r.CheckInDate < checkOut
            && r.CheckOutDate > checkIn,
            ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
        => await _db.Reservations.AddAsync(reservation, ct);

    public void Update(Reservation reservation)
        => _db.Reservations.Update(reservation);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
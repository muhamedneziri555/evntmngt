using Microsoft.EntityFrameworkCore;
using EventMngt.Data;
using EventMngt.Models;

namespace EventMngt.Repositories;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _dbSet
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .ToListAsync();
    }

    public override async Task<Event?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        return await _dbSet
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Where(e => e.StartDate > DateTime.UtcNow && e.IsActive)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsByOrganizerAsync(string organizerId)
    {
        return await _dbSet
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Where(e => e.StartDate >= startDate && e.EndDate <= endDate && e.IsActive)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetRegistrationCountAsync(int eventId)
    {
        return await _context.Registrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);
    }

    public async Task<bool> IsEventFullAsync(int eventId)
    {
        var ev = await _dbSet.FindAsync(eventId);
        if (ev == null) return false;

        var registrationCount = await GetRegistrationCountAsync(eventId);
        return registrationCount >= ev.Capacity;
    }
} 
using EventMngt.Models;

namespace EventMngt.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<IEnumerable<Event>> GetEventsByOrganizerAsync(string organizerId);
    Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetRegistrationCountAsync(int eventId);
    Task<bool> IsEventFullAsync(int eventId);
} 
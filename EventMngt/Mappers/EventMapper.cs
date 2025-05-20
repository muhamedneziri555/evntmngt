using EventMngt.DTOs;
using EventMngt.Models;

namespace EventMngt.Mappers;

public static class EventMapper
{
    public static EventDTO ToDTO(Event ev, string organizerName, int currentRegistrations, string categoryName)
    {
        return new EventDTO
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Location = ev.Location,
            Capacity = ev.Capacity,
            OrganizerName = organizerName,
            CurrentRegistrations = currentRegistrations,
            IsActive = ev.IsActive,
            CategoryId = ev.CategoryId,
            CategoryName = categoryName
        };
    }

    public static Event ToModel(CreateEventDTO dto, string organizerId)
    {
        return new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Location = dto.Location,
            Capacity = dto.Capacity,
            OrganizerId = organizerId,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public static void UpdateModel(Event ev, UpdateEventDTO dto)
    {
        ev.Title = dto.Title;
        ev.Description = dto.Description;
        ev.StartDate = dto.StartDate;
        ev.EndDate = dto.EndDate;
        ev.Location = dto.Location;
        ev.Capacity = dto.Capacity;
        ev.IsActive = dto.IsActive;
        ev.CategoryId = dto.CategoryId;
        ev.UpdatedAt = DateTime.UtcNow;
    }
} 
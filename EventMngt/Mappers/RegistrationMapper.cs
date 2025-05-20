using EventMngt.DTOs;
using EventMngt.Models;

namespace EventMngt.Mappers;

public static class RegistrationMapper
{
    public static RegistrationDTO ToDTO(Registration registration)
    {
        return new RegistrationDTO
        {
            Id = registration.Id,
            EventId = registration.EventId,
            UserId = registration.UserId,
            Status = registration.Status.ToString(),
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        };
    }

    public static Registration ToModel(CreateRegistrationDTO dto, string userId)
    {
        return new Registration
        {
            EventId = dto.EventId,
            UserId = userId,
            Status = RegistrationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateModel(Registration registration, UpdateRegistrationDTO dto)
    {
        if (Enum.TryParse<RegistrationStatus>(dto.Status, out var status))
        {
            registration.Status = status;
            registration.UpdatedAt = DateTime.UtcNow;
        }
    }
} 
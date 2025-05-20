using EventMngt.DTOs;
using EventMngt.Models;

namespace EventMngt.Mappers;

public static class UserMapper
{
    public static UserDTO ToDTO(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return new UserDTO
        {
            Id = user.Id ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public static UserProfileDTO ToProfileDTO(User user)
    {
        return new UserProfileDTO
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public static User ToModel(RegisterDTO dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };
    }
} 
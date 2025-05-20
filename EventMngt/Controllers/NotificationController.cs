using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventMngt.DTOs;
using EventMngt.Models;
using EventMngt.Repositories;
using FluentValidation;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IValidator<NotificationDTO> _validator;

    public NotificationController(
        IRepository<Notification> notificationRepository,
        IValidator<NotificationDTO> validator)
    {
        _notificationRepository = notificationRepository;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var notifications = await _notificationRepository.FindAsync(n => n.UserId == userId);
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (notification.UserId != userId)
        {
            return Forbid();
        }

        return Ok(notification);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NotificationDTO dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (notification.UserId != userId)
        {
            return Forbid();
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        await _notificationRepository.UpdateAsync(notification);
        return Ok(notification);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var notifications = await _notificationRepository.FindAsync(n => 
            n.UserId == userId && !n.IsRead);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (notification.UserId != userId)
        {
            return Forbid();
        }

        await _notificationRepository.DeleteAsync(notification);
        return NoContent();
    }
} 
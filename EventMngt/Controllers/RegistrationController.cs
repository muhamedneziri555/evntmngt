using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventMngt.DTOs;
using EventMngt.Models;
using EventMngt.Repositories;
using EventMngt.Validators;
using FluentValidation;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegistrationController : ControllerBase
{
    private readonly IRepository<Registration> _registrationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IValidator<CreateRegistrationDTO> _createValidator;
    private readonly IValidator<UpdateRegistrationDTO> _updateValidator;

    public RegistrationController(
        IRepository<Registration> registrationRepository,
        IEventRepository eventRepository,
        IValidator<CreateRegistrationDTO> createValidator,
        IValidator<UpdateRegistrationDTO> updateValidator)
    {
        _registrationRepository = registrationRepository;
        _eventRepository = eventRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var registrations = await _registrationRepository.FindAsync(r => r.UserId == userId);
        return Ok(registrations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var registration = await _registrationRepository.GetByIdAsync(id);
        if (registration == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (registration.UserId != userId)
        {
            return Forbid();
        }

        return Ok(registration);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRegistrationDTO dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var ev = await _eventRepository.GetByIdAsync(dto.EventId);
        if (ev == null)
        {
            return NotFound("Event not found");
        }

        if (!ev.IsActive)
        {
            return BadRequest("Event is not active");
        }

        if (await _eventRepository.IsEventFullAsync(dto.EventId))
        {
            return BadRequest("Event is full");
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var existingRegistration = await _registrationRepository.FindAsync(r => 
            r.EventId == dto.EventId && r.UserId == userId);

        if (existingRegistration.Any())
        {
            return BadRequest("You are already registered for this event");
        }

        var registration = new Registration
        {
            EventId = dto.EventId,
            UserId = userId!,
            Status = RegistrationStatus.Pending
        };

        await _registrationRepository.AddAsync(registration);
        return CreatedAtAction(nameof(GetById), new { id = registration.Id }, registration);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRegistrationDTO dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var registration = await _registrationRepository.GetByIdAsync(id);
        if (registration == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (registration.UserId != userId)
        {
            return Forbid();
        }

        if (!Enum.TryParse<RegistrationStatus>(dto.Status, out var status))
        {
            return BadRequest("Invalid status value");
        }

        registration.Status = status;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration);
        return Ok(registration);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var registration = await _registrationRepository.GetByIdAsync(id);
        if (registration == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (registration.UserId != userId)
        {
            return Forbid();
        }

        registration.Status = RegistrationStatus.Cancelled;
        registration.UpdatedAt = DateTime.UtcNow;

        await _registrationRepository.UpdateAsync(registration);
        return NoContent();
    }
} 
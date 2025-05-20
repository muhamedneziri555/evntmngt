using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventMngt.DTOs;
using EventMngt.Models;
using EventMngt.Repositories;
using EventMngt.Validators;
using FluentValidation;
using EventMngt.Mappers;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly IValidator<CreateEventDTO> _createValidator;
    private readonly IValidator<UpdateEventDTO> _updateValidator;

    public EventController(
        IEventRepository eventRepository,
        IValidator<CreateEventDTO> createValidator,
        IValidator<UpdateEventDTO> updateValidator)
    {
        _eventRepository = eventRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _eventRepository.GetAllAsync();
        var eventDtos = new List<EventDTO>();
        
        foreach (var ev in events)
        {
            var organizerName = ev.Organizer?.UserName ?? "Unknown";
            var categoryName = ev.Category?.Name ?? "Uncategorized";
            var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
            eventDtos.Add(EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName));
        }
        
        return Ok(eventDtos);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var events = await _eventRepository.GetUpcomingEventsAsync();
        var eventDtos = new List<EventDTO>();
        
        foreach (var ev in events)
        {
            var organizerName = ev.Organizer?.UserName ?? "Unknown";
            var categoryName = ev.Category?.Name ?? "Uncategorized";
            var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
            eventDtos.Add(EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName));
        }
        
        return Ok(eventDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        if (ev == null)
        {
            return NotFound();
        }

        var organizerName = ev.Organizer?.UserName ?? "Unknown";
        var categoryName = ev.Category?.Name ?? "Uncategorized";
        var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
        var eventDto = EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName);
        
        return Ok(eventDto);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDTO dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var ev = EventMapper.ToModel(dto, userId);
        await _eventRepository.AddAsync(ev);

        var organizerName = ev.Organizer?.UserName ?? "Unknown";
        var categoryName = ev.Category?.Name ?? "Uncategorized";
        var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
        var eventDto = EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName);
        
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, eventDto);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEventDTO dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var ev = await _eventRepository.GetByIdAsync(id);
        if (ev == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (ev.OrganizerId != userId)
        {
            return Forbid();
        }

        EventMapper.UpdateModel(ev, dto);
        await _eventRepository.UpdateAsync(ev);

        var organizerName = ev.Organizer?.UserName ?? "Unknown";
        var categoryName = ev.Category?.Name ?? "Uncategorized";
        var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
        var eventDto = EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName);
        
        return Ok(eventDto);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        if (ev == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (ev.OrganizerId != userId)
        {
            return Forbid();
        }

        await _eventRepository.DeleteAsync(ev);
        return NoContent();
    }

    [HttpGet("organizer/{organizerId}")]
    public async Task<IActionResult> GetByOrganizer(string organizerId)
    {
        var events = await _eventRepository.GetEventsByOrganizerAsync(organizerId);
        var eventDtos = new List<EventDTO>();
        
        foreach (var ev in events)
        {
            var organizerName = ev.Organizer?.UserName ?? "Unknown";
            var categoryName = ev.Category?.Name ?? "Uncategorized";
            var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
            eventDtos.Add(EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName));
        }
        
        return Ok(eventDtos);
    }

    [HttpGet("date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var events = await _eventRepository.GetEventsByDateRangeAsync(startDate, endDate);
        var eventDtos = new List<EventDTO>();
        
        foreach (var ev in events)
        {
            var organizerName = ev.Organizer?.UserName ?? "Unknown";
            var categoryName = ev.Category?.Name ?? "Uncategorized";
            var currentRegistrations = await _eventRepository.GetRegistrationCountAsync(ev.Id);
            eventDtos.Add(EventMapper.ToDTO(ev, organizerName, currentRegistrations, categoryName));
        }
        
        return Ok(eventDtos);
    }
} 
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
public class FeedbackController : ControllerBase
{
    private readonly IRepository<Feedback> _feedbackRepository;
    private readonly IRepository<Registration> _registrationRepository;
    private readonly IValidator<FeedbackDTO> _validator;

    public FeedbackController(
        IRepository<Feedback> feedbackRepository,
        IRepository<Registration> registrationRepository,
        IValidator<FeedbackDTO> validator)
    {
        _feedbackRepository = feedbackRepository;
        _registrationRepository = registrationRepository;
        _validator = validator;
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventFeedback(int eventId)
    {
        var feedback = await _feedbackRepository.FindAsync(f => f.EventId == eventId);
        return Ok(feedback);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        if (feedback == null)
        {
            return NotFound();
        }
        return Ok(feedback);
    }

    [HttpPost]
    public async Task<IActionResult> Create(FeedbackDTO dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var registration = await _registrationRepository.FindAsync(r => 
            r.EventId == dto.EventId && 
            r.UserId == userId && 
            r.Status == RegistrationStatus.Confirmed);

        if (!registration.Any())
        {
            return BadRequest("You must be a confirmed participant to provide feedback");
        }

        var feedback = new Feedback
        {
            EventId = dto.EventId,
            UserId = userId!,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackRepository.AddAsync(feedback);
        return CreatedAtAction(nameof(GetById), new { id = feedback.Id }, feedback);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FeedbackDTO dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var feedback = await _feedbackRepository.GetByIdAsync(id);
        if (feedback == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (feedback.UserId != userId)
        {
            return Forbid();
        }

        feedback.Rating = dto.Rating;
        feedback.Comment = dto.Comment;
        feedback.UpdatedAt = DateTime.UtcNow;

        await _feedbackRepository.UpdateAsync(feedback);
        return Ok(feedback);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);
        if (feedback == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (feedback.UserId != userId)
        {
            return Forbid();
        }

        await _feedbackRepository.DeleteAsync(feedback);
        return NoContent();
    }

    [HttpGet("my-feedback")]
    public async Task<IActionResult> GetMyFeedback()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var feedback = await _feedbackRepository.FindAsync(f => f.UserId == userId);
        return Ok(feedback);
    }
} 
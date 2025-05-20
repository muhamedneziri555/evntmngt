using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventMngt.Data;
using EventMngt.Models;
using EventMngt.Services;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public RegistrationsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // POST: api/registrations
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] int eventId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null || !ev.IsActive) return NotFound("Event not found or inactive.");
        if (ev.Capacity <= _context.Registrations.Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed))
            return BadRequest("Event is full.");
        if (await _context.Registrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId && r.Status == RegistrationStatus.Confirmed))
            return BadRequest("Already registered.");
        var reg = new Registration { EventId = eventId, UserId = userId, Status = RegistrationStatus.Confirmed };
        _context.Registrations.Add(reg);
        await _context.SaveChangesAsync();
        NotificationService.Notify(userId, $"You have registered for event #{eventId}.");
        return Ok(new { message = "Registered successfully." });
    }

    // GET: api/registrations/myevents
    [HttpGet("myevents")]
    [Authorize]
    public async Task<IActionResult> MyEvents()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();
        var regs = await _context.Registrations
            .Include(r => r.Event)
            .Where(r => r.UserId == userId && r.Status == RegistrationStatus.Confirmed)
            .ToListAsync();
        return Ok(regs);
    }

    // GET: api/registrations/event/{eventId}
    [HttpGet("event/{eventId}")]
    [Authorize]
    public async Task<IActionResult> EventRegistrations(int eventId)
    {
        var userId = _userManager.GetUserId(User);
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null) return NotFound();
        if (ev.OrganizerId != userId) return Forbid();
        var regs = await _context.Registrations
            .Include(r => r.User)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed)
            .ToListAsync();
        return Ok(regs);
    }

    // DELETE: api/registrations/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);
        var reg = await _context.Registrations.Include(r => r.Event).FirstOrDefaultAsync(r => r.Id == id);
        if (reg == null) return NotFound();
        if (reg.UserId != userId && reg.Event.OrganizerId != userId) return Forbid();
        reg.Status = RegistrationStatus.Cancelled;
        await _context.SaveChangesAsync();
        NotificationService.Notify(userId, $"You have cancelled your registration for event #{reg.EventId}.");
        return Ok(new { message = "Registration cancelled." });
    }
} 
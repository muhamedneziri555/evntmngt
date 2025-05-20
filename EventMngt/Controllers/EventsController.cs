using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventMngt.Data;
using EventMngt.Models;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public EventsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/events
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events.Include(e => e.Organizer).ToListAsync();
        return Ok(events);
    }

    // GET: api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null) return NotFound();
        return Ok(ev);
    }

    // POST: api/events
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEvent([FromBody] Event model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();
        model.OrganizerId = userId;
        model.CreatedAt = DateTime.UtcNow;
        _context.Events.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = model.Id }, model);
    }

    // PUT: api/events/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event model)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (ev.OrganizerId != userId) return Forbid();
        ev.Title = model.Title;
        ev.Description = model.Description;
        ev.StartDate = model.StartDate;
        ev.EndDate = model.EndDate;
        ev.Location = model.Location;
        ev.Capacity = model.Capacity;
        ev.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/events/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        var userId = _userManager.GetUserId(User);
        if (ev.OrganizerId != userId) return Forbid();
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }
} 
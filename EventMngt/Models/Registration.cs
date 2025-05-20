namespace EventMngt.Models;

public class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
} 
namespace EventMngt.DTOs;

public class RegistrationDTO
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRegistrationDTO
{
    public int EventId { get; set; }
}

public class UpdateRegistrationDTO
{
    public string Status { get; set; } = string.Empty;
} 
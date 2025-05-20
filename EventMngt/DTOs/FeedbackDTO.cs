namespace EventMngt.DTOs;

public class FeedbackDTO
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFeedbackDTO
{
    public int EventId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class UpdateFeedbackDTO
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
} 
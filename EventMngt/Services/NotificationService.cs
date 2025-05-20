namespace EventMngt.Services;

public static class NotificationService
{
    public static void Notify(string userId, string message)
    {
        // In a real app, send email/SMS/etc. Here, just log.
        Console.WriteLine($"Notification to {userId}: {message}");
    }
} 
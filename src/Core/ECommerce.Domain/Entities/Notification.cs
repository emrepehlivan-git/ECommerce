namespace ECommerce.Domain.Entities;

public sealed class Notification : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRead { get; private set; }
    public Dictionary<string, object>? Data { get; private set; }

    private Notification() { }

    public Notification(string title, string message, string type, Guid? userId = null, Dictionary<string, object>? data = null)
    {
        Title = title;
        Message = message;
        Type = type;
        UserId = userId;
        Data = data;
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
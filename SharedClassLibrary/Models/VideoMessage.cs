using Video.Messaging.App.Models.Enums;

namespace Video.Messaging.App.Models;

public class VideoMessage
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsIncoming { get; set; }
    public VideoMessageStatus Status { get; set; }
}

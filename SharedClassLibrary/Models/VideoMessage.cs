namespace Video.Messaging.App.Models;
public class VideoMessage
{
    public Guid Id { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsIncoming { get; set; }
}
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

public enum VideoMessageStatus
{
    Recording,
    Recorded,
    Playing,
    Paused,
    Stopped
}

public enum MessageType
{
    Incoming,
    Outgoing
}

public class PlaybackState
{
    public bool IsPlaying { get; set; }
    public bool IsPaused { get; set; }
    public double CurrentTime { get; set; }
    public double Duration { get; set; }
    public double PlaybackSpeed { get; set; } = 1.0;
    public bool IsMuted { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
}


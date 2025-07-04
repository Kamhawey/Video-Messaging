namespace Video.Messaging.App.Models;


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


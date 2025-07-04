using Video.Messaging.App.Models;

namespace Video.Messaging.App.Utils;

public static class VideoMessageValidator
{
    public static bool IsValidVideoMessage(VideoMessage message)
    {
        if (message == null) return false;
        if (string.IsNullOrWhiteSpace(message.SenderName)) return false;
        if (message.Duration.TotalMinutes > Constants.MAX_RECORDING_DURATION_MINUTES) return false;
        if (message.CreatedAt > DateTime.Now) return false;

        return true;
    }

    public static bool IsValidVideoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out _) ||
               url.StartsWith("data:video/") ||
               url.StartsWith("blob:");
    }

    public static bool IsValidPlaybackSpeed(double speed)
    {
        return Constants.PLAYBACK_SPEEDS.Contains(speed);
    }
}
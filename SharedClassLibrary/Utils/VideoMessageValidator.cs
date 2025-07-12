using Video.Messaging.App.Models;

namespace Video.Messaging.App.Utils;

public static class VideoMessageValidator
{
    public static bool IsValidVideoMessage(VideoMessage message)
    {
        if (message == null) return false;

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
namespace Video.Messaging.App.Utils;

public static class Constants
{
    public const int DEFAULT_SKIP_SECONDS = 10;
    public const double DEFAULT_PLAYBACK_SPEED = 1.0;
    public const int MAX_RECORDING_DURATION_MINUTES = 5;
    public const int VIDEO_QUALITY_WIDTH = 1280;
    public const int VIDEO_QUALITY_HEIGHT = 720;

    public static readonly double[] PLAYBACK_SPEEDS = { 0.5, 1.0, 1.25, 1.5, 2.0 };

    public static class MediaConstraints
    {
        public const string VIDEO_CODEC = "video/webm;codecs=vp9";
        public const string AUDIO_CODEC = "audio/webm;codecs=opus";
        public const int VIDEO_BITRATE = 2500000; // 2.5 Mbps
        public const int AUDIO_BITRATE = 128000;  // 128 kbps
    }
}

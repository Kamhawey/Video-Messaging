using Video.Messaging.App.Models;

namespace Video.Messaging.App.Services;


public interface IVideoPlayerService
{
    Task<bool> PlayAsync(string videoUrl);
    Task<bool> PauseAsync();
    Task<bool> StopAsync();
    Task<bool> ReplayAsync();
    Task<bool> SeekAsync(double seconds);
    Task<bool> SetPlaybackSpeedAsync(double speed);
    Task<bool> SkipForwardAsync(int seconds = 10);
    Task<bool> SkipBackwardAsync(int seconds = 10);
    Task<PlaybackState> GetPlaybackStateAsync();
    event EventHandler<PlaybackState>? PlaybackStateChanged;
}

public class VideoPlayerService : IVideoPlayerService
{
    private readonly IJSVideoService _jsVideoService;
    private PlaybackState _currentState = new();
    public event EventHandler<PlaybackState>? PlaybackStateChanged;

    public VideoPlayerService(IJSVideoService jsVideoService)
    {
        _jsVideoService = jsVideoService;
    }

    public async Task<bool> PlayAsync(string videoUrl)
    {
        var success = await _jsVideoService.PlayVideoAsync("videoPlayer", videoUrl);
        if (success)
        {
            _currentState.IsPlaying = true;
            _currentState.IsPaused = false;
            _currentState.VideoUrl = videoUrl;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> PauseAsync()
    {
        var success = await _jsVideoService.PauseVideoAsync("videoPlayer");
        if (success)
        {
            _currentState.IsPlaying = false;
            _currentState.IsPaused = true;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> StopAsync()
    {
        var success = await _jsVideoService.PauseVideoAsync("videoPlayer");
        if (success)
        {
            _currentState.IsPlaying = false;
            _currentState.IsPaused = false;
            _currentState.CurrentTime = 0;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> ReplayAsync()
    {
        var success = await _jsVideoService.SetVideoCurrentTimeAsync("videoPlayer", 0);
        if (success)
        {
            success = await _jsVideoService.PlayVideoAsync("videoPlayer", _currentState.VideoUrl);
            if (success)
            {
                _currentState.IsPlaying = true;
                _currentState.IsPaused = false;
                _currentState.CurrentTime = 0;
                PlaybackStateChanged?.Invoke(this, _currentState);
            }
        }
        return success;
    }

    public async Task<bool> SeekAsync(double seconds)
    {
        var success = await _jsVideoService.SetVideoCurrentTimeAsync("videoPlayer", seconds);
        if (success)
        {
            _currentState.CurrentTime = seconds;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> SetPlaybackSpeedAsync(double speed)
    {
        var success = await _jsVideoService.SetVideoPlaybackRateAsync("videoPlayer", speed);
        if (success)
        {
            _currentState.PlaybackSpeed = speed;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> SkipForwardAsync(int seconds = 10)
    {
        var currentTime = await _jsVideoService.GetVideoCurrentTimeAsync("videoPlayer");
        var duration = await _jsVideoService.GetVideoDurationAsync("videoPlayer");
        var newTime = Math.Min(currentTime + seconds, duration);
        var success = await _jsVideoService.SetVideoCurrentTimeAsync("videoPlayer", newTime);
        if (success)
        {
            _currentState.CurrentTime = newTime;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<bool> SkipBackwardAsync(int seconds = 10)
    {
        var currentTime = await _jsVideoService.GetVideoCurrentTimeAsync("videoPlayer");
        var newTime = Math.Max(0, currentTime - seconds);
        var success = await _jsVideoService.SetVideoCurrentTimeAsync("videoPlayer", newTime);
        if (success)
        {
            _currentState.CurrentTime = newTime;
            PlaybackStateChanged?.Invoke(this, _currentState);
        }
        return success;
    }

    public async Task<PlaybackState> GetPlaybackStateAsync()
    {
        _currentState.CurrentTime = await _jsVideoService.GetVideoCurrentTimeAsync("videoPlayer");
        _currentState.Duration = await _jsVideoService.GetVideoDurationAsync("videoPlayer");
        return _currentState;
    }
}
